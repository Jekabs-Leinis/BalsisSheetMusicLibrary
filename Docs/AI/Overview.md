# Client Project Overview

## Technology Stack

- **Framework:** Vue 3 (Single Page Application)
- **Build Tool:** Vite
- **State Management:** Pinia
- **Routing:** Vue Router
- **HTTP Client:** Axios
- **Real-time Communication:** @microsoft/signalr
- **UI & Styling:** Bootstrap 5, Bootstrap Icons, SCSS
- **Notifications:** vue-toastification
- **Drag-and-Drop:** vue-draggable-plus
- **Utilities:** lodash.debounce

## Project Structure

- **App.vue / main.js:** Entry point and root component. The app is initialized with Pinia for state management, Vue Router for navigation, and global plugins such as toast notifications and custom directives.
- **api/**: Contains logic for API interactions.
- **assets/**: Static assets, including JS and SCSS for Bootstrap customization.
- **components/**: Reusable Vue components.
- **directives/**: Custom Vue directives (e.g., vLoading).
- **models/**: Data models or TypeScript interfaces.
- **router/**: Routing configuration (routes.js).
- **services/**: Service classes/utilities for business logic or API abstraction.
- **static/**: Additional static files.
- **stores/**: Pinia stores for application state (e.g., userStore, notesheetStore, setlistStore).
- **views/**: Page-level Vue components for different routes.

# Server Project Overview

## Technology Stack

- **Framework:** ASP.NET Core 8.0 (Web API)
- **ORM:** Entity Framework Core (with SQLite)
- **Authentication:** ASP.NET Core Identity (cookie-based)
- **API Documentation:** Swashbuckle (Swagger)
- **Real-time Communication:** SignalR
- **Configuration:** appsettings.json, environment-based settings

## Project Structure

- **Program.cs:** Application startup, service configuration, middleware, and routing.
- **Controllers/**: API endpoints for authentication, CSRF, downloads, note sheets, set lists, and real-time status updates.
- **DTOs/**: Data Transfer Objects for structuring API requests and responses.
- **Helpers/**: Utility classes for LINQ, roles, SQLite, string extensions, and user/database seeding.
- **Migrations/**: Entity Framework migrations for database schema evolution.
- **Models/**: Core data models and DbContext (e.g., NoteSheet, SetList, Role, AppDbContext).
- **Static/**: Static file serving.
- **appsettings.json**: Application and environment configuration.

# Documentation Project Overview

## Purpose

The Docs project serves as the central location for documentation, configuration, and project overviews for the BalsisNoteSheetLibrary solution.

## Project Structure

- **Overview.md:** The main documentation file, containing overviews and summaries of the client, server, and documentation projects.