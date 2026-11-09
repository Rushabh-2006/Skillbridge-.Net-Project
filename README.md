<p align="center">
  <img src="https://img.shields.io/badge/.NET%20Core-10.0%20%7C%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET Version" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC%20%26%20Web%20API-blue?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/Database-MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/Data%20Access-Pure%20ADO.NET-00599C?style=for-the-badge" alt="Pure ADO.NET" />
  <img src="https://img.shields.io/badge/Frontend-Bootstrap%205%20%7C%20Chart.js-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Frontend" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License" />
</p>

<h1 align="center">🚀 SkillBridge</h1>
<h3 align="center">Career Readiness, Skill Gap Analysis & Internship Platform</h3>

<p align="center">
  A full-stack .NET web application bridging the gap between academic learning and industry demands through real-time skill gap analysis, level-based roadmaps, project proof verification, and smart recruiter matching.
</p>

---

## 🛠️ Tech Stack

| Layer | Technologies & Tools |
|---|---|
| **Frontend** | ASP.NET Core MVC (Razor Views `.cshtml`), Bootstrap 5, Custom Modern CSS, Chart.js |
| **Backend** | ASP.NET Core Web API & MVC Controllers (C# / .NET 10 & 8) |
| **Data Access** | **Pure ADO.NET** (`MySqlConnector`) — Zero ORM / No Entity Framework |
| **Database** | MySQL / MariaDB (`skillbridge_db`) |
| **Security** | Role-Based Access Control (RBAC), Cookie Authentication, Salted SHA-256 Hashing |
| **Architecture** | N-Tier Clean Architecture (Decoupled Web UI, REST API, DAL, and Shared Domain) |

---

## 📂 Folder Structure

```
.
├── 📁 Database/                          # SQL Scripts
│   ├── 01_SkillBridge_Schema.sql         # Database schema (15 relational tables)
│   └── 02_SkillBridge_SeedData.sql       # Seed data for all 4 roles, skills & companies
│
├── 📁 SkillBridge.Web/                   # Frontend MVC Application (Port: 5000)
│   ├── Controllers/                      # Student, Recruiter, Faculty, Admin, Account
│   ├── Views/                            # Role-based dashboards, razor views & partials
│   ├── wwwroot/                          # CSS styling, JS scripts, images, and vendor libraries
│   └── appsettings.json                  # Web configuration & MySQL connection string
│
├── 📁 SkillBridge.API/                   # Backend REST API Service (Port: 5050)
│   ├── Controllers/                      # Auth, Skill, Roadmap, Project, Internship endpoints
│   ├── Program.cs                        # DI setup, CORS policies & endpoint mapping
│   └── appsettings.json                  # API configuration & database connection string
│
├── 📁 SkillBridge.DataAccess/            # Data Access Layer (Pure ADO.NET)
│   ├── Entities/                         # Strongly-typed database entities
│   ├── Repositories/                     # SQL queries and parameterized DB operations
│   └── DBHelper.cs                       # Resilient MySQL connection provider
│
├── 📁 SkillBridge.Common/                # Shared Class Library
│   ├── DTOs/                             # Request/Response data models
│   ├── Security/                         # PasswordHasher (Salted SHA-256)
│   └── Utilities/                        # Constants, Enums (RoleConstants, etc.)
│
├── .gitignore                            # Standard .NET / Visual Studio git ignore rules
└── SkillBridge.sln                       # Unified Visual Studio Solution file
```

---

## 📌 Core Features & Workflows

- **Role-Based Portals**: Dedicated dashboards for **Student**, **Recruiter**, **Faculty**, and **Admin**.
- **Skill Gap Analysis**: Compares student competencies with target career requirements; shows mastered skills vs. missing gaps.
- **5-Tier Career Roadmaps**: Guided level progression from fundamentals to advanced cloud, major projects, and placement.
- **Project Recommendations & Proofs**: Algorithm-matched project suggestions with GitHub repo / live demo proof submission and review.
- **Candidate-Internship Matching**: Algorithmic matching score calculating candidate fit for recruiters.
- **ATS & Application Pipeline**: Recruiter tracking from *Applied* to *Reviewing*, *Shortlisted*, *Selected*, or *Rejected*.
- **Faculty Readiness Oversight**: Institutional placement tracking, submission approvals, and Chart.js analytics.
- **Public Portfolio**: Shareable student profile with verified badges (GitHub, LinkedIn, Email, Phone).

---

## 👥 Demo Logins

| Role | Email | Password | Access Highlights |
|---|---|---|---|
| **Student** | `student@skillbridge.io` | `Student@2026!` | Gap Analysis, Roadmaps, Project Submissions, Portfolio |
| **Recruiter** | `recruiter@nexuscloud.io` | `Recruiter@2026!` | Candidate Matching, Internship Postings, ATS Pipeline |
| **Faculty** | `faculty@stanford.edu` | `Faculty@2026!` | Placement Readiness, Project Approvals, Skill Analytics |
| **Admin** | `admin@skillbridge.io` | `Admin@2026!` | Master Skills, Career Taxonomy, Platform Reports |

---

## 🚀 Quick Setup & Run

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) or XAMPP (MariaDB)
- Visual Studio 2022 or VS Code

### 2. Setup Database
Import the SQL scripts into your MySQL instance in sequential order:
```bash
mysql -u root -p < Database/01_SkillBridge_Schema.sql
mysql -u root -p < Database/02_SkillBridge_SeedData.sql
```

Ensure connection strings in `SkillBridge.Web/appsettings.json` and `SkillBridge.API/appsettings.json` match your MySQL setup:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=skillbridge_db;Uid=root;Pwd=YOUR_PASSWORD;AllowUserVariables=True;"
}
```

### 3. Run the Projects
Open two terminal tabs and run:

```bash
# Terminal 1: Start REST API (Port 5050)
dotnet run --project SkillBridge.API

# Terminal 2: Start Web Application (Port 5000)
dotnet run --project SkillBridge.Web
```

Open your browser at **`http://localhost:5000`** to access the platform.

---

## 📡 API Endpoints

- `POST /api/auth/login` & `POST /api/auth/register` — Authentication & registration
- `GET /api/skill/master` — Standard skills taxonomy
- `GET /api/skill/gap/{studentId}/{careerGoalId}` — Real-time skill gap analysis
- `GET /api/roadmap/levels/{careerGoalId}` — Level-based roadmap steps
- `GET /api/project/recommendations/{studentId}` — Matched project recommendations
- `GET /api/internship/list` & `POST /api/application/apply` — Internship search & 1-click apply

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
