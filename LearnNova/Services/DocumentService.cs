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

    public DocumentService(IPaymentRepository paymentRepo, IGenericRepository<LearnNova.Models.Entities.Certificate> certRepo)
    {
        _paymentRepo = paymentRepo;
        _certRepo = certRepo;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int paymentId)
    {
        var payment = await _paymentRepo.GetQueryable()
            .Include(p => p.Student)
            .Include(p => p.Course).ThenInclude(c => c.Teacher)
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
            
        if (payment == null) throw new Exception("Payment not found");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

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
                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().Text("INVOICE").FontSize(20).Bold().FontColor(Colors.Grey.Darken3).AlignRight();
                                c.Item().Text($"# {payment.Invoice?.InvoiceNumber ?? "N/A"}").FontSize(12).FontColor(Colors.Grey.Darken2).AlignRight();
                                c.Item().Text($"Date: {payment.CreatedAt:MMM dd, yyyy}").FontSize(10).FontColor(Colors.Grey.Medium).AlignRight();
                            });
                        });
                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Billed To:").SemiBold().FontColor(Colors.Grey.Darken2);
                                c.Item().Text(payment.Student?.FullName ?? "Student").FontSize(14).Bold();
                                c.Item().Text(payment.Student?.Email ?? "student@learnnova.com").FontColor(Colors.Grey.Medium);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Provided By:").SemiBold().FontColor(Colors.Grey.Darken2);
                                c.Item().Text(payment.Course?.Teacher?.FullName ?? "Teacher").FontSize(14).Bold();
                                c.Item().Text("LearnNova Instructor").FontColor(Colors.Grey.Medium);
                            });
                        });

                        col.Item().PaddingVertical(20);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#4338CA").Padding(5).Text("#").FontColor(Colors.White).SemiBold();
                                header.Cell().Background("#4338CA").Padding(5).Text("Course").FontColor(Colors.White).SemiBold();
                                header.Cell().Background("#4338CA").Padding(5).Text("Category").FontColor(Colors.White).SemiBold();
                                header.Cell().Background("#4338CA").Padding(5).Text("Status").FontColor(Colors.White).SemiBold();
                                header.Cell().Background("#4338CA").Padding(5).AlignRight().Text("Amount").FontColor(Colors.White).SemiBold();
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("1");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(payment.Course?.Title ?? "N/A").SemiBold();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(payment.Course?.Subject ?? "General");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(payment.Status.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{payment.StudentPaid:N2} {payment.Currency}");
                        });

                        col.Item().PaddingVertical(20);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(250).Column(c =>
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Subtotal:").FontColor(Colors.Grey.Darken2);
                                    r.RelativeItem().AlignRight().Text($"{payment.StudentPaid:N2} {payment.Currency}");
                                });
                                c.Item().PaddingVertical(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Discount:").FontColor(Colors.Grey.Darken2);
                                    r.RelativeItem().AlignRight().Text("0.00 EGP");
                                });
                                c.Item().PaddingVertical(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Platform Fee:").FontColor(Colors.Grey.Darken2);
                                    r.RelativeItem().AlignRight().Text($"{payment.PlatformFee:N2} {payment.Currency}");
                                });
                                c.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Grand Total:").FontSize(14).Bold().FontColor("#4338CA");
                                    r.RelativeItem().AlignRight().Text($"{payment.StudentPaid:N2} {payment.Currency}").FontSize(14).Bold().FontColor("#4338CA");
                                });
                            });
                        });
                        
                        col.Item().PaddingVertical(20);
                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(c => {
                            c.Item().Text("Payment Details").SemiBold().FontColor(Colors.Grey.Darken3);
                            c.Item().Text($"Method: {payment.PaymentMethod ?? "Wallet"}").FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Transaction ID: {payment.TransactionId ?? "N/A"}").FontColor(Colors.Grey.Darken2);
                        });
                    });
                });

                page.Footer().Element(footer => 
                {
                    footer.Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(10).AlignCenter().Text("Thank you for learning with LearnNova!").FontColor(Colors.Grey.Medium);
                        col.Item().AlignCenter().Text("support@learnnova.com | www.learnnova.com").FontSize(9).FontColor(Colors.Grey.Lighten1);
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

