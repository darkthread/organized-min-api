using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MinApiDemo
{
    public static class CrudApis
    {
        public static void RegisterCrudEndPoints(this WebApplication app)
        {
            var bookmarkGroup = app.MapGroup("/bookmarks");   
            bookmarkGroup.MapGet("/list", GetBookmarks);
            bookmarkGroup.MapPost("/add", AddBookmark);
            bookmarkGroup.MapPost("/remove/{id}", RemoveBookmark);
        }

        private static async Task<Ok<Bookmark[]>> GetBookmarks(AppDbContext db)
        {
            var list = await db.Bookmarks.ToArrayAsync();
            return TypedResults.Ok(list);
        }

        private static async Task<Results<Ok<Bookmark>, BadRequest<List<ValidationResult>>>> AddBookmark(AppDbContext db, Bookmark bookmark)
        {
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(bookmark, new ValidationContext(bookmark), results, true);
            if (!isValid) return TypedResults.BadRequest(results);
            db.Bookmarks.Add(bookmark);
            await db.SaveChangesAsync();
            return TypedResults.Ok(bookmark);
        }

        private static async Task<Results<Ok<string>, NotFound>> RemoveBookmark(AppDbContext db, int id)
        {
            var bookmark = await db.Bookmarks.FindAsync(id);
            if (bookmark == null) return TypedResults.NotFound();
            db.Bookmarks.Remove(bookmark);
            await db.SaveChangesAsync();
            return TypedResults.Ok("OK");
        }
    }
}