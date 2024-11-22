using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace tree_form_API.Services
{
    public class EmitterService
    {
        private readonly IMongoCollection<Emitter> _emitterCollection;

        public EmitterService(IOptions<EmitterDatabaseSettings> emitterDatabaseSettings)
        {
            var mongoClient = new MongoClient(emitterDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(emitterDatabaseSettings.Value.DatabaseName);
            _emitterCollection = mongoDatabase.GetCollection<Emitter>(emitterDatabaseSettings.Value.Collections["Emitters"]);
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

            // Update top-level properties
            UpdateTopLevelProperties(existingEmitter, updatedEmitter);

            // Update Modes collection
            SynchronizeCollection(
                existingEmitter.Modes,
                updatedEmitter.Modes,
                UpdateMode
            );

            // Apply the updated emitter to the database
            var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
            await _emitterCollection.ReplaceOneAsync(filter, existingEmitter);
        }

        private void UpdateTopLevelProperties(Emitter existingEmitter, Emitter updatedEmitter)
        {
            if (!Equals(existingEmitter.Notation, updatedEmitter.Notation))
                existingEmitter.Notation = updatedEmitter.Notation;

            if (!Equals(existingEmitter.EmitterName, updatedEmitter.EmitterName))
                existingEmitter.EmitterName = updatedEmitter.EmitterName;

            if (!Equals(existingEmitter.SpotNo, updatedEmitter.SpotNo))
                existingEmitter.SpotNo = updatedEmitter.SpotNo;

            if (!Equals(existingEmitter.Function, updatedEmitter.Function))
                existingEmitter.Function = updatedEmitter.Function;

            if (!Equals(existingEmitter.NumberOfModes, updatedEmitter.NumberOfModes))
                existingEmitter.NumberOfModes = updatedEmitter.NumberOfModes;
        }

        private void UpdateMode(EmitterMode existingMode, EmitterMode updatedMode)
        {
            if (!Equals(existingMode.ModeName, updatedMode.ModeName))
                existingMode.ModeName = updatedMode.ModeName;

            if (!Equals(existingMode.AmplitudeMin, updatedMode.AmplitudeMin))
                existingMode.AmplitudeMin = updatedMode.AmplitudeMin;

            if (!Equals(existingMode.AmplitudeMax, updatedMode.AmplitudeMax))
                existingMode.AmplitudeMax = updatedMode.AmplitudeMax;

            if (!Equals(existingMode.TheoricalRangeMin, updatedMode.TheoricalRangeMin))
                existingMode.TheoricalRangeMin = updatedMode.TheoricalRangeMin;

            if (!Equals(existingMode.TheoricalRangeMax, updatedMode.TheoricalRangeMax))
                existingMode.TheoricalRangeMax = updatedMode.TheoricalRangeMax;

            // Update Beams and Pris within the Mode
            SynchronizeCollection(
                existingMode.Beams,
                updatedMode.Beams,
                UpdateBeam
            );

            SynchronizeCollection(
                existingMode.Pris,
                updatedMode.Pris,
                UpdatePri
            );
        }

        private void UpdateBeam(EmitterModeBeam existingBeam, EmitterModeBeam updatedBeam)
        {
            if (!Equals(existingBeam.BeamName, updatedBeam.BeamName))
                existingBeam.BeamName = updatedBeam.BeamName;

            if (!Equals(existingBeam.AntennaGainMin, updatedBeam.AntennaGainMin))
                existingBeam.AntennaGainMin = updatedBeam.AntennaGainMin;

            if (!Equals(existingBeam.AntennaGainMax, updatedBeam.AntennaGainMax))
                existingBeam.AntennaGainMax = updatedBeam.AntennaGainMax;

            if (!Equals(existingBeam.BeamPositionMin, updatedBeam.BeamPositionMin))
                existingBeam.BeamPositionMin = updatedBeam.BeamPositionMin;

            if (!Equals(existingBeam.BeamPositionMax, updatedBeam.BeamPositionMax))
                existingBeam.BeamPositionMax = updatedBeam.BeamPositionMax;

            // Update nested collections in Beam
            SynchronizeCollection(
                existingBeam.DwellDurationValues,
                updatedBeam.DwellDurationValues,
                UpdateDwellDuration
            );
        }

        private void UpdateDwellDuration(EmitterModeBeamPositionDwellDurationValue existingDwell, EmitterModeBeamPositionDwellDurationValue updatedDwell)
        {
            if (!Equals(existingDwell.BeamWPositionDurationMin, updatedDwell.BeamWPositionDurationMin))
                existingDwell.BeamWPositionDurationMin = updatedDwell.BeamWPositionDurationMin;

            if (!Equals(existingDwell.BeamWPositionDurationMax, updatedDwell.BeamWPositionDurationMax))
                existingDwell.BeamWPositionDurationMax = updatedDwell.BeamWPositionDurationMax;

            if (!Equals(existingDwell.BeamWPositionIndex, updatedDwell.BeamWPositionIndex))
                existingDwell.BeamWPositionIndex = updatedDwell.BeamWPositionIndex;

            // Update Firing Orders within Dwell Duration
            SynchronizeCollection(
                existingDwell.FiringOrders,
                updatedDwell.FiringOrders,
                UpdateFiringOrder
            );
        }

        private void UpdateFiringOrder(EmitterModeBeamPositionFiringOrder existingOrder, EmitterModeBeamPositionFiringOrder updatedOrder)
        {
            if (!Equals(existingOrder.BeamPositionOrderIndexMin, updatedOrder.BeamPositionOrderIndexMin))
                existingOrder.BeamPositionOrderIndexMin = updatedOrder.BeamPositionOrderIndexMin;

            if (!Equals(existingOrder.BeamPositionOrderIndexMax, updatedOrder.BeamPositionOrderIndexMax))
                existingOrder.BeamPositionOrderIndexMax = updatedOrder.BeamPositionOrderIndexMax;

            if (!Equals(existingOrder.ElevationMin, updatedOrder.ElevationMin))
                existingOrder.ElevationMin = updatedOrder.ElevationMin;

            if (!Equals(existingOrder.ElevationMax, updatedOrder.ElevationMax))
                existingOrder.ElevationMax = updatedOrder.ElevationMax;
        }

        private void UpdatePri(EmitterModePri existingPri, EmitterModePri updatedPri)
        {
            if (!Equals(existingPri.PriName, updatedPri.PriName))
                existingPri.PriName = updatedPri.PriName;

            if (!Equals(existingPri.PriLimitMin, updatedPri.PriLimitMin))
                existingPri.PriLimitMin = updatedPri.PriLimitMin;

            if (!Equals(existingPri.PriLimitMax, updatedPri.PriLimitMax))
                existingPri.PriLimitMax = updatedPri.PriLimitMax;

            // Update nested collections in Pri
            SynchronizeCollection(
                existingPri.SuperPeriods,
                updatedPri.SuperPeriods,
                UpdateSuperPeriod
            );
        }

        private void UpdateSuperPeriod(EmitterModePriSuperPeriodValue existingSuper, EmitterModePriSuperPeriodValue updatedSuper)
        {
            if (!Equals(existingSuper.SuperPeriodValueMin, updatedSuper.SuperPeriodValueMin))
                existingSuper.SuperPeriodValueMin = updatedSuper.SuperPeriodValueMin;

            if (!Equals(existingSuper.SuperPeriodValueMax, updatedSuper.SuperPeriodValueMax))
                existingSuper.SuperPeriodValueMax = updatedSuper.SuperPeriodValueMax;
        }

        // Helper function to synchronize collections
        private void SynchronizeCollection<T>(
            List<T> existingCollection,
            List<T> updatedCollection,
            Action<T, T> updateItemAction) where T : class, new()
        {
            // Update or Add items
            for (int i = 0; i < updatedCollection.Count; i++)
            {
                if (i < existingCollection.Count)
                {
                    updateItemAction(existingCollection[i], updatedCollection[i]);
                }
                else
                {
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