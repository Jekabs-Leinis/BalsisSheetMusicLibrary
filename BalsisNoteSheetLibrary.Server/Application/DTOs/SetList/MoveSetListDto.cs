using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;

public class MoveSetListDto
{
    [Required] public required uint Id { get; set; }
    [Required] public required uint NewOrder { get; set; }
}