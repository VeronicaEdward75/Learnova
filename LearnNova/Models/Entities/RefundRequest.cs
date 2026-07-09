using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class RefundRequest
{
    public int Id { get; set; }
    
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;
    
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }
    
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
