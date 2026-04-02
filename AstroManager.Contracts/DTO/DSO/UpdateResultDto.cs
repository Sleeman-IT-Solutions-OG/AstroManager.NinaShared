using System;

namespace Shared.Model.DTO.DSO
{
    /// <summary>
    /// Data Transfer Object for operation results that include a count of updated items
    /// </summary>
    public class UpdateResultDto
    {
        /// <summary>
        /// Gets or sets the number of items that were updated
        /// </summary>
        public int UpdatedCount { get; set; }
    }
}
