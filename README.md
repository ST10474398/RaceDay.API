# RaceDay — Part 3 (Web Application Automation)

**Student Name:** [Your Full Name]  
**Student Number:** [Your Student Number]  
**Module:** [Module Name]  

## Project Overview
This repository contains the final part of the RaceDay project. It consists of two main components:
1.  **RaceDay.API:** The RESTful backend API (built in Part 2) handling all database operations, authentication, and business logic.
2.  **RaceDay.MVC:** The frontend web application that consumes the API. It provides distinct interfaces for Organisers and Participants.

## Technologies Used
*   ASP.NET Core 8.0 (API and MVC)
*   Entity Framework Core (Code-First)
*   SQL Server (LocalDB)
*   ASP.NET Core Identity (Authentication & Roles)
*   Bootstrap 5 (UI Design)
*   Docker (Containerization)
*   GitHub Actions (CI/CD)

---

## Setup and Run Instructions

### Prerequisites
*   Visual Studio 2022
*   .NET 8.0 SDK
*   SQL Server LocalDB
*   Docker Desktop (for containerization)

### 1. Running the Application Locally
1.  Open `RaceDay.API.sln` in Visual Studio.
2.  Set **Multiple Startup Projects**: Set `RaceDay.API` and `RaceDay.MVC` to **Start**.
3.  Open the **Package Manager Console** (set Default project to `RaceDay.API`) and run:
    ```powershell
    Update-Database