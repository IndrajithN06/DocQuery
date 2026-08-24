using DocQuery.Models;
using UglyToad.PdfPig;

namespace DocQuery.Services;

public class PdfService
{
    public List<PdfPageContent> ExtractPages(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);

        return document
            .GetPages()
            .Select(page=>new PdfPageContent
            {
            PageNumber = page.Number,
            Text= page.Text
            })
            .ToList();
    }
}