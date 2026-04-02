namespace Shared.Model.Enums;

/// <summary>
/// Pattern for shooting filter sequences during imaging sessions
/// </summary>
public enum FilterShootingPattern
{
    /// <summary>
    /// Cycle through all filters continuously (e.g., R→G→B→R→G→B...)
    /// </summary>
    Loop,
    
    /// <summary>
    /// Take a batch of exposures per filter before switching (e.g., RRR→GGG→BBB...)
    /// </summary>
    Batch,
    
    /// <summary>
    /// Complete all exposures for one filter before moving to next (e.g., RRRR...→GGGG...→BBBB...)
    /// </summary>
    Complete
}
