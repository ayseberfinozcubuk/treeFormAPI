// Models/User.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tree_form_API.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; } = "read"; // Default role is 'read'
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? RoleUpdatedBy { get; set; }
        public DateTime? RoleUpdatedDate { get; set; }
    }
}
