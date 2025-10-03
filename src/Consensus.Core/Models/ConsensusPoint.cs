namespace Consensus.Models;

/// <summary>
/// Represents a point of agreement among models
/// </summary>
public class ConsensusPoint
{
    /// <summary>
    /// The specific point or statement that models agree on
    /// </summary>
    public required string Point { get; set; }
    
    /// <summary>
    /// Number of models that support this point
    /// </summary>
    public int SupportingModels { get; set; }
    
    /// <summary>
    /// Names of the models that support this point
    /// </summary>
    public List<string> ModelNames { get; set; } = new();
}
