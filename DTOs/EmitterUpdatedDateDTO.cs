using System.Text.Json.Serialization;

namespace tree_form_API.Models
{
    public class EmitterUpdatedDateDTO
    {
        public Guid Id { get; set; }

        [JsonPropertyName("updateddate")]
        public DateTime UpdatedDate { get; set; }
    }
}
