using System.Text.Json.Serialization;

namespace tree_form_API.Models
{
    public class PlatformUpdatedByDTO
    {
        public Guid Id { get; set; }

        [JsonPropertyName("updatedby")]
        public Guid UpdatedBy { get; set; }
    }
}
