using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using ConsensusAgent.Logging;

namespace ConsensusAgent.Services;

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

    public async Task<string> QueryModelAsync(string model, string prompt, CancellationToken cancellationToken = default)
    {
        if (!_modelContexts.TryGetValue(model, out var context))
        {
            throw new InvalidOperationException($"Agent for model '{model}' not initialized. Call InitializeAgentsAsync first.");
        }

        var (agent, thread) = context;
        
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
        
        // Return the result
        return await queryTask;
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
