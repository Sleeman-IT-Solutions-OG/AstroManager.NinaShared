namespace Shared.Model.Enums;

/// <summary>
/// Defines how mosaic panels should be ordered for shooting
/// </summary>
public enum MosaicPanelOrderingMethod
{
    /// <summary>
    /// User manually defines the shooting order for each panel
    /// Uses the ShootingOrder property on each panel
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// Automatically order panels by observability window, shortest first
    /// Shoots panels with the least observable time first
    /// Use case: Panels near horizon or targets with limited observation time
    /// </summary>
    AutoMinObservability = 1,
    
    /// <summary>
    /// Automatically order panels by observability window, longest first
    /// Shoots panels with the most observable time first
    /// Use case: Save difficult panels for later in the session
    /// </summary>
    AutoMaxObservability = 2
}
