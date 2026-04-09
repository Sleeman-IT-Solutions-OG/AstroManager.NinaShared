using System.ComponentModel.DataAnnotations;
using Shared.Model.Enums;

namespace Shared.Model.DTO.Scheduler;

public class SchedulerWeightedCriterionDto
{
    public SchedulerWeightedMetric Metric { get; set; }

    [Range(0, 100, ErrorMessage = "Weight must be between 0 and 100")]
    public int Weight { get; set; }

    /// <summary>
    /// True when larger values should score better; false when smaller values should score better.
    /// </summary>
    public bool PreferHigherValues { get; set; } = true;

    /// <summary>
    /// Value at or below/above which the criterion contributes zero points, depending on preference.
    /// </summary>
    public double? ZeroScoreThreshold { get; set; }

    /// <summary>
    /// Value at or above/below which the criterion contributes full points, depending on preference.
    /// </summary>
    public double? FullScoreThreshold { get; set; }
}

public class SchedulerScoreContributionDto
{
    public SchedulerWeightedMetric Metric { get; set; }
    public double Value { get; set; }
    public int Weight { get; set; }
    public double NormalizedScore { get; set; }
    public double WeightedScore { get; set; }
    public bool PreferHigherValues { get; set; }
    public double? ZeroScoreThreshold { get; set; }
    public double? FullScoreThreshold { get; set; }
    public string? DisplayValue { get; set; }
}
