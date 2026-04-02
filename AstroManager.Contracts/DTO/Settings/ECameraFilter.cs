using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Settings
{
    [JsonConverter(typeof(JsonStringEnumConverter<ECameraFilter>))]
    public enum ECameraFilter
    {
        L, R, G, B, Ha, Oiii, Sii
    }
}
