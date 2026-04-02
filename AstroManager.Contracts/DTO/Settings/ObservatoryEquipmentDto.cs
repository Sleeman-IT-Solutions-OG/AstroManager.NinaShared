using System;
using System.Collections.Generic;

namespace Shared.Model.DTO.Settings
{
    public class ObservatoryEquipmentDto
    {
        public Guid ObservatoryId { get; set; }
        public List<EquipmentDto> AssignedEquipment { get; set; } = new();
    }
}
