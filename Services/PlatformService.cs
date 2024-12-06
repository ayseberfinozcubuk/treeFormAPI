using System.Collections;
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

        public async Task<bool> DeletePlatformAsync(Guid id)
        {
            var result = await _platformCollection.DeleteOneAsync(platform => platform.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task PlatformUpdatedByAsync(Guid id, Guid updatedBy)
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
