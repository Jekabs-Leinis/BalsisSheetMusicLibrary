using System.ComponentModel.DataAnnotations;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

public class MoveSetListItemDto
{
    [Required] public uint SetListId { get; set; }
    [Required] public uint NoteSheetId { get; set; }
    [Required] public uint NewOrder { get; set; }
}