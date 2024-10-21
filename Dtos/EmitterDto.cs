using System.Collections.Generic;

namespace tree_form_API.Dtos
{
    public class EmitterDto
    {
        public string Notation { get; set; } = string.Empty;
        public string EmitterName { get; set; } = string.Empty;
        public string? SpotNo { get; set; }
        public string? Function { get; set; }
        public int? NumberOfModes { get; set; }
        public List<EmitterModeDto>? Modes { get; set; } = new List<EmitterModeDto>();
    }

    public class EmitterModeDto
    {
        public string ModeName { get; set; } = string.Empty;
        public double? Amplitude { get; set; }
        public double? TheoricalRange { get; set; }
        public List<EmitterModeBeamDto>? Beams { get; set; } = new List<EmitterModeBeamDto>();
    }

    public class EmitterModeBeamDto
    {
        public string BeamName { get; set; } = string.Empty;
        public double? AntennaGain { get; set; }
        public double? BeamPosition { get; set; }
        public double? BeamWidthAzimute { get; set; }
        public double? BeamWidthElevation { get; set; }
        public List<EmitterModeBeamPositionDwellDurationValueDto>? DwellDurationValues { get; set; } = new List<EmitterModeBeamPositionDwellDurationValueDto>();
        public List<EmitterModeBeamPositionSequenceDto>? Sequences { get; set; } = new List<EmitterModeBeamPositionSequenceDto>();
    }

    public class EmitterModeBeamPositionDwellDurationValueDto
    {
        public double? BeamWPositionDuration { get; set; }
        public int BeamWPositionIndex { get; set; }
    }

    public class EmitterModeBeamPositionSequenceDto
    {
        public string SequenceName { get; set; } = string.Empty;
        public List<EmitterModeBeamPositionFiringOrderDto>? FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrderDto>();
    }

    public class EmitterModeBeamPositionFiringOrderDto
    {
        public int BeamPositionOrderIndex { get; set; }
        public int BeamPositionIndex { get; set; }
        public int BeamPositionDuration { get; set; }
        public double? Elevation { get; set; }
        public double? Azimuth { get; set; }
    }
}
