using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Settings
{
    [JsonConverter(typeof(JsonStringEnumConverter<EManualFilterChangeSource>))]
    public enum EManualFilterChangeSource
    {
        ManualFilterWheelInNina,
        DirectInputInAstroManager
    }
}
