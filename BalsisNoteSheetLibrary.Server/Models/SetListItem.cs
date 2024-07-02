// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Models;

[PrimaryKey(nameof(SetListId), nameof(NoteSheetId))]
public class SetListItem
{
    public uint? SetListId { get; set; }
    public uint? NoteSheetId { get; set; }
    public uint? Order { get; set; }

    [JsonIgnore] public SetList? SetList { get; set; }
    [JsonIgnore] public NoteSheet? NoteSheet { get; set; }
}