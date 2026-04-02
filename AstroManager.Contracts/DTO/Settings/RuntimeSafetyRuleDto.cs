using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings;

public enum RuntimeSafetyConditionOperator
{
    GreaterThan = 0,
    GreaterThanOrEqual = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    Equals = 4,
    NotEquals = 5
}

public class RuntimeSafetyMetricConditionDto
{
    [Required]
    [StringLength(100)]
    public string MetricKey { get; set; } = string.Empty;

    public RuntimeSafetyConditionOperator ConditionOperator { get; set; } = RuntimeSafetyConditionOperator.GreaterThan;

    /// <summary>
    /// Numeric threshold value for numeric metrics.
    /// </summary>
    public double? NumericValue { get; set; }

    /// <summary>
    /// Boolean threshold for boolean metrics.
    /// </summary>
    public bool? BoolValue { get; set; }
}

/// <summary>
/// Dynamic runtime safety rule definition (V2).
/// </summary>
public class RuntimeSafetyRuleDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Multiple metric conditions for this rule.
    /// Rule matches when ANY condition is true (OR).
    /// </summary>
    public List<RuntimeSafetyMetricConditionDto> Conditions { get; set; } = new();

    /// <summary>
    /// Legacy single-condition metric key. Kept for backward compatibility.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string MetricKey { get; set; } = string.Empty;

    /// <summary>
    /// Legacy single-condition operator. Kept for backward compatibility.
    /// </summary>
    public RuntimeSafetyConditionOperator ConditionOperator { get; set; } = RuntimeSafetyConditionOperator.GreaterThan;

    /// <summary>
    /// Legacy single-condition numeric threshold. Kept for backward compatibility.
    /// </summary>
    public double? NumericValue { get; set; }

    /// <summary>
    /// Legacy single-condition bool threshold. Kept for backward compatibility.
    /// </summary>
    public bool? BoolValue { get; set; }

    /// <summary>
    /// Actions to execute when this rule condition is met.
    /// Order matters.
    /// </summary>
    public List<SchedulerViolationAction> Actions { get; set; } = new();

    /// <summary>
    /// Optional per-action payload/configuration.
    /// </summary>
    public List<RuntimeSafetyActionConfigDto> ActionConfigs { get; set; } = new();

    public string? Notes { get; set; }
}

public class RuntimeSafetyActionConfigDto
{
    public SchedulerViolationAction Action { get; set; }

    /// <summary>
    /// Optional reconnect target used when Action is ReconnectEquipment.
    /// </summary>
    public RuntimeSafetyReconnectComponent? ReconnectComponent { get; set; }

    /// <summary>
    /// Optional wait override in minutes for retry-based actions.
    /// Applies to ParkAndRetry and StopTrackingAndRetry.
    /// </summary>
    [Range(1, 240)]
    public int? WaitMinutes { get; set; }

    /// <summary>
    /// When enabled for retry actions, the scheduler checks the rule periodically during the wait window
    /// and continues early as soon as the rule is no longer violated.
    /// </summary>
    public bool RetryWhenRuleClears { get; set; }

    /// <summary>
    /// Optional interval in minutes for in-wait rule re-checks.
    /// Applies only when <see cref="RetryWhenRuleClears"/> is enabled.
    /// </summary>
    [Range(1, 30)]
    public int? RetryCheckIntervalMinutes { get; set; }

    /// <summary>
    /// Optional signed cooler setpoint delta in °C.
    /// Applies to AdjustCoolerTemperatureDelta.
    /// </summary>
    [Range(-30, 30)]
    public double? CoolerDeltaC { get; set; }

    /// <summary>
    /// Optional email payload used when Action is SendEmail.
    /// </summary>
    public RuntimeSafetyEmailActionConfigDto? Email { get; set; }

    /// <summary>
    /// Optional notification payload used when Action is CreateNotification.
    /// </summary>
    public RuntimeSafetyNotificationActionConfigDto? Notification { get; set; }
}

public enum RuntimeSafetyReconnectComponent
{
    CriticalImaging = 0,
    Camera = 1,
    Mount = 2,
    Guider = 3,
    Focuser = 4,
    FilterWheel = 5,
    Rotator = 6,
    SafetyMonitor = 7,
    Weather = 8,
    Dome = 9
}

public class RuntimeSafetyEmailActionConfigDto
{
    /// <summary>
    /// Optional additional recipients.
    /// </summary>
    public List<string> AdditionalRecipients { get; set; } = new();

    [StringLength(200)]
    public string? SubjectTemplate { get; set; }

    [StringLength(4000)]
    public string? BodyTemplate { get; set; }
}

public class RuntimeSafetyNotificationActionConfigDto
{
    [StringLength(200)]
    public string? TitleTemplate { get; set; }

    [StringLength(1000)]
    public string? ContentTemplate { get; set; }
}
