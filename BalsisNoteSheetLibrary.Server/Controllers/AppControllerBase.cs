using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers
{
    public abstract class AppControllerBase(AppDbContext context) : ControllerBase
    {
        protected readonly AppDbContext Context = context;
    }
}
