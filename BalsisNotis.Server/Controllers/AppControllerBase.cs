using BalsisNotis.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNotis.Server.Controllers
{
    abstract public class AppControllerBase(AppDbContext context) : ControllerBase
    {
        protected readonly AppDbContext _context = context;
    }
}
