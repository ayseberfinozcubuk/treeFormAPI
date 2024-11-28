using MongoDB.Driver;
using tree_form_API.Models;

namespace tree_form_API.Services
{
    public class PlatformService
    {
        private readonly IMongoCollection<Platform> _platformCollection;

        public PlatformService(IMongoCollection<Platform> platformCollection)
        {
            _platformCollection = platformCollection;
        }

        public async Task<List<Platform>> GetAllPlatformsAsync()
        {
            return await _platformCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Platform?> GetPlatformByIdAsync(Guid id)
        {
            return await _platformCollection.Find(platform => platform.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddPlatformAsync(Platform platform)
        {
            platform.Id = Guid.NewGuid();
            await _platformCollection.InsertOneAsync(platform);
        }

        public async Task<bool> DeletePlatformAsync(Guid id)
        {
            var result = await _platformCollection.DeleteOneAsync(platform => platform.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task PlatformUpdatedByAsync(Guid id, string updatedBy)
        {
            var filter = Builders<Platform>.Filter.Eq(e => e.Id, id);
            var update = Builders<Platform>.Update.Set(e => e.UpdatedBy, updatedBy);

            var result = await _platformCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException($"Platform with ID {id} not found.");
            }
        }
    }
}
