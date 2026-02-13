// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace BalsisSheetMusicLibrary.Server.Domain.Entities;

public class SetListItem
{
    public uint? SetListId { get; set; }
    public uint? NoteSheetId { get; set; }
    public uint? Order { get; set; }
    
    public SetList? SetList { get; set; }
    public NoteSheet? NoteSheet { get; set; }
}