using System.Text.Json.Serialization;

namespace multi_login.Models;

public class TokenDTO
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
