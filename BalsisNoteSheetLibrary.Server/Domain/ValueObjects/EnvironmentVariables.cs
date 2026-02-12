namespace BalsisNoteSheetLibrary.Server.Domain.ValueObjects;

public static class EnvironmentVariables
{
    public const string AdminUsername = "LIB_ADMIN_NAME";
    public const string AdminPassword = "LIB_ADMIN_PASS";
    public const string UserUsername = "LIB_USER_NAME";
    public const string UserPassword = "LIB_USER_PASS";
    public const string EnableSeeders = "LIB_ENABLE_SEEDERS";
    public const string AllowSeederPasswordReset = "LIB_ALLOW_SEEDER_PASSWORD_RESET";
    public const string AllowManualPasswordReset = "LIB_ALLOW_MANUAL_PASSWORD_RESET";
    public const string SheetsFolderPath = "LIB_SHEETS_FOLDER_PATH";
    public const string TrashFolderPath = "LIB_TRASH_FOLDER_PATH";
    public const string SoftDeleteDisabled = "LIB_SOFT_DELETE_DISABLED";
}