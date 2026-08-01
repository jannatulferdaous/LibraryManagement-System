using Application.Reports.Dtos;
using Application.Reports.Queries.GetMostBorrowedBooksReport;
using Application.Reports.Queries.GetOverdueLoansReport;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using Asp.Versioning;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ReportsAccess")]
public class ReportsController : ControllerBase
{
    private readonly ISender _mediator;

    public ReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("overdue-loans")]
    public async Task<ActionResult<IReadOnlyList<OverdueLoanReportDto>>> GetOverdueLoans(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOverdueLoansReportQuery(), cancellationToken));

    [HttpGet("overdue-loans/export")]
    public async Task<IActionResult> ExportOverdueLoans(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetOverdueLoansReportQuery(), cancellationToken);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Overdue Loans");

        sheet.Cell(1, 1).Value = "Member";
        sheet.Cell(1, 2).Value = "Email";
        sheet.Cell(1, 3).Value = "Book";
        sheet.Cell(1, 4).Value = "Due Date";
        sheet.Cell(1, 5).Value = "Days Overdue";
        sheet.Range(1, 1, 1, 5).Style.Font.Bold = true;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var r = i + 2;
            sheet.Cell(r, 1).Value = row.MemberName;
            sheet.Cell(r, 2).Value = row.MemberEmail;
            sheet.Cell(r, 3).Value = row.BookTitle;
            sheet.Cell(r, 4).Value = row.DueDate;
            sheet.Cell(r, 4).Style.DateFormat.Format = "yyyy-mm-dd";
            sheet.Cell(r, 5).Value = row.DaysOverdue;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"overdue-loans-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("most-borrowed-books")]
    public async Task<ActionResult<IReadOnlyList<MostBorrowedBookReportDto>>> GetMostBorrowedBooks(
        [FromQuery] int top = 10, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetMostBorrowedBooksReportQuery(top), cancellationToken));

    [HttpGet("overdue-loans/export-pdf")]
    public async Task<IActionResult> ExportOverdueLoansPdf(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetOverdueLoansReportQuery(), cancellationToken);
        var generatedAt = DateTime.UtcNow;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Overdue Loans Report").FontSize(18).Bold();
                    col.Item().Text($"Generated {generatedAt:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.5f); // Member
                        columns.RelativeColumn(3);    // Book
                        columns.RelativeColumn(1.5f);  // Due Date
                        columns.RelativeColumn(1.5f);  // Days Overdue
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Member");
                        header.Cell().Element(HeaderCell).Text("Book");
                        header.Cell().Element(HeaderCell).Text("Due Date");
                        header.Cell().Element(HeaderCell).Text("Days Overdue");

                        static IContainer HeaderCell(IContainer c) => c
                            .DefaultTextStyle(x => x.Bold())
                            .PaddingVertical(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Black);
                    });

                    foreach (var row in rows)
                    {
                        table.Cell().Element(BodyCell).Text($"{row.MemberName}\n{row.MemberEmail}").FontSize(9);
                        table.Cell().Element(BodyCell).Text(row.BookTitle);
                        table.Cell().Element(BodyCell).Text(row.DueDate.ToString("yyyy-MM-dd"));
                        table.Cell().Element(BodyCell).Text(row.DaysOverdue.ToString()).FontColor(Colors.Red.Medium);

                        static IContainer BodyCell(IContainer c) => c
                            .PaddingVertical(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2);
                    }

                    if (rows.Count == 0)
                    {
                        table.Cell().ColumnSpan(4).Element(c => c.PaddingVertical(15))
                            .AlignCenter().Text("No overdue loans.").FontColor(Colors.Grey.Medium);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        var fileName = $"overdue-loans-{generatedAt:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
