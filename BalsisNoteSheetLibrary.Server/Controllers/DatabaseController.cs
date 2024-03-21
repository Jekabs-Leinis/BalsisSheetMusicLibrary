using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    public class DatabaseController(AppDbContext context) : AppControllerBase(context)
    {
        [HttpGet(Name = "GenerateTables")]
        public void GenerateTables()
        {
            Context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Users (
                    id INTEGER PRIMARY KEY,
                    email TEXT NOT NULL,
                    password_hash TEXT NOT NULL,
                    is_admin INTEGER NOT NULL DEFAULT 0
                )
            ");
        }
    }
}
