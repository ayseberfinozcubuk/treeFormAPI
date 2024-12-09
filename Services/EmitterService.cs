using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections;

namespace tree_form_API.Services
{
    public class EmitterService
    {
        private readonly IMongoCollection<Emitter> _emitterCollection;
        private readonly ILogger<UserService> _logger;

        public EmitterService(IOptions<EmitterDatabaseSettings> emitterDatabaseSettings, ILogger<UserService> logger)
        {
            var mongoClient = new MongoClient(emitterDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(emitterDatabaseSettings.Value.DatabaseName);
            _emitterCollection = mongoDatabase.GetCollection<Emitter>(emitterDatabaseSettings.Value.Collections["Emitters"]);
            _logger = logger;
        }

        public async Task CreateAsync(Emitter newEmitter)
        {
            //_logger.LogInformation("Emitter at creation Service: ", newEmitter);
            if (newEmitter == null)
                throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");

            // Assign CreatedDate and UpdatedDate
            newEmitter.CreatedDate = DateTime.UtcNow;

            await _emitterCollection.InsertOneAsync(newEmitter);
        }

        public async Task<List<Emitter>> GetAllAsync() => await _emitterCollection.Find(_ => true).ToListAsync();

        public async Task<Emitter?> GetByIdAsync(Guid id)
        {
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            return await _emitterCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Guid id, Emitter updatedEmitter)
        {
            if (updatedEmitter == null)
                throw new ArgumentNullException(nameof(updatedEmitter), "Updated Emitter data cannot be null.");

            var existingEmitter = await GetByIdAsync(id);
            if (existingEmitter == null)
                throw new InvalidOperationException($"Emitter with ID {id} not found.");

            // Assign UpdatedDate
            updatedEmitter.UpdatedDate = DateTime.UtcNow;

            // Dynamically update the object
            UpdateObject(existingEmitter, updatedEmitter);

            // Save updated object to the database
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            await _emitterCollection.ReplaceOneAsync(filter, existingEmitter);
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

        public async Task DeleteAsync(Guid id)
        {
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var result = await _emitterCollection.DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }
}