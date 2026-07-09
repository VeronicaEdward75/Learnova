using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Admin;

public class WithdrawalActionViewModel
{
    [Required]
    public int RequestId { get; set; }

    public string? AdminNotes { get; set; }
}
