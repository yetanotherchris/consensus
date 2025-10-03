using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text.RegularExpressions;
using Consensus.Logging;
using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for managing AI agents and executing queries
/// </summary>
public class AgentService : IAgentService
{
    private readonly SimpleLogger _logger;
    private readonly Dictionary<string, (AIAgent agent, AgentThread thread)> _modelContexts = new();

    public AgentService(SimpleLogger logger)
    {
        _logger = logger;
    }

    public async Task InitializeAgentsAsync(string[] models, string apiEndpoint, string apiKey)
    {
        _logger.LogInformation("Initializing {0} agent contexts", models.Length);
        
        foreach (string model in models)
        {
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(apiEndpoint) }
            );
            var chatClient = openAIClient.GetChatClient(model);
            AIAgent agent = chatClient.CreateAIAgent();
            AgentThread thread = agent.GetNewThread();
            
            _modelContexts[model] = (agent, thread);
        }
        
        _logger.LogInformation("Successfully initialized {0} agent contexts", _modelContexts.Count);
        
        await Task.CompletedTask;
    }

    public async Task<ModelResponse> QueryModelWithResponseAsync(string model, string prompt, CancellationToken cancellationToken = default)
    {
        if (!_modelContexts.TryGetValue(model, out var context))
        {
            throw new InvalidOperationException($"Agent for model '{model}' not initialized. Call InitializeAgentsAsync first.");
        }

        var (agent, thread) = context;
        var startTime = DateTime.UtcNow;
        
        // Create a task for the agent query
        var queryTask = Task.Run(async () =>
        {
            var response = await agent.RunAsync(prompt, thread, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }, cancellationToken);
        
        // Wait for either the query to complete or cancellation
        var completedTask = await Task.WhenAny(
            queryTask,
            Task.Delay(Timeout.Infinite, cancellationToken)
        );
        
        // If the delay task won (cancellation occurred), throw
        if (completedTask != queryTask)
        {
            _logger.LogWarning("Query to {0} was cancelled by timeout", model);
            throw new OperationCanceledException($"Query to {model} timed out");
        }
        
        // Get the raw response
        string rawResponse = await queryTask;
        
        // Parse into ModelResponse
        return ParseModelResponse(model, rawResponse, startTime);
    }

    private ModelResponse ParseModelResponse(string modelName, string rawResponse, DateTime timestamp)
    {
        // Try to extract reasoning and confidence from the response
        string answer = rawResponse;
        string reasoning = string.Empty;
        double confidence = 0.0;

        // Look for reasoning section (various possible formats)
        var reasoningMatch = Regex.Match(rawResponse, @"(?:Reasoning|My reasoning|Step-by-step):\s*(.+?)(?=(?:Confidence|$))", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (reasoningMatch.Success)
        {
            reasoning = reasoningMatch.Groups[1].Value.Trim();
            // Remove reasoning from answer
            answer = rawResponse.Replace(reasoningMatch.Value, "").Trim();
        }

        // Look for confidence score - try XML format first, then fallback to other patterns
        // Pattern 0 (PREFERRED): XML format like <confidence>0.85</confidence>
        var xmlConfidenceMatch = Regex.Match(rawResponse, @"<confidence>([\d.]+)</confidence>", 
            RegexOptions.IgnoreCase);
        
        if (xmlConfidenceMatch.Success && double.TryParse(xmlConfidenceMatch.Groups[1].Value, out double xmlConfValue))
        {
            // If value is between 0 and 1, use it directly; if between 0 and 100, convert to 0-1 range
            if (xmlConfValue >= 0 && xmlConfValue <= 1)
            {
                confidence = xmlConfValue;
            }
            else if (xmlConfValue > 1 && xmlConfValue <= 100)
            {
                confidence = xmlConfValue / 100.0;
            }
            
            // Remove XML confidence tag from answer
            answer = rawResponse.Replace(xmlConfidenceMatch.Value, "").Trim();
        }
        else
        {
            // Fallback to legacy patterns for backward compatibility
            Match confidenceMatch;
            
            // Pattern 1: "Confidence: 85%" or "Confidence level: 85"
            confidenceMatch = Regex.Match(rawResponse, @"(?:Confidence|Confidence level):\s*(\d+)%?", 
                RegexOptions.IgnoreCase);
            
            // Pattern 2: Numbered section like "3. Your confidence level (0-100%)\n\n95%" or "### 3. Confidence Level\n\n**95%**"
            if (!confidenceMatch.Success)
            {
                confidenceMatch = Regex.Match(rawResponse, 
                    @"(?:^|\n)\s*(?:\d+\.|###)\s*(?:Your\s+)?[Cc]onfidence\s+[Ll]evel[^\n]*\n+\s*\*{0,2}(\d+)%?\*{0,2}", 
                    RegexOptions.Multiline);
            }
            
            // Pattern 3: Standalone percentage near the end of response (like "**95%**" or "85%" on its own line)
            if (!confidenceMatch.Success)
            {
                // Look for standalone percentage in the last 500 characters
                var endSection = rawResponse.Length > 500 ? rawResponse.Substring(rawResponse.Length - 500) : rawResponse;
                confidenceMatch = Regex.Match(endSection, 
                    @"(?:^|\n)\s*\*{0,2}(\d{1,3})%\*{0,2}\s*(?:\n|$)", 
                    RegexOptions.Multiline);
            }
            
            // Pattern 4: Embedded in text like "confidence (80%)" or "with 85% confidence"
            if (!confidenceMatch.Success)
            {
                confidenceMatch = Regex.Match(rawResponse, 
                    @"(?:confidence|confident)(?:\s+is|\s+level)?[:\s(]+(\d+)%", 
                    RegexOptions.IgnoreCase);
            }
            
            if (confidenceMatch.Success && int.TryParse(confidenceMatch.Groups[1].Value, out int confValue))
            {
                // Validate confidence is in reasonable range (0-100)
                if (confValue >= 0 && confValue <= 100)
                {
                    confidence = confValue / 100.0;
                    // Remove confidence from answer
                    answer = rawResponse.Replace(confidenceMatch.Value, "").Trim();
                }
            }
        }

        // Look for summary in XML format: <summary>...</summary>
        string summary = "No summary provided by model";
        var summaryMatch = Regex.Match(rawResponse, @"<summary>(.+?)</summary>", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        if (summaryMatch.Success)
        {
            summary = summaryMatch.Groups[1].Value.Trim();
            // Remove summary tag from answer
            answer = answer.Replace(summaryMatch.Value, "").Trim();
        }

        return new ModelResponse
        {
            ModelName = modelName,
            Answer = answer,
            Reasoning = reasoning,
            ConfidenceScore = confidence,
            Summary = summary,
            Timestamp = timestamp,
            TokensUsed = 0 // Not currently tracked
        };
    }

    public async Task<string> QueryModelOneOffAsync(string model, string prompt, string apiEndpoint, string apiKey, CancellationToken cancellationToken = default)
    {
        var queryTask = Task.Run(async () =>
        {
            // Create OpenAI client configured for OpenRouter
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(apiEndpoint) }
            );
            
            // Get the chat client and create an AI Agent using the Microsoft Agent Framework
            var chatClient = openAIClient.GetChatClient(model);
            AIAgent agent = chatClient.CreateAIAgent();

            // Use AgentThread for conversation state
            AgentThread thread = agent.GetNewThread();
            
            // Run the agent with the prompt
            var response = await agent.RunAsync(prompt, thread, cancellationToken: cancellationToken);
            
            return response.Text ?? string.Empty;
        }, cancellationToken);
        
        // Wait for either completion or cancellation
        var completedTask = await Task.WhenAny(
            queryTask,
            Task.Delay(Timeout.Infinite, cancellationToken)
        );
        
        if (completedTask != queryTask)
        {
            throw new OperationCanceledException($"Query to {model} timed out");
        }
        
        return await queryTask;
    }
}
