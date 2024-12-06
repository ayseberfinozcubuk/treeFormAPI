using System.Text.Json.Serialization;

namespace tree_form_API.Models
{
    public class PlatformUpdatedDateDTO
    {
        public Guid Id { get; set; }

        [JsonPropertyName("updateddate")]
        public DateTime UpdatedDate { get; set; }
    }
}
