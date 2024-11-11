// Models/IEmitterDatabaseSettings.cs
namespace tree_form_API.Models
{
    public interface IEmitterDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        Dictionary<string, string> Collections { get; set; }
    }
}
