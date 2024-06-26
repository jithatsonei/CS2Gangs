using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace plugin;

public class CS2GangsConfig : BasePluginConfig
{
    [JsonPropertyName("DBConnectionString")]
    public string? DBConnectionString { get; set; }
    [JsonPropertyName("DebugPermission")]
    public string? DebugPermission { get; set; }

    [JsonPropertyName("GangsCreationPrice")]
    public int GangCreationPrice { get; set; }
    [JsonPropertyName("GangInviteExpireMinutes")]
    public int GangInviteExpireMinutes { get; set; }
    [JsonPropertyName("VIPTier1Group")]
    public string? VIPTier1Group { get; set; }
    [JsonPropertyName("VIPTier2Group")]
    public string? VIPTier2Group { get; set; }
    [JsonPropertyName("VIPTier3Group")]
    public string? VIPTier3Group { get; set; }
    [JsonPropertyName("VIPTier4Group")]
    public string? VIPTier4Group { get; set; }
    [JsonPropertyName("CreditsDeliveryInterval")]
    public int CreditsDeliveryInterval { get; set; }
}