# Kira

Kira is a project and issue tracker with an Angular frontend and an ASP.NET Core API backed by SQL Server.

## Run locally

Prerequisites: .NET 10 SDK, Node.js with npm, and Docker Compose.

1. Start the database from the repository root:

   ```powershell
   docker compose up -d sqlserver
   ```

2. Apply the API migrations from the repository root:

   ```powershell
   dotnet ef database update --project Kira.Api
   ```

   If the `dotnet ef` command is unavailable, install the EF Core CLI tool:

   ```powershell
   dotnet tool install --global dotnet-ef --version 10.0.12
   ```

3. Start the API in a terminal:

   ```powershell
   dotnet run --project Kira.Api
   ```

   The development profile listens on `http://localhost:5263`.

4. In another terminal, start the Angular app:

   ```powershell
   cd Kira.Web
   npm install
   npm start
   ```

   Open `http://localhost:4200`. The Angular development proxy forwards `/api` requests to the API.

## Features

- Register and sign in; authenticated routes attach the JWT to API requests.
- Create and manage projects, update project details, and delete projects.
- View project members, invite existing accounts, change their role by re-inviting them with the desired role, and remove members while preserving at least one manager.
- Create, filter, edit, and delete project issues; assign members and track due dates, status, and priority.
- Add comments and review issue history.

## Build and test

Run these commands from their respective project directories:

```powershell
# Kira.Api
dotnet build

# Kira.Web
npm run build
npm test -- --watch=false
```
