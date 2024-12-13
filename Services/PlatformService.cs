using System.Collections;
using MongoDB.Driver;
using tree_form_API.Models;

namespace tree_form_API.Services
{
    public class PlatformService
    {
        private readonly IMongoCollection<Platform> _platformCollection;
        private readonly IMongoCollection<Emitter> _emitterCollection;
        private readonly ILogger<PlatformService> _logger;

        public PlatformService(IMongoCollection<Platform> platformCollection, IMongoCollection<Emitter> emitterCollection, ILogger<PlatformService> logger)
        {
            _platformCollection = platformCollection;
            _emitterCollection = emitterCollection;
            _logger = logger;
        }

        public async Task<List<Platform>> GetAllPlatformsAsync()
        {
            _logger.LogInformation("GetAllPlatformsAsync: Retrieving all platforms.");
            return await _platformCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Platform?> GetPlatformByIdAsync(Guid id)
        {
            _logger.LogInformation("GetPlatformByIdAsync: Retrieving platform with ID {PlatformId}.", id);
            var platform = await _platformCollection.Find(platform => platform.Id == id).FirstOrDefaultAsync();

            if (platform == null)
            {
                _logger.LogWarning("GetPlatformByIdAsync: Platform with ID {PlatformId} not found.", id);
            }

            return platform;
        }

        public async Task AddPlatformAsync(Platform platform)
        {
            platform.Id = Guid.NewGuid();
            platform.CreatedDate = DateTime.UtcNow;

            await _platformCollection.InsertOneAsync(platform);
            _logger.LogInformation("AddPlatformAsync: Platform with ID {PlatformId} added successfully.", platform.Id);
        }

        public async Task<Platform?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("GetByIdAsync: Retrieving platform with ID {PlatformId}.", id);
            var platform = await _platformCollection.Find(e => e.Id == id).FirstOrDefaultAsync();

            if (platform == null)
            {
                _logger.LogWarning("GetByIdAsync: Platform with ID {PlatformId} not found.", id);
            }

            return platform;
        }

        public async Task UpdateAsync(Guid id, Platform updatedPlatform)
        {
            if (updatedPlatform == null)
            {
                _logger.LogWarning("UpdateAsync: Attempted to update with null platform data.");
                throw new ArgumentNullException(nameof(updatedPlatform), "Updated Platform data cannot be null.");
            }

            var existingPlatform = await GetByIdAsync(id);
            if (existingPlatform == null)
            {
                _logger.LogWarning("UpdateAsync: Platform with ID {PlatformId} not found for update.", id);
                throw new InvalidOperationException($"Platform with ID {id} not found.");
            }

            updatedPlatform.UpdatedDate = DateTime.UtcNow;

            UpdateObject(existingPlatform, updatedPlatform);

            var filter = Builders<Platform>.Filter.Eq(e => e.Id, id);
            await _platformCollection.ReplaceOneAsync(filter, existingPlatform);

            _logger.LogInformation("UpdateAsync: Platform with ID {PlatformId} updated successfully.", id);
        }

        /// <summary>
        /// Delete a platform if it has no associated emitters.
        /// </summary>
        public async Task<(bool IsDeleted, string Message)> DeletePlatformAsync(Guid platformId)
        {
            _logger.LogInformation("DeletePlatformAsync: Checking associations for platform with ID {PlatformId}.", platformId);

            var filter = Builders<Emitter>.Filter.ElemMatch(
                e => e.AssociatedPlatforms,
                p => p.PlatformId == platformId
            );

            var isAssociated = await _emitterCollection.CountDocumentsAsync(filter) > 0;

            if (isAssociated)
            {
                _logger.LogWarning("DeletePlatformAsync: Cannot delete platform with ID {PlatformId} as it is associated with emitters.", platformId);
                return (false, "Cannot delete platform as it is associated with one or more emitters.");
            }

            var result = await _platformCollection.DeleteOneAsync(p => p.Id == platformId);

            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("DeletePlatformAsync: Platform with ID {PlatformId} deleted successfully.", platformId);
                return (true, "Platform deleted successfully.");
            }
            else
            {
                _logger.LogWarning("DeletePlatformAsync: Platform with ID {PlatformId} not found for deletion.", platformId);
                return (false, $"Platform with ID {platformId} not found.");
            }
        }

        public async Task<long> GetCountAsync()
        {
            _logger.LogInformation("GetCountAsync: Counting all platforms.");
            return await _platformCollection.CountDocumentsAsync(_ => true);
        }

        public async Task<long> GetRecentCountAsync(TimeSpan timeSpan)
        {
            var recentDate = DateTime.UtcNow.Subtract(timeSpan);
            _logger.LogInformation("GetRecentCountAsync: Counting platforms created since {RecentDate}.", recentDate);
            var filter = Builders<Platform>.Filter.Gte(e => e.CreatedDate, recentDate);
            return await _platformCollection.CountDocumentsAsync(filter);
        }

        private void UpdateObject(object existingObject, object updatedObject)
        {
            if (existingObject == null || updatedObject == null)
                return;

            var properties = existingObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                var existingValue = property.GetValue(existingObject);
                var updatedValue = property.GetValue(updatedObject);

                if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) && property.PropertyType != typeof(string))
                {
                    UpdateCollection(existingValue as IList, updatedValue as IList, property.PropertyType.GetGenericArguments().FirstOrDefault());
                }
                else
                {
                    if (!Equals(existingValue, updatedValue))
                    {
                        property.SetValue(existingObject, updatedValue);
                    }
                }
            }
        }

        private void UpdateCollection(IList? existingCollection, IList? updatedCollection, Type? elementType)
        {
            if (existingCollection == null || updatedCollection == null || elementType == null)
            {
                if (existingCollection != null && updatedCollection == null)
                {
                    existingCollection.Clear();
                }
                return;
            }

            for (int i = 0; i < updatedCollection.Count; i++)
            {
                if (i < existingCollection.Count)
                {
                    UpdateObject(existingCollection[i], updatedCollection[i]);
                }
                else
                {
                    existingCollection.Add(updatedCollection[i]);
                }
            }

            while (existingCollection.Count > updatedCollection.Count)
            {
                existingCollection.RemoveAt(existingCollection.Count - 1);
            }
        }
    }
}
