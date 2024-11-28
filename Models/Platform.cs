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
        public string PlatformName { get; set; } = string.Empty; // Notasyon: Alphanumeric emitter notation
        public double? Latitude { get; set; }  // Emiter Adı
        public double? Longitude { get; set; } // Spot No
        public string PlatformType { get; set; } = string.Empty;// Görev Kodu
        public string? PennantOrTailOrPlateNumber { get; set; } // Mod Sayısı
        public string PlatformCategory { get; set; } = string.Empty; // Emiter Mod Listesi
        public string? UpdatedBy { get; set; }
    }
}