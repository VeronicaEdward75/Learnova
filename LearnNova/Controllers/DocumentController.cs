using System.Threading.Tasks;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;
using LearnNova.Repositories;

namespace LearnNova.Controllers;

[Authorize]
public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IGenericRepository<Certificate> _certRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public DocumentController(IDocumentService documentService, IPaymentRepository paymentRepo, IGenericRepository<Certificate> certRepo, UserManager<ApplicationUser> userManager)
    {
        _documentService = documentService;
        _paymentRepo = paymentRepo;
        _certRepo = certRepo;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        if (payment == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        // Security: Students can only download their own invoices
        if (payment.StudentId != userId && !isAdmin) return Forbid();

        var pdfBytes = await _documentService.GenerateInvoicePdfAsync(id);
        return File(pdfBytes, "application/pdf", "Invoice_" + (payment.Invoice?.InvoiceNumber ?? id.ToString()) + ".pdf");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadReceipt(int id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        if (payment == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        if (payment.StudentId != userId && !isAdmin) return Forbid();

        var pdfBytes = await _documentService.GenerateReceiptPdfAsync(id);
        return File(pdfBytes, "application/pdf", "Receipt_" + id + ".pdf");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCertificate(int id)
    {
        var cert = await _certRepo.GetByIdAsync(id);
        if (cert == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        if (cert.StudentId != userId && !isAdmin) return Forbid();

        var pdfBytes = await _documentService.GenerateCertificatePdfAsync(id);
        return File(pdfBytes, "application/pdf", "Certificate_" + cert.CertificateNumber + ".pdf");
    }
}
