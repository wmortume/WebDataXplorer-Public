using System.Text.Json.Serialization;

namespace WebDataXplorer.Server.Models
{
    public class InventoryQuery //uses JsonPropertyName to map properties from PascalCase to snake_case
    {
        [JsonPropertyName("location")]
        public required string Location { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("keyword")]
        public string? Keyword { get; set; }

        [JsonPropertyName("selective_search")]
        public bool? SelectiveSearch { get; set; }
    }
}
