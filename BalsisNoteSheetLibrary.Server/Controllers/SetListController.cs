using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
public class SetListController(AppDbContext context) : AppControllerBase(context)
{
    [HttpGet(Name = "GetAll")]
    public AppResponse<IEnumerable<SetList>> GetAll()
    {
        var setLists = Context.SetLists
            .Include(list => list.Items)
            .OrderBy(list => list.Order);

        return new AppResponse<IEnumerable<SetList>>(setLists, true);
    }

    [HttpGet("{id:int}")]
    public AppResponse<SetList?> Get(uint id)
    {
        var setList = Context.SetLists.Find(id);

        return new AppResponse<SetList?>(
            setList,
            setList is not null,
            setList is null ? "Set list not found" : string.Empty
        );
    }

    [HttpPost]
    public AppResponse<string> Add(SetList setList)
    {
        Context.SetLists.Add(setList);
        Context.SaveChanges();

        return new AppResponse<string>("Set list added", true);
    }

    [HttpPost]
    public AppResponse<string> Update(SetList setList)
    {
        var sheet = Context.SetLists.Find(setList.Id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Set list not found");
        }

        Context.SetLists.Update(setList);
        Context.SaveChanges();

        return new AppResponse<string>("Set list updated", true);
    }

    [HttpDelete("{id:int}")]
    public AppResponse<string> Delete(uint id)
    {
        var setList = Context.SetLists.Find(id);

        if (setList is null)
        {
            return new AppResponse<string>(null, false, "Set list not found");
        }

        Context.SetLists.Remove(setList);
        Context.SaveChanges();

        return new AppResponse<string>("Set list deleted", true);
    }
}