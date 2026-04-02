namespace Shared.Model.Enums
{
    /// <summary>
    /// Priority levels for target list items
    /// </summary>
    public enum TargetPriority
    {
        /// <summary>
        /// Critical priority - must observe
        /// </summary>
        Critical = 1,
        
        /// <summary>
        /// High priority - very important
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Medium-High priority
        /// </summary>
        MediumHigh = 3,
        
        /// <summary>
        /// Medium priority - normal importance
        /// </summary>
        Medium = 4,
        
        /// <summary>
        /// Medium-Low priority
        /// </summary>
        MediumLow = 5,
        
        /// <summary>
        /// Low priority - nice to have
        /// </summary>
        Low = 6,
        
        /// <summary>
        /// Very low priority - if time permits
        /// </summary>
        VeryLow = 7
    }
}
