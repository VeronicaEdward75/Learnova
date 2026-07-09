using System;
using System.IO;
using System.Threading.Tasks;
using LearnNova.Repositories;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LearnNova.Services;

public class DocumentService : IDocumentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IGenericRepository<LearnNova.Models.Entities.Certificate> _certRepo;
    private readonly IRefundService _refundService;

    public DocumentService(IPaymentRepository paymentRepo, IGenericRepository<LearnNova.Models.Entities.Certificate> certRepo, IRefundService refundService)
    {
        _paymentRepo = paymentRepo;
        _certRepo = certRepo;
        _refundService = refundService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int paymentId)
    {
        var payment = await _paymentRepo.GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == paymentId);
            
        if (payment == null) throw new Exception("Payment not found");

        var details = await _refundService.GetOrderDetailsAsync(paymentId, payment.StudentId);
        if (details == null) throw new Exception("Order details not found");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                // SECTION 1: Header
                page.Header().Element(header => 
                {
                    header.Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("LearnNova").FontSize(28).SemiBold().FontColor("#4338CA");
                                c.Item().Text("Premium Learning Platform").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Text("INVOICE").FontSize(20).Bold().FontColor("#4338CA").AlignRight();
                                c.Item().Text($"Invoice #: {details.InvoiceNumber ?? "N/A"}").FontSize(10).FontColor(Colors.Grey.Darken2).AlignRight();
                                c.Item().Text($"Issue Date: {details.PurchaseDate:yyyy-MM-dd}").FontSize(10).FontColor(Colors.Grey.Medium).AlignRight();
                                c.Item().Text($"Payment Date: {details.PurchaseDate:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium).AlignRight();
                                c.Item().Text($"Order #: {details.PaymentId}").FontSize(10).FontColor(Colors.Grey.Darken2).AlignRight();
                            });
                        });
                        col.Item().PaddingVertical(15).LineHorizontal(2).LineColor("#4338CA");
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        // SECTIONS 2 & 4: Student & Payment Info
                        col.Item().Row(row =>
                        {
                            // Student Info
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Billed To:").SemiBold().FontColor("#4338CA").FontSize(12);
                                c.Item().PaddingBottom(2).Text(details.StudentName).FontSize(12).Bold();
                                c.Item().Text(details.StudentEmail).FontColor(Colors.Grey.Darken2);
                                c.Item().Text("LearnNova Student").FontColor(Colors.Grey.Medium);
                            });
                            
                            // Payment Info
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Payment Info:").SemiBold().FontColor("#4338CA").FontSize(12).AlignRight();
                                c.Item().PaddingBottom(2).Text($"Method: {details.PaymentMethod}").FontSize(11).AlignRight();
                                c.Item().Text($"Transaction ID: {details.TransactionId ?? "N/A"}").FontColor(Colors.Grey.Darken2).AlignRight();
                                c.Item().Text($"Status: {details.PaymentStatus}").FontColor(Colors.Grey.Darken2).AlignRight();
                                c.Item().Text($"Currency: {details.Currency}").FontColor(Colors.Grey.Darken2).AlignRight();
                            });
                        });

                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // SECTION 3: Course Info
                        col.Item().Text("Service Details").SemiBold().FontColor("#4338CA").FontSize(12);
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Course Name
                                columns.RelativeColumn(2); // Instructor
                                columns.RelativeColumn(2); // Category & Level
                                columns.RelativeColumn(1); // Date
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#f3f4f6").Padding(5).Text("Course Name").SemiBold();
                                header.Cell().Background("#f3f4f6").Padding(5).Text("Instructor").SemiBold();
                                header.Cell().Background("#f3f4f6").Padding(5).Text("Category - Level").SemiBold();
                                header.Cell().Background("#f3f4f6").Padding(5).AlignRight().Text("Date").SemiBold();
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(details.CourseTitle).SemiBold();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(details.TeacherName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{details.CourseCategory} - {details.CourseLevel}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{details.PurchaseDate:yyyy-MM-dd}");
                        });

                        col.Item().PaddingVertical(15);

                        // Layout for Coupon/Refund (Left) and Financials (Right)
                        col.Item().Row(row =>
                        {
                            // Left Column: Coupon & Refund
                            row.RelativeItem(5).Column(c =>
                            {
                                // SECTION 6: Coupon Info
                                c.Item().Text("Coupon Details").SemiBold().FontColor("#4338CA").FontSize(11);
                                c.Item().PaddingVertical(5).Background("#f8fafc").Padding(10).Column(cc =>
                                {
                                    if (!string.IsNullOrEmpty(details.CouponCode))
                                    {
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Coupon Code:"); r.RelativeItem().AlignRight().Text(details.CouponCode).SemiBold().FontColor("#4338CA"); });
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Discount Type:"); r.RelativeItem().AlignRight().Text(details.CouponDiscountType); });
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Discount Value:"); r.RelativeItem().AlignRight().Text(details.CouponDiscountValue.ToString("N2")).FontColor(Colors.Red.Medium); });
                                        cc.Item().Row(r => { r.RelativeItem().Text("Saved Amount:"); r.RelativeItem().AlignRight().Text($"{details.DiscountAmount:N2} {details.Currency}").SemiBold().FontColor(Colors.Green.Medium); });
                                    }
                                    else
                                    {
                                        cc.Item().Text("No Coupon Applied").FontColor(Colors.Grey.Medium).AlignCenter();
                                    }
                                });

                                c.Item().PaddingTop(10).Text("Refund Information").SemiBold().FontColor("#4338CA").FontSize(11);
                                c.Item().PaddingVertical(5).Background("#f8fafc").Padding(10).Column(cc =>
                                {
                                    if (details.HasRefundRequest)
                                    {
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Status:"); r.RelativeItem().AlignRight().Text(details.RefundStatus.ToString()).SemiBold(); });
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Date:"); r.RelativeItem().AlignRight().Text(details.RefundDate?.ToString("yyyy-MM-dd") ?? "N/A"); });
                                        cc.Item().PaddingBottom(2).Row(r => { r.RelativeItem().Text("Amount:"); r.RelativeItem().AlignRight().Text($"{details.RefundAmount:N2} {details.Currency}"); });
                                        cc.Item().Row(r => { r.RelativeItem().Text("Admin Decision:"); r.RelativeItem().AlignRight().Text(string.IsNullOrEmpty(details.RefundAdminNotes) ? "N/A" : details.RefundAdminNotes).FontSize(9); });
                                    }
                                    else
                                    {
                                        cc.Item().Text("No refund requested.").FontColor(Colors.Grey.Medium).AlignCenter();
                                    }
                                });
                            });

                            row.ConstantItem(20).Text(""); // Spacer

                            // Right Column: SECTION 5: Financial Breakdown
                            row.RelativeItem(5).Column(c =>
                            {
                                c.Item().Text("Financial Breakdown").SemiBold().FontColor("#4338CA").FontSize(12).AlignRight();
                                c.Item().PaddingTop(5).Table(t =>
                                {
                                    t.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn();
                                        cols.ConstantColumn(80);
                                    });

                                    t.Cell().PaddingVertical(3).Text("Course Price").FontColor(Colors.Grey.Darken2);
                                    t.Cell().PaddingVertical(3).AlignRight().Text(details.OriginalPrice.ToString("N2")).SemiBold();

                                    t.Cell().PaddingVertical(3).Text("Coupon Discount").FontColor(Colors.Grey.Darken2);
                                    t.Cell().PaddingVertical(3).AlignRight().Text($"- {details.DiscountAmount:N2}").SemiBold().FontColor(Colors.Green.Medium);

                                    t.Cell().PaddingVertical(3).Text("Wallet Used").FontColor(Colors.Grey.Darken2);
                                    t.Cell().PaddingVertical(3).AlignRight().Text(details.WalletPaid.ToString("N2")).SemiBold();

                                    t.Cell().PaddingVertical(3).Text("Gateway Payment").FontColor(Colors.Grey.Darken2);
                                    t.Cell().PaddingVertical(3).AlignRight().Text(details.GatewayPaid.ToString("N2")).SemiBold();

                                    t.Cell().PaddingVertical(3).Text("Taxes (VAT)").FontColor(Colors.Grey.Darken2);
                                    t.Cell().PaddingVertical(3).AlignRight().Text(details.VatAmount.ToString("N2")).SemiBold();

                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text("Platform Fees").FontColor(Colors.Grey.Darken2);
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).AlignRight().Text(details.PlatformFee.ToString("N2")).SemiBold();

                                    t.Cell().Background("#f3f4f6").Padding(5).Text("Final Amount Paid").SemiBold().FontColor("#4338CA").FontSize(12);
                                    t.Cell().Background("#f3f4f6").Padding(5).AlignRight().Text($"{details.TotalPaid:N2} {details.Currency}").SemiBold().FontColor("#4338CA").FontSize(12);
                                });
                            });
                        });
                    });
                });

                // SECTION 8: Footer
                page.Footer().Element(footer => 
                {
                    footer.Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(10).AlignCenter().Text("Thank you for choosing LearnNova.").FontColor(Colors.Grey.Darken2).SemiBold();
                        col.Item().PaddingTop(2).AlignCenter().Text("support@learnnova.com | www.learnnova.com").FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(2).AlignCenter().Text("Invoice generated automatically. This is a computer generated document and does not require a physical signature.").FontSize(8).FontColor(Colors.Grey.Lighten1).Italic();
                    });
                });
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> GenerateReceiptPdfAsync(int paymentId)
    {
        return await GenerateInvoicePdfAsync(paymentId);
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(int certificateId)
    {
        var cert = await _certRepo.GetQueryable().Include(c => c.Course).ThenInclude(c => c.Teacher).Include(c => c.Student).FirstOrDefaultAsync(c => c.Id == certificateId);
        if (cert == null) throw new Exception("Certificate not found");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                
                page.Content().Border(10).BorderColor("#4338CA").Padding(2, Unit.Centimetre).Column(col =>
                {
                    col.Item().AlignCenter().Text("CERTIFICATE OF COMPLETION").FontSize(32).SemiBold().FontColor("#4338CA");
                    col.Item().PaddingTop(20).AlignCenter().Text("This certifies that").FontSize(16).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(10).AlignCenter().Text(cert.Student?.FullName ?? "Student").FontSize(40).Bold();
                    col.Item().PaddingTop(10).AlignCenter().Text("has successfully completed the course").FontSize(16).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(10).AlignCenter().Text(cert.Course?.Title ?? "Course Name").FontSize(28).SemiBold().FontColor("#4338CA");
                    
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(cert.Course?.Teacher?.FullName ?? "Teacher").FontSize(16).Bold();
                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().Text("Instructor").FontSize(12);
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text(cert.IssueDate.ToString("yyyy-MM-dd")).FontSize(16).Bold();
                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().Text("Date").FontSize(12);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text(cert.CertificateNumber).FontSize(16).Bold();
                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().Text("Certificate ID").FontSize(12);
                        });
                    });
                });
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }
}

