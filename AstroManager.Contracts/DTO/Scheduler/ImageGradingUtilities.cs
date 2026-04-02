namespace Shared.Model.DTO.Scheduler;

public static class ImageGradingMetricKeys
{
    public const string Hfr = "hfr";
    public const string Fwhm = "fwhm";
    public const string StarCount = "starCount";
    public const string Snr = "snr";
    public const string Eccentricity = "eccentricity";
    public const string GuidingRms = "guidingRms";
    public const string Altitude = "altitude";
    public const string Airmass = "airmass";
    public const string MoonDistance = "moonDistance";
    public const string MoonIllumination = "moonIllumination";
    public const string SkyQuality = "skyQuality";
    public const string WindSpeed = "windSpeed";
    public const string CloudCover = "cloudCover";
    public const string Humidity = "humidity";
    public const string Temperature = "temperature";
    public const string DewPoint = "dewPoint";
    public const string Pressure = "pressure";
    public const string BackgroundMean = "backgroundMean";
    public const string BackgroundNoise = "backgroundNoise";
    public const string MedianAdu = "medianAdu";
    public const string Adu = "adu";
}

public class ImageMetricDefinition
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public ImageMetricTrend DefaultTrend { get; set; }
    public double DefaultGoodThreshold { get; set; }
    public double DefaultBadThreshold { get; set; }
}

public enum ImageMetricTrend
{
    HigherIsBetter,
    LowerIsBetter
}

public class ImageMetricCriteria
{
    public string MetricKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public double Weight { get; set; } = 1;
    public ImageMetricTrend Trend { get; set; } = ImageMetricTrend.HigherIsBetter;
    public double GoodThreshold { get; set; }
    public double BadThreshold { get; set; }
    public bool UseDeviationScoring { get; set; }
    public double AllowedDeviationPercent { get; set; } = 20;
}

public class ImageAutoScoreProfile
{
    public List<ImageMetricCriteria> Metrics { get; set; } = new();
    public int OneMetricPenalty { get; set; } = 8;
    public int TwoMetricPenalty { get; set; } = 4;

    public static ImageAutoScoreProfile CreateDefault()
    {
        return new ImageAutoScoreProfile
        {
            Metrics = new()
            {
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.Hfr, Weight = 0.25, Trend = ImageMetricTrend.LowerIsBetter, GoodThreshold = 1.8, BadThreshold = 5.0 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.StarCount, Weight = 0.20, Trend = ImageMetricTrend.HigherIsBetter, GoodThreshold = 250, BadThreshold = 20 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.GuidingRms, Weight = 0.15, Trend = ImageMetricTrend.LowerIsBetter, GoodThreshold = 0.5, BadThreshold = 2.5 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.Altitude, Weight = 0.08, Trend = ImageMetricTrend.HigherIsBetter, GoodThreshold = 70, BadThreshold = 20 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.SkyQuality, Weight = 0.07, Trend = ImageMetricTrend.HigherIsBetter, GoodThreshold = 20.5, BadThreshold = 17.0 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.WindSpeed, Weight = 0.05, Trend = ImageMetricTrend.LowerIsBetter, GoodThreshold = 0.0, BadThreshold = 10.0 },
                new ImageMetricCriteria { MetricKey = ImageGradingMetricKeys.CloudCover, Weight = 0.04, Trend = ImageMetricTrend.LowerIsBetter, GoodThreshold = 5.0, BadThreshold = 70.0 }
            }
        };
    }
}

public class ImageGradeThresholds
{
    public int MinScoreForA { get; set; } = 90;
    public int MinScoreForB { get; set; } = 80;
    public int MinScoreForC { get; set; } = 70;
    public int MinScoreForD { get; set; } = 60;
}

public static class ImageGradingUtilities
{
    public static IReadOnlyList<ImageMetricDefinition> MetricDefinitions { get; } = new List<ImageMetricDefinition>
    {
        new() { Key = ImageGradingMetricKeys.Hfr, DisplayName = "HFR", Unit = "px", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 1.8, DefaultBadThreshold = 5.0 },
        new() { Key = ImageGradingMetricKeys.Fwhm, DisplayName = "FWHM", Unit = "arcsec", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 2.0, DefaultBadThreshold = 6.0 },
        new() { Key = ImageGradingMetricKeys.StarCount, DisplayName = "Star Count", Unit = "stars", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 250, DefaultBadThreshold = 20 },
        new() { Key = ImageGradingMetricKeys.Snr, DisplayName = "SNR", Unit = null, DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 25, DefaultBadThreshold = 5 },
        new() { Key = ImageGradingMetricKeys.Eccentricity, DisplayName = "Eccentricity", Unit = null, DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 0.35, DefaultBadThreshold = 0.8 },
        new() { Key = ImageGradingMetricKeys.GuidingRms, DisplayName = "Guiding RMS", Unit = "arcsec", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 0.5, DefaultBadThreshold = 2.5 },
        new() { Key = ImageGradingMetricKeys.Altitude, DisplayName = "Altitude", Unit = "deg", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 70, DefaultBadThreshold = 20 },
        new() { Key = ImageGradingMetricKeys.Airmass, DisplayName = "Airmass", Unit = null, DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 1.1, DefaultBadThreshold = 2.5 },
        new() { Key = ImageGradingMetricKeys.MoonDistance, DisplayName = "Moon Distance", Unit = "deg", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 90, DefaultBadThreshold = 20 },
        new() { Key = ImageGradingMetricKeys.MoonIllumination, DisplayName = "Moon Illumination", Unit = "%", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 10, DefaultBadThreshold = 80 },
        new() { Key = ImageGradingMetricKeys.SkyQuality, DisplayName = "Sky Quality", Unit = "mag/arcsec2", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 20.5, DefaultBadThreshold = 17.0 },
        new() { Key = ImageGradingMetricKeys.WindSpeed, DisplayName = "Wind Speed", Unit = "m/s", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 0, DefaultBadThreshold = 10 },
        new() { Key = ImageGradingMetricKeys.CloudCover, DisplayName = "Cloud Cover", Unit = "%", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 5, DefaultBadThreshold = 70 },
        new() { Key = ImageGradingMetricKeys.Humidity, DisplayName = "Humidity", Unit = "%", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 30, DefaultBadThreshold = 95 },
        new() { Key = ImageGradingMetricKeys.Temperature, DisplayName = "Temperature", Unit = "C", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 10, DefaultBadThreshold = -10 },
        new() { Key = ImageGradingMetricKeys.DewPoint, DisplayName = "Dew Point", Unit = "C", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = -10, DefaultBadThreshold = 12 },
        new() { Key = ImageGradingMetricKeys.Pressure, DisplayName = "Pressure", Unit = "hPa", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 1020, DefaultBadThreshold = 980 },
        new() { Key = ImageGradingMetricKeys.BackgroundMean, DisplayName = "Background Mean", Unit = "ADU", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 400, DefaultBadThreshold = 5000 },
        new() { Key = ImageGradingMetricKeys.BackgroundNoise, DisplayName = "Background Noise", Unit = "ADU", DefaultTrend = ImageMetricTrend.LowerIsBetter, DefaultGoodThreshold = 150, DefaultBadThreshold = 2000 },
        new() { Key = ImageGradingMetricKeys.MedianAdu, DisplayName = "Median ADU", Unit = "ADU", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 2500, DefaultBadThreshold = 500 },
        new() { Key = ImageGradingMetricKeys.Adu, DisplayName = "ADU", Unit = "ADU", DefaultTrend = ImageMetricTrend.HigherIsBetter, DefaultGoodThreshold = 2500, DefaultBadThreshold = 500 }
    };

    public static ImageMetricDefinition? GetMetricDefinition(string metricKey)
        => MetricDefinitions.FirstOrDefault(x => string.Equals(x.Key, metricKey, StringComparison.OrdinalIgnoreCase));

    public static int ClampScore(int score) => Math.Max(1, Math.Min(100, score));

    public static string? GetBandFromScore(int? score, ImageGradeThresholds? thresholds = null)
    {
        if (!score.HasValue)
            return null;

        var t = thresholds ?? new ImageGradeThresholds();
        var s = ClampScore(score.Value);

        if (s >= t.MinScoreForA) return "A";
        if (s >= t.MinScoreForB) return "B";
        if (s >= t.MinScoreForC) return "C";
        if (s >= t.MinScoreForD) return "D";
        return "E";
    }

    public static string NormalizeGradeBand(string? band)
    {
        var normalized = (band ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is "A" or "B" or "C" or "D" or "E" ? normalized : "E";
    }

    public static bool MeetsMinimumGradeBand(string? actualBand, string? minimumBand)
    {
        var minimum = NormalizeGradeBand(minimumBand);
        if (minimum == "E")
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(actualBand))
        {
            return true;
        }

        return GetGradeBandRank(NormalizeGradeBand(actualBand)) <= GetGradeBandRank(minimum);
    }

    private static int GetGradeBandRank(string band)
    {
        return band switch
        {
            "A" => 1,
            "B" => 2,
            "C" => 3,
            "D" => 4,
            _ => 5
        };
    }

    // Criteria-driven weighted model using a dynamic metric dictionary.
    // Profile can be user-defined and persisted in criteria-set configuration.
    public static int? CalculateAutoScore(
        IReadOnlyDictionary<string, double?> metricValues,
        ImageAutoScoreProfile? profile = null,
        IReadOnlyDictionary<string, double?>? goalBaselines = null)
    {
        if (metricValues == null)
            return null;

        profile ??= ImageAutoScoreProfile.CreateDefault();

        double weightedScore = 0;
        double weightSum = 0;

        void AddMetric(ImageMetricCriteria criteria)
        {
            if (criteria == null || !criteria.IsEnabled || criteria.Weight <= 0 || string.IsNullOrWhiteSpace(criteria.MetricKey))
                return;

            if (!metricValues.TryGetValue(criteria.MetricKey, out var value))
                return;

            if (!value.HasValue)
                return;

            var raw = value.Value;
            if (double.IsNaN(raw) || double.IsInfinity(raw) || raw < 0)
                return;

            var metricScore = ScoreMetric(raw, criteria, goalBaselines);
            weightedScore += metricScore * criteria.Weight;
            weightSum += criteria.Weight;
        }

        foreach (var criteria in profile.Metrics)
        {
            AddMetric(criteria);
        }

        if (weightSum <= 0)
            return null;

        var normalizedScore = weightedScore / weightSum;

        return ClampScore((int)Math.Round(normalizedScore));
    }

    // Backward-compatible overload for older call sites.
    public static int? CalculateAutoScore(
        double? hfr,
        int? starCount,
        double? snr,
        double? guidingRms,
        double? altitude,
        double? skyQuality,
        double? windSpeed,
        ImageAutoScoreProfile? profile = null)
    {
        var metricValues = new Dictionary<string, double?>
        {
            [ImageGradingMetricKeys.Hfr] = hfr,
            [ImageGradingMetricKeys.StarCount] = starCount,
            [ImageGradingMetricKeys.Snr] = snr,
            [ImageGradingMetricKeys.GuidingRms] = guidingRms,
            [ImageGradingMetricKeys.Altitude] = altitude,
            [ImageGradingMetricKeys.SkyQuality] = skyQuality,
            [ImageGradingMetricKeys.WindSpeed] = windSpeed
        };

        return CalculateAutoScore(metricValues, profile, null);
    }

    private static double ScoreMetric(
        double value,
        ImageMetricCriteria criteria,
        IReadOnlyDictionary<string, double?>? goalBaselines)
    {
        if (IsHardFailMetricValue(criteria.MetricKey, value))
        {
            return 0;
        }

        var absoluteScore = criteria.Trend == ImageMetricTrend.LowerIsBetter
            ? ScoreLowerIsBetter(value, good: criteria.GoodThreshold, bad: criteria.BadThreshold)
            : ScoreHigherIsBetter(value, bad: criteria.BadThreshold, good: criteria.GoodThreshold);

        if (criteria.UseDeviationScoring
            && !string.IsNullOrWhiteSpace(criteria.MetricKey)
            && goalBaselines != null
            && goalBaselines.TryGetValue(criteria.MetricKey, out var baselineValue)
            && baselineValue.HasValue
            && baselineValue.Value > 0)
        {
            var relativeChangePercent = ((value - baselineValue.Value) / baselineValue.Value) * 100.0;
            var deviationPercent = criteria.Trend == ImageMetricTrend.LowerIsBetter
                ? Math.Max(0, relativeChangePercent)
                : Math.Max(0, -relativeChangePercent);
            var allowedPercent = Math.Max(0.1, criteria.AllowedDeviationPercent);

            double deviationScore;
            if (deviationPercent <= allowedPercent)
            {
                deviationScore = 100;
            }

            else
            {
                var badPercent = Math.Max(allowedPercent + 0.1, allowedPercent * 2.0);
                deviationScore = ScoreLowerIsBetter(deviationPercent, good: allowedPercent, bad: badPercent);
            }

            // Deviation can hide globally poor conditions when the whole baseline is bad.
            // Keep absolute thresholds as a safety floor.
            return Math.Min(deviationScore, absoluteScore);
        }

        return absoluteScore;
    }

    private static bool IsHardFailMetricValue(string? metricKey, double value)
    {
        if (string.IsNullOrWhiteSpace(metricKey))
        {
            return false;
        }

        if (value <= 0)
        {
            return string.Equals(metricKey, ImageGradingMetricKeys.Hfr, StringComparison.OrdinalIgnoreCase)
                || string.Equals(metricKey, ImageGradingMetricKeys.StarCount, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static double ScoreLowerIsBetter(double value, double good, double bad)
    {
        if (Math.Abs(good - bad) < 0.000001)
            return value <= good ? 100 : 0;

        if (value <= good) return 100;
        if (value >= bad) return 0;
        return 100 * ((bad - value) / (bad - good));
    }

    private static double ScoreHigherIsBetter(double value, double bad, double good)
    {
        if (Math.Abs(good - bad) < 0.000001)
            return value >= good ? 100 : 0;

        if (value <= bad) return 0;
        if (value >= good) return 100;
        return 100 * ((value - bad) / (good - bad));
    }
}
