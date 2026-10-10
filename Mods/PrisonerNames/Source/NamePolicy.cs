using System;
using System.Collections.Generic;
using System.Globalization;

namespace Odf.PrisonerNames
{
    public enum NameStyle
    {
        Code,
        Short,
        Full,
        First,
        Last
    }

    public sealed class RacePrefix
    {
        public string Race = string.Empty;
        public string Prefix = string.Empty;
    }

    public sealed class GradeBand
    {
        public double Minimum;
        public string Name = string.Empty;
    }

    public sealed class NamePolicy
    {
        public const double DefaultNeutralScore = 0d;
        public const string DefaultGrade = "Neutral";
        public const string DefaultChineseGrade = "普通";

        public string Pattern = "{race_prefix}{number_text}";
        public string ChinesePattern = "{race_prefix}{number_text}";
        public NameStyle Style = NameStyle.Code;
        public string DefaultPrefix = "P";
        public string ChineseDefaultPrefix = "囚";
        public double NeutralScore = DefaultNeutralScore;
        public double ResistancePerPoint;
        public double CustodyDayScore;
        public double PrisonBreakScore;
        public List<RacePrefix> Races = new List<RacePrefix>();
        public List<GradeBand> Grades = new List<GradeBand>();

        public static NamePolicy CreateDefault()
        {
            var policy = new NamePolicy();
            policy.Races.Add(new RacePrefix { Race = "Human", Prefix = "P" });
            policy.Races.Add(new RacePrefix { Race = "Ratkin", Prefix = "RK" });
            policy.Grades.Add(new GradeBand { Minimum = double.NegativeInfinity, Name = DefaultGrade });
            policy.Grades.Add(new GradeBand { Minimum = 20d, Name = "Noted" });
            policy.Grades.Add(new GradeBand { Minimum = 50d, Name = "Valued" });
            return policy;
        }

        public NamePolicy Clone()
        {
            var copy = new NamePolicy
            {
                Pattern = Pattern,
                ChinesePattern = ChinesePattern,
                Style = Style,
                DefaultPrefix = DefaultPrefix,
                ChineseDefaultPrefix = ChineseDefaultPrefix,
                NeutralScore = NeutralScore,
                ResistancePerPoint = ResistancePerPoint,
                CustodyDayScore = CustodyDayScore,
                PrisonBreakScore = PrisonBreakScore
            };
            for (int i = 0; i < Races.Count; i++)
                copy.Races.Add(new RacePrefix { Race = Races[i].Race, Prefix = Races[i].Prefix });
            for (int i = 0; i < Grades.Count; i++)
                copy.Grades.Add(new GradeBand { Minimum = Grades[i].Minimum, Name = Grades[i].Name });
            return copy;
        }

        public string PrefixFor(string race, bool chinese)
        {
            if (!string.IsNullOrEmpty(race))
            {
                for (int i = 0; i < Races.Count; i++)
                    if (string.Equals(Races[i].Race, race, StringComparison.Ordinal))
                        return Races[i].Prefix ?? string.Empty;
            }
            return chinese ? ChineseDefaultPrefix ?? string.Empty : DefaultPrefix ?? string.Empty;
        }

        public string GradeFor(double score)
        {
            if (double.IsNaN(score) || double.IsInfinity(score)) score = NeutralScore;
            GradeBand best = null;
            for (int i = 0; i < Grades.Count; i++)
            {
                GradeBand band = Grades[i];
                if (double.IsNaN(band.Minimum)) continue;
                if (score < band.Minimum) continue;
                if (best == null || band.Minimum > best.Minimum
                    || (band.Minimum == best.Minimum && string.CompareOrdinal(band.Name, best.Name) < 0))
                    best = band;
            }
            return best == null || string.IsNullOrEmpty(best.Name) ? DefaultGrade : best.Name;
        }

        public double? NextThreshold(double score)
        {
            if (double.IsNaN(score) || double.IsInfinity(score)) score = NeutralScore;
            double? next = null;
            for (int i = 0; i < Grades.Count; i++)
            {
                double minimum = Grades[i].Minimum;
                if (double.IsNaN(minimum) || double.IsInfinity(minimum)) continue;
                if (minimum <= score) continue;
                if (next == null || minimum < next.Value) next = minimum;
            }
            return next;
        }

        public static bool TryParseStyle(string text, out NameStyle style)
        {
            style = NameStyle.Code;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (string.Equals(text, "code", StringComparison.OrdinalIgnoreCase)) { style = NameStyle.Code; return true; }
            if (string.Equals(text, "short", StringComparison.OrdinalIgnoreCase)) { style = NameStyle.Short; return true; }
            if (string.Equals(text, "full", StringComparison.OrdinalIgnoreCase)) { style = NameStyle.Full; return true; }
            if (string.Equals(text, "first", StringComparison.OrdinalIgnoreCase)) { style = NameStyle.First; return true; }
            if (string.Equals(text, "last", StringComparison.OrdinalIgnoreCase)) { style = NameStyle.Last; return true; }
            return false;
        }

        public static string StyleName(NameStyle style)
        {
            switch (style)
            {
                case NameStyle.Short: return "short";
                case NameStyle.Full: return "full";
                case NameStyle.First: return "first";
                case NameStyle.Last: return "last";
                default: return "code";
            }
        }

        public static string FormatNumber(double value)
        {
            if (double.IsNegativeInfinity(value)) return "…";
            if (double.IsNaN(value) || double.IsInfinity(value)) return "0";
            if (value == Math.Truncate(value) && value >= int.MinValue && value <= int.MaxValue)
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
