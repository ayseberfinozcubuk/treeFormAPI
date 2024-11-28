using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using tree_form_API.Models.Interfaces;

namespace tree_form_API.Models
{
    public class Platform
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string PlatformName { get; set; } = string.Empty; // Platform Adı
        public double? Latitude { get; set; }  // Enlem
        public double? Longitude { get; set; } // Boylam
        public string PlatformType { get; set; } = string.Empty; // Platform Tipi
        public string? PennantOrTailOrPlateNumber { get; set; } // Bayrak/ Kuyruk/ Plaka Numarası
        public string PlatformCategory { get; set; } = string.Empty; // Platform Kategorisi
        public string? UpdatedBy { get; set; }
    }
}