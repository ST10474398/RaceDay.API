# RaceDay API — Part 2

A RESTful Web API built with ASP.NET Core for managing running, walking, and cycling events. The API powers the RaceDay system by handling users, events, enrolments, and results.

**Student Name:** Hilton 
**Student Number:** ST10474398 
**Module:**  Prog6212
**Repository:**  (https://github.com/ST10474398/RaceDay.API)

---

## 1. Project Overview

The RaceDay API is built with **ASP.NET Core 8.0 Web API** and **SQL Server**, using **Entity Framework Core (Code-First)** for database access. It is designed to support role-based access control for three user types and can be tested using Swagger. The project also includes xUnit unit tests and a GitHub Actions CI/CD workflow.

### Tech Stack

- ASP.NET Core 8.0 Web API
- Entity Framework Core (Code-First)
- SQL Server LocalDB
- ASP.NET Core Identity and role-based authorization
- Swagger / OpenAPI for API documentation and testing
- xUnit and Moq for unit testing
- GitHub Actions for CI/CD

---

## 2. Features

Depending on the implemented endpoints and authorization rules, the API supports the following RaceDay functionality:

- **User management:** registration, login, and role-based access.
- **Event management:** create, view, update, and delete running, walking, and cycling events.
- **Enrolments:** allow participants to enrol in events and manage their enrolments.
- **Results:** store and retrieve event results.
- **API documentation:** explore and test endpoints through Swagger UI.
- **Automated testing:** run unit tests with xUnit and Moq.
- **Continuous integration:** use GitHub Actions to build and test the project automatically.

> Update this list if any feature is not implemented in your current version of the API.

---

## 3. Prerequisites

Install the following before running the project:

- Visual Studio 2022 or later
- .NET 8.0 SDK
- SQL Server LocalDB (usually available through Visual Studio)
- Git
- A compatible web browser for Swagger UI

---

## 4. Setup and Run Instructions

### Step 1: Clone the repository

Open a terminal and run:

```bash
git clone https://github.com/[Your Username]/RaceDay.API.git
cd RaceDay.API
```

Replace `[Your Username]` with your GitHub username.

### Step 2: Restore dependencies

From the folder containing the solution or project file, run:

```bash
dotnet restore
```

### Step 3: Configure the database connection

Open `appsettings.json` and check the `ConnectionStrings` section. For SQL Server LocalDB, a connection string may look like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Use the connection-string name and database settings expected by your project. Do not commit passwords, API keys, or other secrets to source control.

### Step 4: Apply Entity Framework Core migrations

If the project already contains migrations, run:

```bash
dotnet ef database update
```

If the `dotnet ef` command is unavailable, install the EF Core command-line tool:

```bash
dotnet tool install --global dotnet-ef
```

If migrations do not exist yet, create one from the project that contains the `DbContext`:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Only create a new migration if your project does not already have the required migrations. If the API and `DbContext` are in separate projects, you may need to specify the project and startup project using `--project` and `--startup-project`.

### Step 5: Run the API

From the directory containing the API `.csproj` file, run:

```bash
dotnet run
```

Alternatively, open the solution in Visual Studio and run the API project.

### Step 6: Open Swagger

When the application starts, check the terminal or Visual Studio output for the local URL. Open the Swagger URL shown there, usually:

```text
https://localhost:<port>/swagger
```

Replace `<port>` with the HTTPS port displayed by your application. If Swagger is configured only for development, make sure the application is running in the Development environment.

---

## 5. API Endpoints

The exact routes and HTTP methods depend on the controllers implemented in this repository. Use Swagger UI to view the available endpoints, required parameters, request bodies, and response codes.

Typical REST API operations may include:

| Resource | Common operations |
|---|---|
| Users / Authentication | Register, log in, view or update profile |
| Events | List events, view an event, create, update, or delete an event |
| Enrolments | Enrol in an event, view enrolments, cancel an enrolment |
| Results | Add, view, or update event results |

This table describes likely resource operations; it is not a guarantee that every route is implemented.

---

## 6. Authentication and Authorization

The project uses ASP.NET Core Identity and role-based authorization, as configured in the application. Protected endpoints may require a valid authentication token or another configured authentication mechanism.

- Sign in using the authentication flow implemented by the project.
- If Swagger supports bearer-token authentication, select **Authorize** and enter the token in the format expected by the API.
- Ensure each user has the correct role before testing role-restricted endpoints.

Do not assume that a role or endpoint is available unless it is configured in the current codebase.

---

## 7. Running Unit Tests

To run the automated tests, open a terminal at the solution directory and execute:

```bash
dotnet test
```

The test project should contain the xUnit tests and any Moq-based test doubles. Review the test output to identify failed tests.

---

## 8. Continuous Integration (GitHub Actions)

The repository may use GitHub Actions to build and test the API whenever changes are pushed or a pull request is opened.

To check the workflow:

1. Push your changes to GitHub.
2. Open the repository on GitHub.
3. Select the **Actions** tab.
4. Open the latest workflow run and review its build and test results.

Ensure the workflow file is present in `.github/workflows/` and matches the solution and test-project paths in your repository.

---

## 9. Troubleshooting

- **Database connection fails:** Confirm SQL Server LocalDB is installed and the connection string matches the database configuration.
- **Migrations fail:** Run commands from the correct directory and confirm the startup project and `DbContext` project are correctly specified.
- **Swagger does not open:** Check the URL and port printed when the API starts, and confirm Swagger is enabled for the current environment.
- **Packages are missing:** Run `dotnet restore`.
- **Tests fail:** Run `dotnet test` and inspect the error output.
- **Port is already in use:** Stop the other application using the port or configure a different development port.

---

## 10. Project Structure

A typical ASP.NET Core Web API solution may look like this. Your actual folder names may differ.

```text
RaceDay.API/
├── Controllers/
├── Data/                  # DbContext and database configuration
├── Models/                # Entity and request/response models
├── Migrations/            # Entity Framework Core migrations
├── Services/              # Business logic, if used
├── Properties/
├── appsettings.json
├── Program.cs
└── RaceDay.API.csproj

RaceDay.API.Tests/         # Example test project location
```

---

## 11. Security Notes

- Never commit production passwords, tokens, or other secrets.
- Use HTTPS for development and production where supported.
- Validate incoming data and enforce authorization on protected endpoints.
- Store sensitive configuration in user secrets or environment variables where appropriate.

---

## 12. License

Add the license used by this project here, if applicable.

---

## 13. Author

**Student Name:** Hilton 
**Student Number:** ST10474398
**Module:** Prog6212
