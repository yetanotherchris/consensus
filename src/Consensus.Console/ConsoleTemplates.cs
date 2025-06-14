namespace Consensus.Console;

using Consensus.Core;

internal sealed class ConsoleTemplates : ITemplate
{
    public string QueryingTemplate => ResourceHelper.GetString("Consensus.Console.Resources.QueryingTemplate.md");
    public string ModelSummaryTemplate => ResourceHelper.GetString("Consensus.Console.Resources.ModelSummaryTemplate.md");
    public string InitialAnswerTemplate => ResourceHelper.GetString("Consensus.Console.Resources.InitialAnswerTemplate.md");
    public string AnswerTemplate => ResourceHelper.GetString("Consensus.Console.Resources.AnswerTemplate.md");
    public string ResponseTemplate => ResourceHelper.GetString("Consensus.Console.Resources.ResponseTemplate.md");
}
