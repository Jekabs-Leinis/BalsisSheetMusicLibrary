# BalsisSheetMusicLibrary

Balsis Sheet Music Library is a web SPA for choristers from choir **Balsis** to store sheet music and organize them into setlists.

## Project reasoning and goals

This project represents a clean-room reimagination of a legacy Python 2 WSGI application. While the previous iteration was functional, its deprecated dependencies ultimately made setup difficult and rendered the project unmaintainable. Consequently, the primary objective of this new architecture is to ensure long-term viability and ease of use.

Because the system is intended to be deployed by choir members with varying levels of technical proficiency, ease of setup is a critical requirement. The technology stack was specifically selected with this constraint in mind, prioritizing tools that are widely used, stable, and well-documented to facilitate straightforward maintenance.

Designed strictly as a self-hosted solution for internal use, the project adopts a pragmatic security posture. Since the threat model is limited, certain deployment conveniences are prioritized over strict security best practices. For example, the system utilizes dotenv files for configuration rather than complex vault solutions and does not enforce aggressive password policies.

Development also served as an opportunity to explore modern technologies and architectural patterns, specifically ASP.NET Core, the Vue 3 Composition API, and Domain-Driven Design (DDD). Note that while DDD concepts were an influence, this project is not a strict implementation of DDD and does not follow all of its principles as that would be overkill for a project of this size and scope.

## Deployment & Usage

For deployment instructions, see [Installation.md](https://github.com/Jekabs-Leinis/BalsisNoteSheetLibrary/blob/master/Docs/Installation.md).
For updating instructions, see [Updating.md](https://github.com/Jekabs-Leinis/BalsisSheetMusicLibrary/blob/master/Docs/Updating.md)

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
    - Configure `LIB_SHEETS_FOLDER_PATH` to point to the folder where sheet music PDFs will be stored. This should be either a relative path from the server binary BalsisSheetMusicLibrary.Server folder (e.g., `files/sheets`) or an absolute path (e.g., `C:/BalsisSheets`).

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

- Unit and integration tests live in `BalsisSheetMusicLibrary.Tests`.
- Unit testing guidelines: `Docs/ForLLMs/Guidelines/UnitTestingGuidelines.md`
- Integration testing guidelines: `Docs/ForLLMs/Guidelines/IntegrationTestingGuidelines.md`

## License

Licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See `LICENSE.txt`.
