// Models/UserRoleUpdateDTO.cs
namespace tree_form_API.Models
{
    public class UserRoleUpdateDTO
    {
        public string Role { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
