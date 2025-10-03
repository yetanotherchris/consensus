using System.Text;
using ConsensusAgent.Utilities;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for building prompts for different consensus scenarios
/// </summary>
public class PromptBuilder : IPromptBuilder
{
    public string BuildNextRoundPrompt(string originalPrompt, List<(string model, string response)> previousResponses, string voteResult, int currentRound)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine($"Round {currentRound + 1} of consensus building.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Original question:");
        promptBuilder.AppendLine(originalPrompt);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Previous round responses summary:");
        
        foreach (var (model, response) in previousResponses)
        {
            promptBuilder.AppendLine($"- {model}: {TextHelper.Truncate(response, 150)}");
        }
        
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"Vote result: {voteResult}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Based on the above, provide your refined response that builds on the consensus while addressing any gaps or disagreements.");

        return promptBuilder.ToString();
    }

    public string BuildVotingPrompt(List<(string model, string response)> responses)
    {
        var votePrompt = new StringBuilder();
        votePrompt.AppendLine("You are a voting moderator. Review the following responses and identify:");
        votePrompt.AppendLine("1. Common themes or agreements");
        votePrompt.AppendLine("2. Key disagreements or differences");
        votePrompt.AppendLine("3. Which response(s) appear most comprehensive or accurate");
        votePrompt.AppendLine();
        votePrompt.AppendLine("Responses:");
        votePrompt.AppendLine();

        for (int i = 0; i < responses.Count; i++)
        {
            votePrompt.AppendLine($"Response {i + 1} ({responses[i].model}):");
            votePrompt.AppendLine(TextHelper.Truncate(responses[i].response, 500));
            votePrompt.AppendLine();
        }

        votePrompt.AppendLine("Provide a brief summary (2-3 sentences) of the voting result.");

        return votePrompt.ToString();
    }

    public string BuildFinalConsensusPrompt(string originalPrompt, List<string> conversationHistory, int maxRounds)
    {
        var consensusPrompt = new StringBuilder();
        consensusPrompt.AppendLine("You are synthesizing a final consensus response based on a multi-round discussion.");
        consensusPrompt.AppendLine();
        consensusPrompt.AppendLine("Original prompt:");
        consensusPrompt.AppendLine(originalPrompt);
        consensusPrompt.AppendLine();
        consensusPrompt.AppendLine("Discussion history (abbreviated):");
        
        // Include abbreviated history to avoid token limits
        int itemsToShow = Math.Min(10, conversationHistory.Count);
        for (int i = conversationHistory.Count - itemsToShow; i < conversationHistory.Count; i++)
        {
            consensusPrompt.AppendLine(TextHelper.Truncate(conversationHistory[i], 200));
        }
        
        consensusPrompt.AppendLine();
        consensusPrompt.AppendLine("Generate a final, comprehensive consensus response in Markdown format that:");
        consensusPrompt.AppendLine("1. Addresses the original prompt thoroughly");
        consensusPrompt.AppendLine("2. Incorporates the strongest points from the discussion");
        consensusPrompt.AppendLine("3. Resolves any disagreements with the most evidence-based position");
        consensusPrompt.AppendLine("4. Is well-structured and easy to read");

        return consensusPrompt.ToString();
    }
}
