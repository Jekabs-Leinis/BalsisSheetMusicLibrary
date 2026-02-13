using BalsisSheetMusicLibrary.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext.Configurations;

public class SetListItemConfiguration : IEntityTypeConfiguration<SetListItem>
{
    public void Configure(EntityTypeBuilder<SetListItem> builder)
    {
        builder.HasKey(sli => new { sli.SetListId, sli.NoteSheetId });
    }
}