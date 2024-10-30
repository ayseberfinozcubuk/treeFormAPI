using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;
using System;

public class EmitterService
{
    private readonly IMongoCollection<Emitter> _emitterCollection;

    public EmitterService(IOptions<EmitterDatabaseSettings> emitterDatabaseSettings)
    {
        var mongoClient = new MongoClient(emitterDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(emitterDatabaseSettings.Value.DatabaseName);
        _emitterCollection = mongoDatabase.GetCollection<Emitter>(emitterDatabaseSettings.Value.CollectionName);
    }

    // Create a new Emitter
    public async Task CreateAsync(Emitter newEmitter)
    {
        if (newEmitter == null)
        {
            throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
        }

        await _emitterCollection.InsertOneAsync(newEmitter);
    }

    // Get all Emitters
    public async Task<List<Emitter>> GetAllAsync() =>
        await _emitterCollection.Find(_ => true).ToListAsync();

    // Get an Emitter by ID
    public async Task<Emitter?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        return await _emitterCollection.Find(filter).FirstOrDefaultAsync();
    }

    // Update an Emitter by ID (partial update)
    public async Task UpdateAsync(Guid id, Emitter updatedEmitter)
    {
        if (updatedEmitter == null)
        {
            throw new ArgumentNullException(nameof(updatedEmitter), "Updated Emitter data cannot be null.");
        }

        var existingEmitter = await GetByIdAsync(id);
        if (existingEmitter == null)
        {
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }

        var updates = new List<UpdateDefinition<Emitter>>();

        // Check top-level fields for differences
        if (existingEmitter.Notation != updatedEmitter.Notation)
            updates.Add(Builders<Emitter>.Update.Set(e => e.Notation, updatedEmitter.Notation));

        if (existingEmitter.EmitterName != updatedEmitter.EmitterName)
            updates.Add(Builders<Emitter>.Update.Set(e => e.EmitterName, updatedEmitter.EmitterName));

        if (existingEmitter.SpotNo != updatedEmitter.SpotNo)
            updates.Add(Builders<Emitter>.Update.Set(e => e.SpotNo, updatedEmitter.SpotNo));

        if (existingEmitter.Function != updatedEmitter.Function)
            updates.Add(Builders<Emitter>.Update.Set(e => e.Function, updatedEmitter.Function));

        if (existingEmitter.NumberOfModes != updatedEmitter.NumberOfModes)
            updates.Add(Builders<Emitter>.Update.Set(e => e.NumberOfModes, updatedEmitter.NumberOfModes));

        // For nested Modes list
        if (updatedEmitter.Modes != null)
        {
            for (int i = 0; i < updatedEmitter.Modes.Count; i++)
            {
                var updatedMode = updatedEmitter.Modes[i];

                if (i >= existingEmitter.Modes.Count)
                {
                    // New Mode added
                    updates.Add(Builders<Emitter>.Update.Push(e => e.Modes, updatedMode));
                }
                else
                {
                    var existingMode = existingEmitter.Modes[i];
                    
                    // Check each field in Mode and create individual updates if changed
                    if (existingMode.ModeName != updatedMode.ModeName)
                        updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].ModeName, updatedMode.ModeName));
                    
                    if (existingMode.Amplitude != updatedMode.Amplitude)
                        updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].Amplitude, updatedMode.Amplitude));
                    
                    if (existingMode.TheoricalRange != updatedMode.TheoricalRange)
                        updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].TheoricalRange, updatedMode.TheoricalRange));

                    // For nested Beams list in Modes
                    for (int j = 0; j < updatedMode.Beams.Count; j++)
                    {
                        var updatedBeam = updatedMode.Beams[j];
                        if (j >= existingMode.Beams.Count)
                        {
                            // New Beam added
                            updates.Add(Builders<Emitter>.Update.Push(e => e.Modes[i].Beams, updatedBeam));
                        }
                        else
                        {
                            var existingBeam = existingMode.Beams[j];

                            if (existingBeam.BeamName != updatedBeam.BeamName)
                                updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].Beams[j].BeamName, updatedBeam.BeamName));

                            if (existingBeam.AntennaGain != updatedBeam.AntennaGain)
                                updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].Beams[j].AntennaGain, updatedBeam.AntennaGain));

                            // Similarly, add updates for BeamWidthAzimute, BeamWidthElevation, etc.

                            // For DwellDurationValues in Beams
                            for (int k = 0; k < updatedBeam.DwellDurationValues.Count; k++)
                            {
                                var updatedDwell = updatedBeam.DwellDurationValues[k];
                                if (k >= existingBeam.DwellDurationValues.Count)
                                {
                                    updates.Add(Builders<Emitter>.Update.Push(e => e.Modes[i].Beams[j].DwellDurationValues, updatedDwell));
                                }
                                else
                                {
                                    var existingDwell = existingBeam.DwellDurationValues[k];

                                    if (existingDwell.BeamWPositionDuration != updatedDwell.BeamWPositionDuration)
                                        updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].Beams[j].DwellDurationValues[k].BeamWPositionDuration, updatedDwell.BeamWPositionDuration));

                                    if (existingDwell.BeamWPositionIndex != updatedDwell.BeamWPositionIndex)
                                        updates.Add(Builders<Emitter>.Update.Set(e => e.Modes[i].Beams[j].DwellDurationValues[k].BeamWPositionIndex, updatedDwell.BeamWPositionIndex));
                                }
                            }

                            // Similar approach for Sequences in Beams
                        }
                    }
                }
            }
        }

        // Apply the updates if there are any
        if (updates.Count > 0)
        {
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var updateDefinition = Builders<Emitter>.Update.Combine(updates);
            var result = await _emitterCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
            }
        }
    }

    // Delete an Emitter by ID
    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        var result = await _emitterCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }
}
