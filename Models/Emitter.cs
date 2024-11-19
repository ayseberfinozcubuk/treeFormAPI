using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using tree_form_API.Models.Interfaces;

namespace tree_form_API.Models
{
    public class Emitter : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string Notation { get; set; } = string.Empty; // Notasyon: Alphanumeric emitter notation
        public string EmitterName { get; set; } = string.Empty; // Emiter Adı
        public string? SpotNo { get; set; } // Spot No
        public string? Function { get; set; } // Görev Kodu
        public int? NumberOfModes { get; set; } // Mod Sayısı
        public List<EmitterMode> Modes { get; set; } = new List<EmitterMode>(); // Emiter Mod Listesi
    }

    public class EmitterMode : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        //[BsonId]
        //[BsonRepresentation(BsonType.String)]
        public Guid EmitterId { get; set; }
        public string ModeName { get; set; } = string.Empty; // Mode Adı
        public double? AmplitudeMin { get; set; } // Genlik (minivolt)
        public double? AmplitudeMax { get; set; } // Genlik (minivolt)
        public double? TheoricalRangeMin { get; set; } // Teorik Menzil (km)
        public double? TheoricalRangeMax { get; set; } // Teorik Menzil (km)
        public List<EmitterModeBeam> Beams { get; set; } = new List<EmitterModeBeam>(); // Emiter Mod Hüzme Listesi
        public List<EmitterModePri> Pris { get; set; } = new List<EmitterModePri>(); // Emiter Mod PRI Listesi
    }

    public class EmitterModePri : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModeId { get; set; }
        public string PriName { get; set; } = string.Empty; // Pri Adı
        public double? PriLimitMin { get; set; } // Pri Limitleri (us)
        public double? PriLimitMax { get; set; } // Pri Limitleri (us)
        public double? PrfLimitMin { get; set; } // Prf Limitleri (us)
        public double? PrfLimitMax { get; set; } // Prf Limitleri (us)
        public double? NominalPri { get; set; } // Nominal Pri Değeri (microsecond)
        public double? PriMeanMin { get; set; } // Ortalama Pri Değeri (microsecond)
        public double? PriMeanMax { get; set; } // Ortalama Pri Değeri (microsecond)
        public double? StandartDeviation { get; set; } // Standart Sapma (microsecond)
        public double? PrfMeanMin { get; set; } // Ortalama Prf Değeri (Hz)
        public double? PrfMeanMax { get; set; } // Ortalama Prf Değeri (Hz)
        public double? PulseToPulseMean { get; set; } // Palstan Palsa Standart Sapma (microsecond)
        public string? Continuity { get; set; } // Süreklilik (enum: Continuity)
        public string? Pattern { get; set; } // Desen (enum: Pattern)
        public List<EmitterModePriSuperPeriodValue> SuperPeriods { get; set; } = new List<EmitterModePriSuperPeriodValue>();
        public List<EmitterModePriMostProbableValue> MostProbableValues { get; set; } = new List<EmitterModePriMostProbableValue>();
        public List<EmitterModePriDiscreteValue> DiscreteValues { get; set; } = new List<EmitterModePriDiscreteValue>();
        public List<EmitterModePriSequence> Sequences { get; set; } = new List<EmitterModePriSequence>();
    }

    public class EmitterModePriSequence : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModePriId { get; set; }
        public string? SequenceName { get; set; } // Dizi Adı
        public int? NumberOfPulsesInSequence { get; set; } // Dizi İçerisindeki Pals Sayısı
        public double? TotalTimeForSequenceMin { get; set; } // Toplam Dizi Süresi (microsecond)
        public double? TotalTimeForSequenceMax { get; set; } // Toplam Dizi Süresi (microsecond)
        public List<EmitterModePriFiringOrder> FiringOrders { get; set; } = new List<EmitterModePriFiringOrder>();
    }

    public class EmitterModePriFiringOrder : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModePriSequenceId { get; set; }
        public Guid EmitterModePriDiscreteValueId { get; set; }
        public int OrderIndexMin { get; set; } // Gönderim Sıra No
        public int OrderIndexMax { get; set; } // Gönderim Sıra No

    }

    public class EmitterModePriDiscreteValue : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModePriId { get; set; }
        public double DiscreteValueMin { get; set; } // Ayrık Değer (microsecond)
        public double DiscreteValueMax { get; set; } // Ayrık Değer (microsecond)
        public double? DwellDurationMin { get; set; } // Dwell Süresi (microsecond)
        public double? DwellDurationMax { get; set; } // Dwell Süresi (microsecond)
        public double? TransitionRangeMin { get; set; } // Geçiş Süresi (microsecond)
        public double? TransitionRangeMax { get; set; } // Geçiş Süresi (microsecond)
        public List<EmitterModePriFiringOrder> FiringOrders { get; set; } = new List<EmitterModePriFiringOrder>();
    }

    public class EmitterModePriMostProbableValue : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModePriId { get; set; }
        public double MostProbableValueMin { get; set; } // En Yüksel Olasılıklı Değer (microsecond)
        public double MostProbableValueMax { get; set; } // En Yüksel Olasılıklı Değer (microsecond)
    }

    public class EmitterModePriSuperPeriodValue : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModePriId { get; set; }
        public double SuperPeriodValueMin { get; set; } // Alt Periyod Değeri (microsecond)
        public double SuperPeriodValueMax { get; set; } // Alt Periyod Değeri (microsecond)
    }

    public class EmitterModeBeam : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModeId { get; set; }
        public string BeamName { get; set; } = string.Empty; // Beam Adı
        public double? AntennaGainMin { get; set; } // Anten Kazancı (dbi)
        public double? AntennaGainMax { get; set; } // Anten Kazancı (dbi)
        public double? BeamPositionMin { get; set; } // Hüzme Pozisyonu (degree)
        public double? BeamPositionMax { get; set; } // Hüzme Pozisyonu (degree)
        public double? BeamWidthAzimuteMin { get; set; } // Hüzme Genişliği Yatay (degree)
        public double? BeamWidthAzimuteMax { get; set; } // Hüzme Genişliği Yatay (degree)
        public double? BeamWidthElevationMin { get; set; } // Hüzme Genişliği Dikey (degree)
        public double? BeamWidthElevationMax { get; set; } // Hüzme Genişliği Dikey (degree)
        public List<EmitterModeBeamPositionDwellDurationValue> DwellDurationValues { get; set; } = new List<EmitterModeBeamPositionDwellDurationValue>();
        public List<EmitterModeBeamPositionSequence> Sequences { get; set; } = new List<EmitterModeBeamPositionSequence>();
    }

    public class EmitterModeBeamPositionDwellDurationValue : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModeBeamId { get; set; }
        public double? BeamWPositionDurationMin { get; set; }  // Hüzme Pozisyonu Kalış Süresi
        public double? BeamWPositionDurationMax { get; set; }  // Hüzme Pozisyonu Kalış Süresi
        public int BeamWPositionIndex { get; set; } // Sıra No
        public List<EmitterModeBeamPositionFiringOrder> FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrder>();  // Dizi Elemanı Listesi
    }

    public class EmitterModeBeamPositionSequence : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModeBeamId { get; set; }
        public string? SequenceName { get; set; }  // Parametre Dizisi Adı
        public List<EmitterModeBeamPositionFiringOrder> FiringOrders { get; set; } = new List<EmitterModeBeamPositionFiringOrder>();  // Dizi Elemanı Listesi
    }

    public class EmitterModeBeamPositionFiringOrder : IIdentifiable
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid EmitterModeBeamPositionSequenceId { get; set; }
        public Guid EmitterModeBeamPositionDwellDurationValueId { get; set; }
        public int BeamPositionOrderIndexMin { get; set; }  // Gönderim Sıra No
        public int BeamPositionOrderIndexMax { get; set; }  // Gönderim Sıra No
        public int BeamPositionIndexMin { get; set; }  // Hüzme Ayrık Pozisyonu Sıra No
        public int BeamPositionIndexMax { get; set; }  // Hüzme Ayrık Pozisyonu Sıra No
        public int BeamPositionDurationMin { get; set; }  // Hüzme Pozisyonu Kalış Süresi
        public int BeamPositionDurationMax { get; set; }  // Hüzme Pozisyonu Kalış Süresi
        public double? ElevationMin { get; set; }  // Dikey Açı
        public double? ElevationMax { get; set; }  // Dikey Açı
        public double? AzimuthMin { get; set; }  // Yatay Açı
        public double? AzimuthMax { get; set; }  // Yatay Açı
    }
}
