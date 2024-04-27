using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using MinApiDemo;

var builder = WebApplication.CreateBuilder(args);

var sqliteFileName = "bookmark.sqlite";
if (File.Exists(sqliteFileName)) File.Delete(sqliteFileName);
var cs = $"Data Source={sqliteFileName}";

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(cs);
});

var app = builder.Build();

// create scope to create dbcontext
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Bookmarks.Any()) {
        db.Bookmarks.AddRange(new[] {
            new Bookmark { Title = "Google", Url = "https://www.google.com" },
            new Bookmark { Title = "Facebook", Url = "https://www.facebook.com" },
            new Bookmark { Title = "YouTube", Url = "https://www.youtube.com" }
        });
        db.SaveChanges();
    }
}

app.UseFileServer(new FileServerOptions {
    RequestPath = "",
    FileProvider = new Microsoft.Extensions.FileProviders
                    .ManifestEmbeddedFileProvider(
        typeof(Program).Assembly, "ui"
    ) 
});

app.MapGet("/bookmarks/list", async (AppDbContext db) => {
    return Results.Json(await db.Bookmarks.ToListAsync());
});
app.MapPost("/bookmarks/add", async (AppDbContext db, Bookmark bookmark) => {
    var results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(bookmark, new ValidationContext(bookmark), results, true);
    if (!isValid) return Results.BadRequest(results);
    db.Bookmarks.Add(bookmark);
    await db.SaveChangesAsync();
    return Results.Json(bookmark);
});
app.MapPost("/bookmarks/remove/{id}", async (AppDbContext db, int id) => {
    var bookmark = await db.Bookmarks.FindAsync(id);
    if (bookmark == null) return Results.NotFound();
    db.Bookmarks.Remove(bookmark);
    await db.SaveChangesAsync();
    return Results.Ok("OK");
});
app.MapGet("/export/json", async (AppDbContext db) => {
    var bookmarks = await db.Bookmarks.ToListAsync();
    return Results.Json(bookmarks);
});
app.MapGet("/export/xml", async (AppDbContext db) => {
    var bookmarks = await db.Bookmarks.ToListAsync();
    var xd = XDocument.Parse("<bookmarks></bookmarks>");
    var root = xd.Root!;
    foreach (var bookmark in bookmarks) {
        var xe = new XElement("bookmark");
        xe.Add(new XElement("id", bookmark.Id));
        xe.Add(new XElement("title", bookmark.Title));
        xe.Add(new XElement("url", bookmark.Url));
        root.Add(xe);
    }
    return Results.Content(xd.ToString(), "application/xml");
});

app.Run();
