namespace Consensus.Models;

/// <summary>
/// Represents a specific viewpoint in a disagreement
/// </summary>
public class DissentingView
{
    /// <summary>
    /// Name of the model holding this view
    /// </summary>
    public required string ModelName { get; set; }
    
    /// <summary>
    /// The position or stance taken by this model
    /// </summary>
    public required string Position { get; set; }
    
    /// <summary>
    /// Reasoning behind this position
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Represents a point of disagreement among models
/// </summary>
public class Disagreement
{
    /// <summary>
    /// The topic or aspect where models disagree
    /// </summary>
    public required string Topic { get; set; }
    
    /// <summary>
    /// The different views expressed by models
    /// </summary>
    public List<DissentingView> Views { get; set; } = new();
    
    /// <summary>
    /// Whether this disagreement reflects legitimate theoretical differences
    /// (as opposed to errors or misunderstandings)
    /// </summary>
    public bool IsLegitimateTheoretical { get; set; }
}
