namespace Consensus;

internal static class Prompts
{
    public static readonly string InitialSystemPrompt =
        ResourceHelper.GetString("Consensus.Resources.InitialSystemPrompt.txt");
    public static readonly string FollowupSystemPrompt =
        ResourceHelper.GetString("Consensus.Resources.FollowupSystemPrompt.txt");
    public static readonly string ChangeSummarySystemPrompt =
        ResourceHelper.GetString("Consensus.Resources.ChangeSummarySystemPrompt.txt");
    public static readonly string FinalChangesSummaryPrompt =
        ResourceHelper.GetString("Consensus.Resources.FinalChangesSummaryPrompt.txt");
}
