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

app.RegisterCrudEndPoints();
app.RegisterExportApiEndPoints();

app.Run();
