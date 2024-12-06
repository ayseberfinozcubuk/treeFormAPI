using System.Text.Json.Serialization;

namespace tree_form_API.Models
{
    public class EmitterUpdatedByDTO
    {
        public Guid Id { get; set; }

        [JsonPropertyName("updatedby")]
        public Guid UpdatedBy { get; set; }
    }
}
