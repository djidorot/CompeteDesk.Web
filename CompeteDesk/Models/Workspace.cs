using System;
using System.ComponentModel.DataAnnotations;

namespace CompeteDesk.Models;

/// <summary>
/// A Workspace is the top-level container in CompeteDesk.
/// Everything else (Strategies, Actions, Habits, Metrics, War Room intel) will hang off a Workspace.
/// </summary>
public class Workspace
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// IdentityUser.Id of the owner.
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
