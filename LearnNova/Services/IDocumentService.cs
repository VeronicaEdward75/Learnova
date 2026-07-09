using System.Threading.Tasks;

namespace LearnNova.Services;

public interface IDocumentService
{
    Task<byte[]> GenerateInvoicePdfAsync(int paymentId);
    Task<byte[]> GenerateReceiptPdfAsync(int paymentId);
    Task<byte[]> GenerateCertificatePdfAsync(int certificateId);
}
