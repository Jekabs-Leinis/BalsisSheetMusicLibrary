# BalsisNoteSheetLibrary

Balsis Note Sheet Library is a web application for choirists from choir **Balsis** to store note sheets (sheet music) and organize them into setlists.

It includes built-in admin functionality for creating and managing note sheets and setlists.

## Tech stack

- **Backend**
    - ASP.NET Core (.NET `net9.0`)
    - Entity Framework Core + **SQLite**
    - ASP.NET Identity (authentication/authorization)
    - Serilog (logging)
    - Swagger / OpenAPI (Swashbuckle)
- **Frontend**
    - Vue 3 + Vite
    - Pinia, Vue Router
    - Bootstrap
    - Axios
    - SignalR client

## Prerequisites

- `dotnet` SDK **9.x** (project targets `net9.0`)
- `node` **>= 18** minimum (recommended **>= 20**)
- `npm` (ships with Node)

## Local setup

1. Restore .NET dependencies:

```powershell
   dotnet restore .\BalsisNoteSheetLibrary.sln
```

2. Install client dependencies:

```powershell
   npm install --prefix .\balsisnotesheetlibrary.clien
```

3. Configure environment variables:

    - Copy `BalsisNoteSheetLibrary.Server/.env.example` to `BalsisNoteSheetLibrary.Server/.env`
    - Fill in passwords and adjust seeding flags as needed.
    - Configure `LIB_SHEETS_FOLDER_PATH` to point to the folder where note sheet PDFs will be stored. This should be either a relative path from the server binary (e.g., `./Sheets`) or an absolute path (e.g., `C:/BalsisSheets`).

   Example `.env`:

 ```dotenv
LIB_USER_NAME=your_user_username
LIB_USER_PASS=

LIB_ADMIN_NAME=your_admin_username
LIB_ADMIN_PASS=

# If 1, will run the seeder on application start
# Must be enabled on first run to create initial users, then should be disabled
# Password can be changed afterwards via web interface after login
LIB_ENABLE_SEEDERS=1

# If 1, will allow seeding to overwrite existing user passwords with LIB_USER_PASS and LIB_ADMIN_PASS
# Used as a recovery option in case passwords are lost.
LIB_ALLOW_SEEDER_PASSWORD_RESET=0

# If 1, will allow manual password reset for admin via web interface
# Can be disabled for public testing deployments
LIB_ALLOW_MANUAL_PASSWORD_RESET=1

# Path to the folder where the sheets are stored. Path can be either absolute or relative.
# If relative, it must not begin with a '/' or '\', and will be relative to the binary location.
LIB_SHEETS_FOLDER_PATH=path/to/sheets/folder

# If 1, will disable soft delete functionality and permanently delete sheets instead of moving them to the trash folder.
LIB_SOFT_DELETE_DISABLED=0

# Path to the folder where the deleted sheets are moved to when soft delete is enabled. Path can be either absolute or relative to LIB_SHEETS_FOLDER_PATH.
# If not set a default "trash" folder will be used within the sheets folder.
LIB_TRASH_FOLDER_PATH=trash
```

   > [!IMPORTANT]
   > `BalsisNoteSheetLibrary.Server/.env` is gitignored. Do not commit secrets.

4. Run the app:

   ```powershell
   dotnet run --project .\BalsisNoteSheetLibrary.Server\BalsisNoteSheetLibrary.Server.csproj
   ```

   This uses the ASP.NET Core SPA proxy to start the Vite dev server automatically (`npm run dev`).

### Local URLs and ports

- Backend:
    - `https://localhost:7171`
    - `http://localhost:5124`
- Swagger UI: `https://localhost:7171/swagger`
- Vite dev server (SPA proxy target): `https://localhost:5173`

## Publishing

To publish the app for production, use the `dotnet publish` command:

```powershell
dotnet publish .\BalsisNoteSheetLibrary.Server\BalsisNoteSheetLibrary.Server.csproj -c Release -o .\publish
```
or, with runtime and self-contained options for Linux:

```powershell
dotnet publish .\BalsisNoteSheetLibrary.Server\BalsisNoteSheetLibrary.Server.csproj --configuration Release --runtime linux-x64 --self-contained true -o ./publish
```

Copy the contents of the `publish` folder to your server and run the executable (`BalsisNoteSheetLibrary.Server.exe` on Windows, `BalsisNoteSheetLibrary.Server` on Linux).

For deployment instructions, see [the wiki]().

## Testing

This repository uses **xUnit** for tests, with **Moq** for mocking.

Run all tests:

```powershell
dotnet test .\BalsisNoteSheetLibrary.sln
```

Collect coverage (via `coverlet.collector` / .NET test data collector):

```powershell
dotnet test .\BalsisNoteSheetLibrary.sln --collect:"XPlat Code Coverage"
```

Notes:

- Unit and integration tests live in `BalsisNoteSheetLibrary.Tests`.
- Project guidance:
    - Unit testing guidelines: `Docs/ForLLMs/Guidelines/UnitTestingGuidelines.md`
    - Integration testing guidelines: `Docs/ForLLMs/Guidelines/IntegrationTestingGuidelines.md`

## Dependencies and services

| Dependency | Purpose | Notes |
|-----------|---------|------|
| **SQLite (`app.db`)** | Primary data store | Connection string: `Data Source=app.db` (`appsettings.json`) |
| **Serilog** | Logging | Logs to console and `Logs/log-.txt` |
| **Swagger (Swashbuckle)** | API documentation | Enabled in development profile |

## Folder structure

```text
BalsisNotis/
  BalsisNoteSheetLibrary.Server/      # ASP.NET Core backend (API + SPA proxy)
    Api/
    Application/
    Domain/
    Infrastructure/
    Properties/
    Static/
      Sheets/                         # PDF files stored on disk
  BalsisNoteSheetLibrary.Tests/       # xUnit test project
    Integration/
    Unit/
  balsisnotesheetlibrary.client/      # Vue 3 + Vite frontend
    src/
      api/
      assets/
      components/
      config/
      directives/
      models/
      router/
      services/
      static/
      stores/
      views/
  Docs/                               # project docs (incl. testing guidelines)
    ForLLMs/
      Commands/
      Guidelines/
      Research/
  Installation.md                     # deployment notes (kept separate from developer README)
  LICENSE.txt                         
```

## Deployment

Developer README intentionally stays focused on local development.

- For deployment instructions, see: `Installation.md`.
- CI/CD:
    - No pipeline is currently configured (the `.github/workflows/` directory is empty).

## License

Licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See `LICENSE.txt`.
