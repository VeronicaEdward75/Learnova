using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;
using LearnNova.Services;
using LearnNova.Models.ViewModels.Student;
using System.Linq;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class StudentWalletController : Controller
{
    private readonly IWalletService _walletService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentWalletController(IWalletService walletService, UserManager<ApplicationUser> userManager)
    {
        _walletService = walletService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var wallet = await _walletService.GetWalletAsync(user.Id);
        var transactions = await _walletService.GetTransactionsAsync(wallet.Id);

        ViewBag.ActiveNav = "wallet";

        var viewModel = new StudentWalletViewModel
        {
            AvailableBalance = wallet.AvailableBalance,
            Transactions = transactions
        };

        return View(viewModel);
    }
}
