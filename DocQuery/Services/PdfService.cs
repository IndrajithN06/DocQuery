using UglyToad.PdfPig;

namespace DocQuery.Services;

public class PdfService
{
    public string ExtractText(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);

        var pages = document.GetPages();

        var text = string.Join(
            Environment.NewLine + Environment.NewLine,
            pages.Select(page => page.Text));

        return text;
    }
}