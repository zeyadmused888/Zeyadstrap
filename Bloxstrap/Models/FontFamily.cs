namespace Bloxstrap.Models
{
    public class FontFamily
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("loadStrategy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LoadStrategy { get; set; }

        [JsonPropertyName("faces")]
        public IEnumerable<FontFace> Faces { get; set; } = null!;
    }
}
