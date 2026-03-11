using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateBillPdf(TabBillData billData);
}
