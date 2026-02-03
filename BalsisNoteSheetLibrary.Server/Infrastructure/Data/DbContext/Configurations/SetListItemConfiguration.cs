using BalsisNoteSheetLibrary.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Configurations;

public class SetListItemConfiguration : IEntityTypeConfiguration<SetListItem>
{
    public void Configure(EntityTypeBuilder<SetListItem> builder)
    {
        builder.HasKey(sli => new { sli.SetListId, sli.NoteSheetId });
    }
}