using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Client;

public class RuntimeSafetyEmailAlertDto
{
    [Required]
    [StringLength(2000)]
    public string Violation { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MetricKey { get; set; }

    [StringLength(200)]
    public string? Subject { get; set; }

    [StringLength(4000)]
    public string? Body { get; set; }

    public List<string> AdditionalRecipients { get; set; } = new();
}

public class RuntimeSafetyNotificationAlertDto
{
    [Required]
    [StringLength(2000)]
    public string Violation { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MetricKey { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Text { get; set; }
}
