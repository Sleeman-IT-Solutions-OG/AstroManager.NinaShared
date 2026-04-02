namespace Shared.Model.Enums;

/// <summary>
/// Defines how mosaic panels should be shot
/// </summary>
public enum MosaicShootingStrategy
{
    /// <summary>
    /// Rotate through all panels, distributing imaging time evenly across all panels
    /// Example: P1→P2→P3→P1→P2→P3...
    /// Benefit: All panels progress together, balanced completion
    /// </summary>
    Parallel = 0,
    
    /// <summary>
    /// Complete all imaging goals for one panel before moving to the next
    /// Example: Complete P1 fully, then P2 fully, then P3 fully
    /// Benefit: Get complete data for some panels early
    /// </summary>
    Sequential = 1
}
