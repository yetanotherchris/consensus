namespace Consensus.Core;

public interface ITemplate
{
    string QueryingTemplate { get; }
    string ModelSummaryTemplate { get; }
    string AnswerTemplate { get; }
    string ResponseTemplate { get; }
}
