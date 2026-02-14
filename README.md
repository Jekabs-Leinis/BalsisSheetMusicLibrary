# BalsisSheetMusicLibrary

Balsis Sheet Music Library is a web SPA for choristers from choir **Balsis** to store sheet music and organize them into setlists.

## Project reasoning and goals

This project represents a clean-room reimagination of a legacy Python 2 WSGI application. While the previous iteration was functional, its deprecated dependencies ultimately made setup difficult and rendered the project unmaintainable. Consequently, the primary objective of this new architecture is to ensure long-term viability and ease of use.

Because the system is intended to be deployed by choir members with varying levels of technical proficiency, ease of setup is a critical requirement. The technology stack was specifically selected with this constraint in mind, prioritizing tools that are widely used, stable, and well-documented to facilitate straightforward maintenance.

Designed strictly as a self-hosted solution for internal use, the project adopts a pragmatic security posture. Since the threat model is limited, certain deployment conveniences are prioritized over strict security best practices. For example, the system utilizes dotenv files for configuration rather than complex vault solutions and does not enforce aggressive password policies.

Development also served as an opportunity to explore modern technologies and architectural patterns, specifically ASP.NET Core, the Vue 3 Composition API, and Domain-Driven Design (DDD). Note that while DDD concepts were an influence, this project is not a strict implementation of DDD and does not follow all of its principles as that would be overkill for a project of this size and scope.

## Tech stack

- **Backend**
    - ASP.NET Core (.NET `net9.0`)
    - Entity Framework Core + **SQLite**
    - ASP.NET Identity (authentication/authorization)
    - Serilog (logging)
- **Frontend**
    - Vue 3 + Vite
    - Pinia, Vue Router
    - Bootstrap
    - Axios

## Prerequisites

- `dotnet` SDK **9.x** (project targets `net9.0`)
- `node` **>= 18** minimum (recommended **>= 20**)
- `npm` (ships with Node)

## Local setup

1. Restore .NET dependencies:

```powershell
   dotnet restore .\BalsisSheetMusicLibrary.sln
```

2. Install client dependencies:

```powershell
   npm install --prefix .\BalsisSheetMusicLibrary.Client
```

3. Configure environment variables:

    - Copy `BalsisSheetMusicLibrary.Server/.env.example` to `BalsisSheetMusicLibrary.Server/.env`
    - Fill in passwords and adjust seeding flags as needed.
    - Configure `LIB_SHEETS_FOLDER_PATH` to point to the folder where sheet music PDFs will be stored. This should be either a relative path from the server binary (e.g., `./Sheets`) or an absolute path (e.g., `C:/BalsisSheets`).

   Example `.env`:

 ```dotenv
LIB_USER_NAME=your_user_username
LIB_USER_PASS=your_user_password

LIB_ADMIN_NAME=your_admin_username
LIB_ADMIN_PASS=your_admin_password

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
   > `BalsisSheetMusicLibrary.Server/.env` is intentionally gitignored. Do not commit secrets.

4. Run the app:

   ```powershell
   dotnet run --project .\BalsisSheetMusicLibrary.Server\BalsisSheetMusicLibrary.Server.csproj
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
dotnet publish .\BalsisSheetMusicLibrary.Server\BalsisSheetMusicLibrary.Server.csproj -c Release -o .\publish
```
or, with runtime and self-contained options for Linux:

```powershell
dotnet publish .\BalsisSheetMusicLibrary.Server\BalsisSheetMusicLibrary.Server.csproj --configuration Release --runtime linux-x64 --self-contained true -o ./publish
```

Copy the contents of the `publish` folder to your server and run the executable (`BalsisSheetMusicLibrary.Server.exe` on Windows, `BalsisSheetMusicLibrary.Server` on Linux).

For deployment instructions, see [the wiki](https://github.com/Jekabs-Leinis/BalsisNoteSheetLibrary/wiki/Deployment-Guide).

## Testing

This repository uses **xUnit** for tests, with **Moq** for mocking.

Run all tests:

```powershell
dotnet test .\BalsisSheetMusicLibrary.sln
```

Notes:

- Unit and integration tests live in `BalsisSheetMusicLibrary.Tests`.
- Project guidance:
    - Unit testing guidelines: `Docs/ForLLMs/Guidelines/UnitTestingGuidelines.md`
    - Integration testing guidelines: `Docs/ForLLMs/Guidelines/IntegrationTestingGuidelines.md`

## Deployment

Developer README intentionally stays focused on local development.

- For deployment instructions, see [the wiki](https://github.com/Jekabs-Leinis/BalsisNoteSheetLibrary/wiki/Deployment-Guide).

## License

Licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See `LICENSE.txt`.
