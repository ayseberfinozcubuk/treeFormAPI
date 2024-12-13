using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;
using System.Collections;

namespace tree_form_API.Services
{
    public class EmitterService
    {
        private readonly IMongoCollection<Emitter> _emitterCollection;
        private readonly ILogger<EmitterService> _logger;

        public EmitterService(IOptions<EmitterDatabaseSettings> emitterDatabaseSettings, ILogger<EmitterService> logger)
        {
            var mongoClient = new MongoClient(emitterDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(emitterDatabaseSettings.Value.DatabaseName);
            _emitterCollection = mongoDatabase.GetCollection<Emitter>(emitterDatabaseSettings.Value.Collections["Emitters"]);
            _logger = logger;
        }

        public async Task CreateAsync(Emitter newEmitter)
        {
            if (newEmitter == null)
            {
                _logger.LogWarning("CreateAsync: Attempted to create a null emitter.");
                throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
            }

            newEmitter.CreatedDate = DateTime.UtcNow;

            await _emitterCollection.InsertOneAsync(newEmitter);
            _logger.LogInformation("CreateAsync: Emitter with ID {EmitterId} created successfully.", newEmitter.Id);
        }

        public async Task<List<Emitter>> GetAllAsync()
        {
            _logger.LogInformation("GetAllAsync: Retrieving all emitters.");
            return await _emitterCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Emitter?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("GetByIdAsync: Retrieving emitter with ID {EmitterId}.", id);
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var emitter = await _emitterCollection.Find(filter).FirstOrDefaultAsync();

            if (emitter == null)
                _logger.LogWarning("GetByIdAsync: Emitter with ID {EmitterId} not found.", id);

            return emitter;
        }

        public async Task UpdateAsync(Guid id, Emitter updatedEmitter)
        {
            if (updatedEmitter == null)
            {
                _logger.LogWarning("UpdateAsync: Attempted to update with null emitter data.");
                throw new ArgumentNullException(nameof(updatedEmitter), "Updated Emitter data cannot be null.");
            }

            var existingEmitter = await GetByIdAsync(id);
            if (existingEmitter == null)
            {
                _logger.LogWarning("UpdateAsync: Emitter with ID {EmitterId} not found for update.", id);
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
            }

            updatedEmitter.UpdatedDate = DateTime.UtcNow;

            UpdateObject(existingEmitter, updatedEmitter);

            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            await _emitterCollection.ReplaceOneAsync(filter, existingEmitter);

            _logger.LogInformation("UpdateAsync: Emitter with ID {EmitterId} updated successfully.", id);
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("DeleteAsync: Attempting to delete emitter with ID {EmitterId}.", id);
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var result = await _emitterCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("DeleteAsync: Emitter with ID {EmitterId} not found for deletion.", id);
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
            }

            _logger.LogInformation("DeleteAsync: Emitter with ID {EmitterId} deleted successfully.", id);
        }

        public async Task<long> GetCountAsync()
        {
            _logger.LogInformation("GetCountAsync: Counting all emitters.");
            return await _emitterCollection.CountDocumentsAsync(_ => true);
        }

        public async Task<long> GetRecentCountAsync(TimeSpan timeSpan)
        {
            var recentDate = DateTime.UtcNow.Subtract(timeSpan);
            _logger.LogInformation("GetRecentCountAsync: Counting emitters created since {RecentDate}.", recentDate);
            var filter = Builders<Emitter>.Filter.Gte(e => e.CreatedDate, recentDate);
            return await _emitterCollection.CountDocumentsAsync(filter);
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
