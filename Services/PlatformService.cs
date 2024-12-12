using System.Collections;
using MongoDB.Driver;
using tree_form_API.Models;

namespace tree_form_API.Services
{
    public class PlatformService
    {
        private readonly IMongoCollection<Platform> _platformCollection;
        private readonly IMongoCollection<Emitter> _emitterCollection;
        private readonly ILogger<UserService> _logger;

        public PlatformService(IMongoCollection<Platform> platformCollection, IMongoCollection<Emitter> emitterCollection, ILogger<UserService> logger)
        {
            _platformCollection = platformCollection;
            _emitterCollection = emitterCollection;
            _logger = logger;
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
            platform.CreatedDate = DateTime.UtcNow;
            await _platformCollection.InsertOneAsync(platform);
        }

        public async Task<Platform?> GetByIdAsync(Guid id)
        {
            var filter = Builders<Platform>.Filter.Eq(e => e.Id, id);
            return await _platformCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Guid id, Platform updatedPlatform)
        {
            if (updatedPlatform == null)
                throw new ArgumentNullException(nameof(updatedPlatform), "Updated Platform data cannot be null.");

            var existingPlatform = await GetByIdAsync(id);
            if (existingPlatform == null)
                throw new InvalidOperationException($"Platform with ID {id} not found.");

            // Assign UpdatedDate to the current time
            updatedPlatform.UpdatedDate = DateTime.UtcNow;

            // Dynamically update the object
            UpdateObject(existingPlatform, updatedPlatform);

            // Save updated object to the database
            var filter = Builders<Platform>.Filter.Eq(e => e.Id, id);
            await _platformCollection.ReplaceOneAsync(filter, existingPlatform);
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
                    // Handle collections
                    UpdateCollection(existingValue as IList, updatedValue as IList, property.PropertyType.GetGenericArguments().FirstOrDefault());
                }
                else
                {
                    // Handle scalar properties
                    if (updatedValue == null)
                    {
                        // Explicitly set property to null
                        property.SetValue(existingObject, null);
                    }
                    else if (!Equals(existingValue, updatedValue))
                    {
                        // Update property if the value has changed
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
                    // Clear the collection if updatedCollection is null
                    existingCollection.Clear();
                }
                return;
            }

            // Update existing items or add new ones
            for (int i = 0; i < updatedCollection.Count; i++)
            {
                if (i < existingCollection.Count)
                {
                    // Update existing items
                    UpdateObject(existingCollection[i], updatedCollection[i]);
                }
                else
                {
                    // Add new items
                    existingCollection.Add(updatedCollection[i]);
                }
            }

            // Remove excess items
            while (existingCollection.Count > updatedCollection.Count)
            {
                existingCollection.RemoveAt(existingCollection.Count - 1);
            }
        }

        /// <summary>
        /// Delete a platform if it has no associated emitters.
        /// </summary>
        public async Task<(bool IsDeleted, string Message)> DeletePlatformAsync(Guid platformId)
        {
            // Check if the platform is associated with any emitters
            var filter = Builders<Emitter>.Filter.ElemMatch(
                e => e.AssociatedPlatforms,
                p => p.PlatformId == platformId
            );

            var isAssociated = await _emitterCollection.CountDocumentsAsync(filter) > 0;

            if (isAssociated)
            {
                return (false, "Cannot delete platform as it is associated with one or more emitters.");
            }

            // Proceed with deletion
            var result = await _platformCollection.DeleteOneAsync(p => p.Id == platformId);

            if (result.DeletedCount > 0)
            {
                return (true, "Platform deleted successfully.");
            }
            else
            {
                return (false, $"Platform with ID {platformId} not found.");
            }
        }
    
        public async Task<long> GetCountAsync()
        {
            return await _platformCollection.CountDocumentsAsync(_ => true);
        }
    
        public async Task<long> GetRecentCountAsync(TimeSpan timeSpan)
        {
            var recentDate = DateTime.UtcNow.Subtract(timeSpan);
            var filter = Builders<Platform>.Filter.Gte(e => e.CreatedDate, recentDate);
            return await _platformCollection.CountDocumentsAsync(filter);
        }
    
    }
}
