# Skillbridge-.Net-Project

🚀 SkillBridge

 An intelligent career readiness, skill gap analysis, and internship placement platform built with ASP.NET Core and MySQL.


 🛠️ Tech Stack

| Layer | Technologies |
|---|---|
| **Frontend** | ASP.NET Core MVC (Razor Views `.cshtml`), Bootstrap 5, Chart.js, Custom CSS |
| **Backend** | ASP.NET Core Web API & MVC Controllers (C# / .NET) |
| **Data Access** | **Pure ADO.NET** (`MySqlConnector`) — *Zero ORM / No Entity Framework* |
| **Database** | MySQL / MariaDB (`skillbridge_db`) |
| **Architecture** | N-Tier Clean Architecture (Decoupled Web UI, REST API, DAL, Common) |
| **Security** | Role-Based Access Control (RBAC), Cookie Auth, Salted SHA-256 Hashing |

---

## 📂 Folder Structure

```text
.
├── Database/               # MySQL schema (15 tables) and seed data
│   ├── 01_SkillBridge_Schema.sql
│   └── 02_SkillBridge_SeedData.sql
├── SkillBridge.API/        # ASP.NET Core REST API backend (Port: 5050)
├── SkillBridge.Web/        # ASP.NET Core MVC frontend application (Port: 5000)
├── SkillBridge.DataAccess/ # Pure ADO.NET repositories & DBHelper
├── SkillBridge.Common/     # Shared DTOs, models, and utility classes
├── .gitignore              # Standard .NET build ignore rules
└── SkillBridge.sln         # Visual Studio Solution
```

---

## ✨ Key Features

- Role-Based Portals**: Custom dashboards for **Student**, **Recruiter**, **Faculty**, and **Admin**.
- Skill Gap Analysis**: Compares student competencies with career targets to highlight missing skills.
- 5-Tier Career Roadmaps**: Step-by-step progression from fundamentals to placement readiness.
- Project Recommendations**: Skill-matched suggestions with GitHub/demo link verification.
- Smart Candidate Matching**: Algorithmic scoring that ranks candidates by skill coverage.
- Recruiter ATS Pipeline**: Track applicants through Applied, Reviewing, Shortlisted, and Selected.
- Faculty Oversight**: Cohort readiness tracking, submission reviews, and Chart.js analytics.
- Public Portfolio**: Shareable student profiles with verified badges (GitHub, LinkedIn, Email).

---

👥 Demo Logins

| Role | Email | Password |
|---|---|---|
| 🎓 **Student** | `student@skillbridge.io` | `Student@2026!` |
| 🏢 **Recruiter** | `recruiter@nexuscloud.io` | `Recruiter@2026!` |
| 🏫 **Faculty** | `faculty@stanford.edu` | `Faculty@2026!` |
| 🛡️ **Admin** | `admin@skillbridge.io` | `Admin@2026!` |

---

 🚀 Quick Start Guide
 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) or XAMPP (MariaDB)
- Visual Studio 2022 or VS Code

2. Database Setup
Import the database scripts into MySQL:
```bash
mysql -u root -p < Database/01_SkillBridge_Schema.sql
mysql -u root -p < Database/02_SkillBridge_SeedData.sql
```

Verify connection strings in `SkillBridge.Web/appsettings.json` and `SkillBridge.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=skillbridge_db;Uid=root;Pwd=YOUR_PASSWORD;AllowUserVariables=True;"
}
```

 3. Run the Projects
Open two terminal windows:

```bash
# Terminal 1: Launch Backend REST API (Port 5050)
dotnet run --project SkillBridge.API

# Terminal 2: Launch MVC Web Application (Port 5000)
dotnet run --project SkillBridge.Web
```

Open your browser at **`http://localhost:5000`**.



📡 REST API Overview

| Endpoint | Method | Description |
|---|---|---|
| `/api/auth/login` | `POST` | User login & role verification |
| `/api/auth/register` | `POST` | New user registration |
| `/api/skill/master` | `GET` | Get all master industry skills |
| `/api/skill/gap/{studentId}/{careerGoalId}` | `GET` | Run real-time skill gap analysis |
| `/api/roadmap/levels/{careerGoalId}` | `GET` | Get 5-tier roadmap for career path |
| `/api/project/recommendations/{studentId}` | `GET` | Get skill-matched project suggestions |
| `/api/internship/list` | `GET` | List active internship openings |
| `/api/application/apply` | `POST` | Submit an internship application |

 📄 License

This project is licensed under the [MIT License](LICENSE).
