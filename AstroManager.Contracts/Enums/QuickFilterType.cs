namespace Shared.Model.Enums
{
    /// <summary>
    /// Enum to distinguish between different types of quick filters
    /// </summary>
    public enum QuickFilterType
    {
        /// <summary>
        /// Filters for Deep Sky Object searches
        /// </summary>
        DeepSkyObject = 0,
        
        /// <summary>
        /// Filters for Target searches
        /// </summary>
        Target = 1,

        /// <summary>
        /// Filters for Community Target searches
        /// </summary>
        CommunityTarget = 2
    }
}
