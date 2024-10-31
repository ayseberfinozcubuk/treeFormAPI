using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
            throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
        await _emitterCollection.InsertOneAsync(newEmitter);
    }

    // Get all Emitters
    public async Task<List<Emitter>> GetAllAsync() => await _emitterCollection.Find(_ => true).ToListAsync();

    // Get an Emitter by ID
    public async Task<Emitter?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        return await _emitterCollection.Find(filter).FirstOrDefaultAsync();
    }

    // Update an Emitter by ID with full synchronization of nested lists
    public async Task UpdateAsync(Guid id, Emitter updatedEmitter)
    {
        if (updatedEmitter == null)
            throw new ArgumentNullException(nameof(updatedEmitter), "Updated Emitter data cannot be null.");

        var existingEmitter = await GetByIdAsync(id);
        if (existingEmitter == null)
            throw new InvalidOperationException($"Emitter with ID {id} not found.");

        var updates = new List<UpdateDefinition<Emitter>>();

        // Update top-level properties if changed
        AddUpdateIfChanged(updates, existingEmitter.Notation, updatedEmitter.Notation, e => e.Notation);
        AddUpdateIfChanged(updates, existingEmitter.EmitterName, updatedEmitter.EmitterName, e => e.EmitterName);
        AddUpdateIfChanged(updates, existingEmitter.SpotNo, updatedEmitter.SpotNo, e => e.SpotNo);
        AddUpdateIfChanged(updates, existingEmitter.Function, updatedEmitter.Function, e => e.Function);
        AddUpdateIfChanged(updates, existingEmitter.NumberOfModes, updatedEmitter.NumberOfModes, e => e.NumberOfModes);

        // Synchronize Modes list
        SynchronizeNestedList(
            updates,
            existingEmitter.Modes,
            updatedEmitter.Modes,
            e => e.Modes,
            (existingMode, updatedMode, modeIndex) => SynchronizeMode(updates, existingMode, updatedMode, modeIndex)
        );

        // Apply the updates if any exist
        if (updates.Count > 0)
        {
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var updateDefinition = Builders<Emitter>.Update.Combine(updates);
            var result = await _emitterCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }

    // Helper to add an update if the property has changed
    private void AddUpdateIfChanged<T>(List<UpdateDefinition<Emitter>> updates, T existingValue, T updatedValue, Expression<Func<Emitter, T>> field)
    {
        if (!EqualityComparer<T>.Default.Equals(existingValue, updatedValue))
            updates.Add(Builders<Emitter>.Update.Set(field, updatedValue));
    }

    // Synchronize Modes with nested structure
    private void SynchronizeNestedList<T>(
        List<UpdateDefinition<Emitter>> updates,
        List<T> existingList,
        List<T> updatedList,
        Expression<Func<Emitter, IEnumerable<T>>> listField,
        Action<T, T, int> synchronizeNestedFields) where T : class
    {
        if (updatedList.Count < existingList.Count)
        {
            updates.Add(Builders<Emitter>.Update.Set(listField, updatedList));
            return;
        }

        for (int i = 0; i < updatedList.Count; i++)
        {
            if (i >= existingList.Count)
            {
                updates.Add(Builders<Emitter>.Update.Push(listField, updatedList[i]));
            }
            else
            {
                synchronizeNestedFields(existingList[i], updatedList[i], i);
            }
        }
    }

    // Synchronize properties and nested lists for Mode
    private void SynchronizeMode(
        List<UpdateDefinition<Emitter>> updates,
        EmitterMode existingMode,
        EmitterMode updatedMode,
        int modeIndex)
    {
        AddUpdateIfChanged(updates, existingMode.ModeName, updatedMode.ModeName, e => e.Modes[modeIndex].ModeName);
        AddUpdateIfChanged(updates, existingMode.Amplitude, updatedMode.Amplitude, e => e.Modes[modeIndex].Amplitude);
        AddUpdateIfChanged(updates, existingMode.TheoricalRange, updatedMode.TheoricalRange, e => e.Modes[modeIndex].TheoricalRange);

        // Synchronize nested Beams list
        SynchronizeNestedList(
            updates,
            existingMode.Beams,
            updatedMode.Beams,
            e => e.Modes[modeIndex].Beams,
            (existingBeam, updatedBeam, beamIndex) => SynchronizeBeam(updates, existingBeam, updatedBeam, modeIndex, beamIndex)
        );
    }

    // Synchronize properties and nested lists for Beam
    private void SynchronizeBeam(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeam existingBeam,
        EmitterModeBeam updatedBeam,
        int modeIndex,
        int beamIndex)
    {
        AddUpdateIfChanged(updates, existingBeam.BeamName, updatedBeam.BeamName, e => e.Modes[modeIndex].Beams[beamIndex].BeamName);
        AddUpdateIfChanged(updates, existingBeam.AntennaGain, updatedBeam.AntennaGain, e => e.Modes[modeIndex].Beams[beamIndex].AntennaGain);

        // Synchronize nested DwellDurationValues
        SynchronizeNestedList(
            updates,
            existingBeam.DwellDurationValues,
            updatedBeam.DwellDurationValues,
            e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues,
            (existingDwell, updatedDwell, dwellIndex) => SynchronizeDwellDuration(updates, existingDwell, updatedDwell, modeIndex, beamIndex, dwellIndex)
        );

        // Synchronize nested Sequences list
        SynchronizeNestedList(
            updates,
            existingBeam.Sequences,
            updatedBeam.Sequences,
            e => e.Modes[modeIndex].Beams[beamIndex].Sequences,
            (existingSequence, updatedSequence, seqIndex) => SynchronizeSequence(updates, existingSequence, updatedSequence, modeIndex, beamIndex, seqIndex)
        );
    }

    // Synchronize properties for DwellDuration
    private void SynchronizeDwellDuration(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionDwellDurationValue existingDwell,
        EmitterModeBeamPositionDwellDurationValue updatedDwell,
        int modeIndex,
        int beamIndex,
        int dwellIndex)
    {
        AddUpdateIfChanged(updates, existingDwell.BeamWPositionDuration, updatedDwell.BeamWPositionDuration, e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].BeamWPositionDuration);
        AddUpdateIfChanged(updates, existingDwell.BeamWPositionIndex, updatedDwell.BeamWPositionIndex, e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].BeamWPositionIndex);
    }

    // Synchronize properties and nested lists for Sequence
    private void SynchronizeSequence(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionSequence existingSequence,
        EmitterModeBeamPositionSequence updatedSequence,
        int modeIndex,
        int beamIndex,
        int seqIndex)
    {
        AddUpdateIfChanged(updates, existingSequence.SequenceName, updatedSequence.SequenceName, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].SequenceName);

        // Synchronize nested FiringOrders list
        SynchronizeNestedList(
            updates,
            existingSequence.FiringOrders,
            updatedSequence.FiringOrders,
            e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders,
            (existingOrder, updatedOrder, orderIndex) => SynchronizeFiringOrder(updates, existingOrder, updatedOrder, modeIndex, beamIndex, seqIndex, orderIndex)
        );
    }

    // Synchronize properties for FiringOrder
    private void SynchronizeFiringOrder(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionFiringOrder existingOrder,
        EmitterModeBeamPositionFiringOrder updatedOrder,
        int modeIndex,
        int beamIndex,
        int seqIndex,
        int orderIndex)
    {
        AddUpdateIfChanged(updates, existingOrder.BeamPositionOrderIndex, updatedOrder.BeamPositionOrderIndex, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionOrderIndex);
        AddUpdateIfChanged(updates, existingOrder.BeamPositionIndex, updatedOrder.BeamPositionIndex, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionIndex);
    }

    // Delete an Emitter by ID
    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        var result = await _emitterCollection.DeleteOneAsync(filter);
        if (result.DeletedCount == 0)
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
    }
}
