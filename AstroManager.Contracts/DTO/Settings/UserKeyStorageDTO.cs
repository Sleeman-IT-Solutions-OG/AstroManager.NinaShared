using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings
{
    /// <summary>
    /// DTO for UserKeyStorage entity
    /// </summary>
    public class UserKeyStorageDTO
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The key for the stored value
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Key { get; set; }
        
        /// <summary>
        /// The stored value
        /// </summary>
        [Required]
        public string Value { get; set; }
    }
}
