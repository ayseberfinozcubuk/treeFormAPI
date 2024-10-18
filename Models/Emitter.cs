using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tree_form_API.Models
{
    public class Emitter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public string Notation { get; set; } = string.Empty;
        public string EmitterName { get; set; } = string.Empty;
        public string? SpotNo { get; set; }
        public string? Function { get; set; }
        public int? NumberOfModes { get; set; }
        public List<EmitterMode> Modes { get; set; } = new List<EmitterMode>();
    }

    public class EmitterMode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public Guid EmitterId { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public double? Amplitude { get; set; }
        public double? TheoricalRange { get; set; }
        public List<EmitterModeBeam> Beams { get; set; } = new List<EmitterModeBeam>();
    }

    public class EmitterModeBeam
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public Guid EmitterId { get; set; }
        public string BeamName { get; set; } = string.Empty;
        public double? AntennaGain { get; set; }
        public double? BeamPosition { get; set; }
        public double? BeamWidthAzimute { get; set; }
        public double? BeamWidthElevation { get; set; }
        public List<EmitterModeBeamPositionDwellDurationValue> DwellDurationValues { get; set; } = new List<EmitterModeBeamPositionDwellDurationValue>();
        public List<EmitterModeBeamPositionSequence> Sequences { get; set; } = new List<EmitterModeBeamPositionSequence>();
    }

    public class EmitterModeBeamPositionDwellDurationValue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamId { get; set; }
        public double? BeamWPositionDuration { get; set; }
        public int BeamWPositionIndex { get; set; }
    }

    public class EmitterModeBeamPositionSequence
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamId { get; set; }
        public string SequenceName { get; set; } = string.Empty;
        public List<EmitterModeBeamPositionFiringOrder> FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrder>();
    }

    public class EmitterModeBeamPositionFiringOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamPositionSequenceId { get; set; }
        public int BeamPositionOrderIndex { get; set; }
        public int BeamPositionIndex { get; set; }
        public int BeamPositionDuration { get; set; }
        public double? Elevation { get; set; }
        public double? Azimuth { get; set; }
    }
}
