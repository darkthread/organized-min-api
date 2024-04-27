using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MinApiDemo
{
    public class Bookmark
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^https?://.*")]
        public string Url { get; set; } =   string.Empty;
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Bookmark> Bookmarks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}