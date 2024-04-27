using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace MinApiDemo;
public static class ExportApis
{
    public static void RegisterExportApiEndPoints(this WebApplication app)
    {
        var exportGroup = app.MapGroup("/export");
        exportGroup.MapGet("/json", ExportJson);
        exportGroup.MapGet("/xml", ExportXml);
    }

    private static async Task<JsonHttpResult<Bookmark[]>> ExportJson(AppDbContext db)
    {
        var bookmarks = await db.Bookmarks.ToArrayAsync();
        return TypedResults.Json(bookmarks);
    }

    private static async Task<ContentHttpResult> ExportXml(AppDbContext db)
    {
        var bookmarks = await db.Bookmarks.ToListAsync();
        var xd = XDocument.Parse("<bookmarks></bookmarks>");
        var root = xd.Root!;
        foreach (var bookmark in bookmarks)
        {
            root.Add(new XElement("bookmark",
                new XAttribute("id", bookmark.Id),
                new XAttribute("title", bookmark.Title),
                new XAttribute("url", bookmark.Url)
            ));
        }
        return TypedResults.Content(xd.ToString(), "application/xml");
    }
}
