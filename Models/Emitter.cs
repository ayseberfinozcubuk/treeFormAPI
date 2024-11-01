using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tree_form_API.Models
{
    public class Emitter
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Notation { get; set; } = string.Empty; // Notasyon: Alphanumeric emitter notation
        public string EmitterName { get; set; } = string.Empty; // Emiter Adı
        public string? SpotNo { get; set; } // Spot No
        public string? Function { get; set; } // Görev Kodu
        public int? NumberOfModes { get; set; } // Mod Sayısı
        public List<EmitterMode> Modes { get; set; } = new List<EmitterMode>(); // Emiter Mod Listesi
    }

    public class EmitterMode
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterId { get; set; }
        public string ModeName { get; set; } = string.Empty; // Mode Adı
        public double? Amplitude { get; set; } // Genlik (minivolt)
        public double? TheoricalRange { get; set; } // Teorik Menzil (km)
        public List<EmitterModeBeam> Beams { get; set; } = new List<EmitterModeBeam>(); // Emiter Mod Hüzme Listesi
        public List<EmitterModePri> Pris { get; set; } = new List<EmitterModePri>(); // Emiter Mod PRI Listesi
    }

    public class EmitterModePri
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModeId { get; set; }
        public string PriName { get; set; } = string.Empty; // Pri Adı
        public double? PriLimit { get; set; } // Pri Limitleri (us)
        public double? PrfLimit { get; set; } // Prf Limitleri (us)
        public double? NominalPri { get; set; } // Nominal Pri Değeri (microsecond)
        public double? PriMean { get; set; } // Ortalama Pri Değeri (microsecond)
        public double? StandartDeviation { get; set; } // Standart Sapma (microsecond)
        public double? PrfMean { get; set; } // Ortalama Prf Değeri (Hz)
        public double? PulseToPulseMean { get; set; } // Palstan Palsa Standart Sapma (microsecond)
        public string? Continuity { get; set; } // Süreklilik (enum: Continuity)
        public string? Pattern { get; set; } // Desen (enum: Pattern)
        public List<EmitterModePriSuperPeriodValue> SuperPeriods { get; set; } = new List<EmitterModePriSuperPeriodValue>();
        public List<EmitterModePriMostProbableValue> MostProbableValues { get; set; } = new List<EmitterModePriMostProbableValue>();
        public List<EmitterModePriDiscreteValue> DiscreteValues { get; set; } = new List<EmitterModePriDiscreteValue>();
        public List<EmitterModePriSequence> Sequences { get; set; } = new List<EmitterModePriSequence>();
    }

    public class EmitterModePriSequence
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModePriId { get; set; }
        public string? SequenceName { get; set; } // Dizi Adı
        public int? NumberOfPulsesInSequence { get; set; } // Dizi İçerisindeki Pals Sayısı
        public double? TotalTimeForSequence { get; set; } // Toplam Dizi Süresi (microsecond)
        public List<EmitterModePriFiringOrder> FiringOrders { get; set; } = new List<EmitterModePriFiringOrder>();
    }

    public class EmitterModePriFiringOrder
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModePriSequenceId { get; set; }

        public Guid EmitterModePriDiscreteValueId { get; set; }

        public int OrderIndex { get; set; } // Gönderim Sıra No
    }

    public class EmitterModePriDiscreteValue
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModePriId { get; set; }
        public double DiscreteValue { get; set; } // Ayrık Değer (microsecond)
        public double? DwellDuration { get; set; } // Dwell Süresi (microsecond)
        public double? TransitionRange { get; set; } // Geçiş Süresi (microsecond)
        public List<EmitterModePriFiringOrder> FiringOrders { get; set; } = new List<EmitterModePriFiringOrder>();
    }

    public class EmitterModePriMostProbableValue
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModePriId { get; set; }
        public double MostProbableValue { get; set; } // En Yüksel Olasılıklı Değer (microsecond)
    }

    public class EmitterModePriSuperPeriodValue
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModePriId { get; set; }
        public double SuperPeriodValue { get; set; } // Alt Periyod Değeri (microsecond)
    }

    public class EmitterModeBeam
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterId { get; set; }
        public string BeamName { get; set; } = string.Empty; // Beam Adı
        public double? AntennaGain { get; set; } // Anten Kazancı (dbi)
        public double? BeamPosition { get; set; } // Hüzme Pozisyonu (degree)
        public double? BeamWidthAzimute { get; set; } // Hüzme Genişliği Yatay (degree)
        public double? BeamWidthElevation { get; set; } // Hüzme Genişliği Dikey (degree)
        public List<EmitterModeBeamPositionDwellDurationValue> DwellDurationValues { get; set; } = new List<EmitterModeBeamPositionDwellDurationValue>();
        public List<EmitterModeBeamPositionSequence> Sequences { get; set; } = new List<EmitterModeBeamPositionSequence>();
    }

    public class EmitterModeBeamPositionDwellDurationValue
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamId { get; set; }
        public double? BeamWPositionDuration { get; set; }  // Hüzme Pozisyonu Kalış Süresi
        public int BeamWPositionIndex { get; set; } // Sıra No
        public List<EmitterModeBeamPositionFiringOrder> FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrder>();  // Dizi Elemanı Listesi
    }

    public class EmitterModeBeamPositionSequence
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamId { get; set; }
        public string? SequenceName { get; set; }  // Parametre Dizisi Adı
        public List<EmitterModeBeamPositionFiringOrder> FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrder>();  // Dizi Elemanı Listesi
    }

    public class EmitterModeBeamPositionFiringOrder
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid EmitterModeBeamPositionSequenceId { get; set; }

        public Guid EmitterModeBeamPositionDwellDurationValueId { get; set; }

        public int BeamPositionOrderIndex { get; set; }  // Gönderim Sıra No

        public int BeamPositionIndex { get; set; }  // Hüzme Ayrık Pozisyonu Sıra No

        public int BeamPositionDuration { get; set; }  // Hüzme Pozisyonu Kalış Süresi

        public double? Elevation { get; set; }  // Dikey Açı

        public double? Azimuth { get; set; }  // Yatay Açı
    }
}
