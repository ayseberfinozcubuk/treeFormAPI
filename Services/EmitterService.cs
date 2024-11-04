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

    public async Task CreateAsync(Emitter newEmitter)
    {
        if (newEmitter == null)
            throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
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

        var updates = new List<UpdateDefinition<Emitter>>();

        AddUpdateIfChanged(updates, existingEmitter.Notation, updatedEmitter.Notation, e => e.Notation);
        AddUpdateIfChanged(updates, existingEmitter.EmitterName, updatedEmitter.EmitterName, e => e.EmitterName);
        AddUpdateIfChanged(updates, existingEmitter.SpotNo, updatedEmitter.SpotNo, e => e.SpotNo);
        AddUpdateIfChanged(updates, existingEmitter.Function, updatedEmitter.Function, e => e.Function);
        AddUpdateIfChanged(updates, existingEmitter.NumberOfModes, updatedEmitter.NumberOfModes, e => e.NumberOfModes);

        SynchronizeNestedList(
            updates,
            existingEmitter.Modes,
            updatedEmitter.Modes,
            e => e.Modes,
            (existingMode, updatedMode, modeIndex) => SynchronizeMode(updates, existingMode, updatedMode, modeIndex)
        );

        if (updates.Count > 0)
        {
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            var updateDefinition = Builders<Emitter>.Update.Combine(updates);
            var result = await _emitterCollection.UpdateOneAsync(filter, updateDefinition);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }

    private void AddUpdateIfChanged<T>(List<UpdateDefinition<Emitter>> updates, T existingValue, T updatedValue, Expression<Func<Emitter, T>> field)
    {
        if (!EqualityComparer<T>.Default.Equals(existingValue, updatedValue))
            updates.Add(Builders<Emitter>.Update.Set(field, updatedValue));
    }

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

    private void SynchronizeMode(
        List<UpdateDefinition<Emitter>> updates,
        EmitterMode existingMode,
        EmitterMode updatedMode,
        int modeIndex)
    {
        AddUpdateIfChanged(updates, existingMode.ModeName, updatedMode.ModeName, e => e.Modes[modeIndex].ModeName);
        AddUpdateIfChanged(updates, existingMode.AmplitudeMin, updatedMode.AmplitudeMin, e => e.Modes[modeIndex].AmplitudeMin);
        AddUpdateIfChanged(updates, existingMode.AmplitudeMax, updatedMode.AmplitudeMax, e => e.Modes[modeIndex].AmplitudeMax);
        AddUpdateIfChanged(updates, existingMode.TheoricalRangeMin, updatedMode.TheoricalRangeMin, e => e.Modes[modeIndex].TheoricalRangeMin);
        AddUpdateIfChanged(updates, existingMode.TheoricalRangeMax, updatedMode.TheoricalRangeMax, e => e.Modes[modeIndex].TheoricalRangeMax);

        SynchronizeNestedList(
            updates,
            existingMode.Beams,
            updatedMode.Beams,
            e => e.Modes[modeIndex].Beams,
            (existingBeam, updatedBeam, beamIndex) => SynchronizeBeam(updates, existingBeam, updatedBeam, modeIndex, beamIndex)
        );

        SynchronizeNestedList(
            updates,
            existingMode.Pris,
            updatedMode.Pris,
            e => e.Modes[modeIndex].Pris,
            (existingPri, updatedPri, priIndex) => SynchronizePri(updates, existingPri, updatedPri, modeIndex, priIndex)
        );
    }

    private void SynchronizeBeam(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeam existingBeam,
        EmitterModeBeam updatedBeam,
        int modeIndex,
        int beamIndex)
    {
        AddUpdateIfChanged(updates, existingBeam.BeamName, updatedBeam.BeamName, e => e.Modes[modeIndex].Beams[beamIndex].BeamName);
        AddUpdateIfChanged(updates, existingBeam.AntennaGainMin, updatedBeam.AntennaGainMin, e => e.Modes[modeIndex].Beams[beamIndex].AntennaGainMin);
        AddUpdateIfChanged(updates, existingBeam.AntennaGainMax, updatedBeam.AntennaGainMax, e => e.Modes[modeIndex].Beams[beamIndex].AntennaGainMax);
        AddUpdateIfChanged(updates, existingBeam.BeamPositionMin, updatedBeam.BeamPositionMin, e => e.Modes[modeIndex].Beams[beamIndex].BeamPositionMin);
        AddUpdateIfChanged(updates, existingBeam.BeamPositionMax, updatedBeam.BeamPositionMax, e => e.Modes[modeIndex].Beams[beamIndex].BeamPositionMax);
        AddUpdateIfChanged(updates, existingBeam.BeamWidthAzimuteMin, updatedBeam.BeamWidthAzimuteMin, e => e.Modes[modeIndex].Beams[beamIndex].BeamWidthAzimuteMin);
        AddUpdateIfChanged(updates, existingBeam.BeamWidthAzimuteMax, updatedBeam.BeamWidthAzimuteMax, e => e.Modes[modeIndex].Beams[beamIndex].BeamWidthAzimuteMax);
        AddUpdateIfChanged(updates, existingBeam.BeamWidthElevationMin, updatedBeam.BeamWidthElevationMin, e => e.Modes[modeIndex].Beams[beamIndex].BeamWidthElevationMin);
        AddUpdateIfChanged(updates, existingBeam.BeamWidthElevationMax, updatedBeam.BeamWidthElevationMax, e => e.Modes[modeIndex].Beams[beamIndex].BeamWidthElevationMax);

        SynchronizeNestedList(
            updates,
            existingBeam.DwellDurationValues,
            updatedBeam.DwellDurationValues,
            e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues,
            (existingDwell, updatedDwell, dwellIndex) => SynchronizeDwellDuration(updates, existingDwell, updatedDwell, modeIndex, beamIndex, dwellIndex)
        );

        SynchronizeNestedList(
            updates,
            existingBeam.Sequences,
            updatedBeam.Sequences,
            e => e.Modes[modeIndex].Beams[beamIndex].Sequences,
            (existingSequence, updatedSequence, seqIndex) => SynchronizeBeamSequence(updates, existingSequence, updatedSequence, modeIndex, beamIndex, seqIndex)
        );
    }

    private void SynchronizeDwellDuration(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionDwellDurationValue existingDwell,
        EmitterModeBeamPositionDwellDurationValue updatedDwell,
        int modeIndex,
        int beamIndex,
        int dwellIndex)
    {
        AddUpdateIfChanged(updates, existingDwell.BeamWPositionDurationMin, updatedDwell.BeamWPositionDurationMin, e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].BeamWPositionDurationMin);
        AddUpdateIfChanged(updates, existingDwell.BeamWPositionDurationMax, updatedDwell.BeamWPositionDurationMax, e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].BeamWPositionDurationMax);
        AddUpdateIfChanged(updates, existingDwell.BeamWPositionIndex, updatedDwell.BeamWPositionIndex, e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].BeamWPositionIndex);

        SynchronizeNestedList(
            updates,
            existingDwell.FiringOrders,
            updatedDwell.FiringOrders,
            e => e.Modes[modeIndex].Beams[beamIndex].DwellDurationValues[dwellIndex].FiringOrders,
            (existingOrder, updatedOrder, orderIndex) => SynchronizeFiringOrder(updates, existingOrder, updatedOrder, modeIndex, beamIndex, dwellIndex, orderIndex)
        );
    }

    private void SynchronizeBeamSequence(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionSequence existingSequence,
        EmitterModeBeamPositionSequence updatedSequence,
        int modeIndex,
        int beamIndex,
        int seqIndex)
    {
        AddUpdateIfChanged(updates, existingSequence.SequenceName, updatedSequence.SequenceName, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].SequenceName);

        SynchronizeNestedList(
            updates,
            existingSequence.FiringOrders,
            updatedSequence.FiringOrders,
            e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders,
            (existingOrder, updatedOrder, orderIndex) => SynchronizeFiringOrder(updates, existingOrder, updatedOrder, modeIndex, beamIndex, seqIndex, orderIndex)
        );
    }

    private void SynchronizeFiringOrder(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModeBeamPositionFiringOrder existingOrder,
        EmitterModeBeamPositionFiringOrder updatedOrder,
        int modeIndex,
        int beamIndex,
        int seqIndex,
        int orderIndex)
    {
        AddUpdateIfChanged(updates, existingOrder.BeamPositionOrderIndexMin, updatedOrder.BeamPositionOrderIndexMin, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionOrderIndexMin);
        AddUpdateIfChanged(updates, existingOrder.BeamPositionOrderIndexMax, updatedOrder.BeamPositionOrderIndexMax, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionOrderIndexMax);
        AddUpdateIfChanged(updates, existingOrder.BeamPositionIndexMin, updatedOrder.BeamPositionIndexMin, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionIndexMin);
        AddUpdateIfChanged(updates, existingOrder.BeamPositionIndexMax, updatedOrder.BeamPositionIndexMax, e => e.Modes[modeIndex].Beams[beamIndex].Sequences[seqIndex].FiringOrders[orderIndex].BeamPositionIndexMax);
    }

    private void SynchronizePri(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePri existingPri,
        EmitterModePri updatedPri,
        int modeIndex,
        int priIndex)
    {
        AddUpdateIfChanged(updates, existingPri.PriName, updatedPri.PriName, e => e.Modes[modeIndex].Pris[priIndex].PriName);
        AddUpdateIfChanged(updates, existingPri.PriLimitMin, updatedPri.PriLimitMin, e => e.Modes[modeIndex].Pris[priIndex].PriLimitMin);
        AddUpdateIfChanged(updates, existingPri.PriLimitMax, updatedPri.PriLimitMax, e => e.Modes[modeIndex].Pris[priIndex].PriLimitMax);

        SynchronizeNestedList(
            updates,
            existingPri.SuperPeriods,
            updatedPri.SuperPeriods,
            e => e.Modes[modeIndex].Pris[priIndex].SuperPeriods,
            (existingSuperPeriod, updatedSuperPeriod, superPeriodIndex) => SynchronizeSuperPeriod(updates, existingSuperPeriod, updatedSuperPeriod, modeIndex, priIndex, superPeriodIndex)
        );

        SynchronizeNestedList(
            updates,
            existingPri.MostProbableValues,
            updatedPri.MostProbableValues,
            e => e.Modes[modeIndex].Pris[priIndex].MostProbableValues,
            (existingMostProbable, updatedMostProbable, mostProbableIndex) => SynchronizeMostProbableValue(updates, existingMostProbable, updatedMostProbable, modeIndex, priIndex, mostProbableIndex)
        );

        SynchronizeNestedList(
            updates,
            existingPri.DiscreteValues,
            updatedPri.DiscreteValues,
            e => e.Modes[modeIndex].Pris[priIndex].DiscreteValues,
            (existingDiscrete, updatedDiscrete, discreteIndex) => SynchronizeDiscreteValue(updates, existingDiscrete, updatedDiscrete, modeIndex, priIndex, discreteIndex)
        );

        SynchronizeNestedList(
            updates,
            existingPri.Sequences,
            updatedPri.Sequences,
            e => e.Modes[modeIndex].Pris[priIndex].Sequences,
            (existingSequence, updatedSequence, seqIndex) => SynchronizePriSequence(updates, existingSequence, updatedSequence, modeIndex, priIndex, seqIndex)
        );
    }

    private void SynchronizeSuperPeriod(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePriSuperPeriodValue existingSuperPeriod,
        EmitterModePriSuperPeriodValue updatedSuperPeriod,
        int modeIndex,
        int priIndex,
        int superPeriodIndex)
    {
        AddUpdateIfChanged(updates, existingSuperPeriod.SuperPeriodValueMin, updatedSuperPeriod.SuperPeriodValueMin, e => e.Modes[modeIndex].Pris[priIndex].SuperPeriods[superPeriodIndex].SuperPeriodValueMin);
        AddUpdateIfChanged(updates, existingSuperPeriod.SuperPeriodValueMax, updatedSuperPeriod.SuperPeriodValueMax, e => e.Modes[modeIndex].Pris[priIndex].SuperPeriods[superPeriodIndex].SuperPeriodValueMax);
    }

    private void SynchronizeMostProbableValue(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePriMostProbableValue existingMostProbable,
        EmitterModePriMostProbableValue updatedMostProbable,
        int modeIndex,
        int priIndex,
        int mostProbableIndex)
    {
        AddUpdateIfChanged(updates, existingMostProbable.MostProbableValueMin, updatedMostProbable.MostProbableValueMin, e => e.Modes[modeIndex].Pris[priIndex].MostProbableValues[mostProbableIndex].MostProbableValueMin);
        AddUpdateIfChanged(updates, existingMostProbable.MostProbableValueMax, updatedMostProbable.MostProbableValueMax, e => e.Modes[modeIndex].Pris[priIndex].MostProbableValues[mostProbableIndex].MostProbableValueMax);
    }

    private void SynchronizeDiscreteValue(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePriDiscreteValue existingDiscrete,
        EmitterModePriDiscreteValue updatedDiscrete,
        int modeIndex,
        int priIndex,
        int discreteIndex)
    {
        AddUpdateIfChanged(updates, existingDiscrete.DiscreteValueMin, updatedDiscrete.DiscreteValueMin, e => e.Modes[modeIndex].Pris[priIndex].DiscreteValues[discreteIndex].DiscreteValueMin);
        AddUpdateIfChanged(updates, existingDiscrete.DiscreteValueMax, updatedDiscrete.DiscreteValueMax, e => e.Modes[modeIndex].Pris[priIndex].DiscreteValues[discreteIndex].DiscreteValueMax);

        SynchronizeNestedList(
            updates,
            existingDiscrete.FiringOrders,
            updatedDiscrete.FiringOrders,
            e => e.Modes[modeIndex].Pris[priIndex].DiscreteValues[discreteIndex].FiringOrders,
            (existingOrder, updatedOrder, orderIndex) => SynchronizeFiringOrder(updates, existingOrder, updatedOrder, modeIndex, priIndex, discreteIndex, orderIndex)
        );
    }

    private void SynchronizePriSequence(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePriSequence existingSequence,
        EmitterModePriSequence updatedSequence,
        int modeIndex,
        int priIndex,
        int seqIndex)
    {
        AddUpdateIfChanged(updates, existingSequence.SequenceName, updatedSequence.SequenceName, e => e.Modes[modeIndex].Pris[priIndex].Sequences[seqIndex].SequenceName);
        AddUpdateIfChanged(updates, existingSequence.NumberOfPulsesInSequence, updatedSequence.NumberOfPulsesInSequence, e => e.Modes[modeIndex].Pris[priIndex].Sequences[seqIndex].NumberOfPulsesInSequence);

        SynchronizeNestedList(
            updates,
            existingSequence.FiringOrders,
            updatedSequence.FiringOrders,
            e => e.Modes[modeIndex].Pris[priIndex].Sequences[seqIndex].FiringOrders,
            (existingOrder, updatedOrder, orderIndex) => SynchronizeFiringOrder(updates, existingOrder, updatedOrder, modeIndex, priIndex, seqIndex, orderIndex)
        );
    }

    private void SynchronizeFiringOrder(
        List<UpdateDefinition<Emitter>> updates,
        EmitterModePriFiringOrder existingOrder,
        EmitterModePriFiringOrder updatedOrder,
        int modeIndex,
        int priIndex,
        int seqIndex,
        int orderIndex)
    {
        AddUpdateIfChanged(updates, existingOrder.OrderIndexMin, updatedOrder.OrderIndexMin, e => e.Modes[modeIndex].Pris[priIndex].Sequences[seqIndex].FiringOrders[orderIndex].OrderIndexMin);
        AddUpdateIfChanged(updates, existingOrder.OrderIndexMax, updatedOrder.OrderIndexMax, e => e.Modes[modeIndex].Pris[priIndex].Sequences[seqIndex].FiringOrders[orderIndex].OrderIndexMax);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        var result = await _emitterCollection.DeleteOneAsync(filter);
        if (result.DeletedCount == 0)
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
    }
}
