using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<NoteSheet> NoteSheets { get; set; } = null!;
        public DbSet<SetList> SetLists { get; set; } = null!;
        public DbSet<SetListItem> SetListItems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
}