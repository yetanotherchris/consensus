namespace Consensus.Core;

internal static class Prompts
{
    public static readonly string InitialSystemPrompt =
        ResourceHelper.GetString("Consensus.Core.Resources.InitialSystemPrompt.txt");
    public static readonly string FollowupSystemPrompt =
        ResourceHelper.GetString("Consensus.Core.Resources.FollowupSystemPrompt.txt");
    public static readonly string ChangeSummarySystemPrompt =
        ResourceHelper.GetString("Consensus.Core.Resources.ChangeSummarySystemPrompt.txt");
    public static readonly string FinalChangesSummaryPrompt =
        ResourceHelper.GetString("Consensus.Core.Resources.FinalChangesSummaryPrompt.txt");
}
