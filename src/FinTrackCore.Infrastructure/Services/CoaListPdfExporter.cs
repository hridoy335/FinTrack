using FinTrackCore.Application.Features.Coas.Models;
using FinTrackCore.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinTrackCore.Infrastructure.Services;

public sealed class CoaListPdfExporter : ICoaListPdfExporter
{
    static CoaListPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(CoaListResponse list, string userDisplayName)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(text => text.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("FinTrack")
                        .FontSize(18)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text("Chart of Accounts")
                        .FontSize(14)
                        .SemiBold();

                    column.Item().Text(userDisplayName)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);

                    column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(column =>
                {
                    foreach (var section in list.Sections)
                    {
                        column.Item().PaddingTop(10).Text(section.AccountTypeName)
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken1);

                        column.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                    .Text("Code").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                    .Text("Account Head Name").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                    .AlignRight().Text("Status").SemiBold();
                            });

                            foreach (var item in section.Items)
                            {
                                var status = item.IsActive ? "Active" : "Inactive";
                                if (item.IsSystemDefault)
                                {
                                    status = "Default";
                                }

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(item.Code);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(item.AccountHeadName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .AlignRight().Text(status);
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
