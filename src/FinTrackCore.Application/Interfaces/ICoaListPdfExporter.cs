using FinTrackCore.Application.Features.Coas.Models;

namespace FinTrackCore.Application.Interfaces;

public interface ICoaListPdfExporter
{
    byte[] Generate(CoaListResponse list, string userDisplayName);
}
