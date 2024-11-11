// Models/EmitterDatabaseSettings.cs
namespace tree_form_API.Models
{
    public class EmitterDatabaseSettings : IEmitterDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public Dictionary<string, string> Collections { get; set; }
    }
}
