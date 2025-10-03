namespace ConsensusAgent.Models;

/// <summary>
/// Represents the level of consensus achieved among the models
/// </summary>
public enum ConsensusLevel
{
    /// <summary>
    /// 80%+ agreement among models
    /// </summary>
    StrongConsensus,
    
    /// <summary>
    /// 60-80% agreement among models
    /// </summary>
    ModerateConsensus,
    
    /// <summary>
    /// 40-60% agreement among models
    /// </summary>
    WeakConsensus,
    
    /// <summary>
    /// Less than 40% agreement among models
    /// </summary>
    Conflicted
}
