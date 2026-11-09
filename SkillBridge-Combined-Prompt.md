# SkillBridge — Combined Project Prompt (Backend + Frontend/UI + Folder Structure)

This is the single source-of-truth prompt for SkillBridge. It merges the backend/functional
requirements (original + fixes) with the UI/UX design direction, and adds the concrete
Visual Studio solution structure so the frontend and backend are wired together as one
working project — not two disconnected halves.

---

## 1. Tech Stack

- **ASP.NET MVC** — server-rendered Razor views (`.cshtml`), this is the front-end
- **ASP.NET Web API** — separate project, consumed by the MVC app for data operations
- **ADO.NET** — all data access (no Entity Framework, no ORM)
- **MySQL** — the only database (via `MySql.Data` / `MySqlConnector` ADO.NET provider)
- **Bootstrap 5 + plain CSS + Chart.js** — for UI (no React/Angular/Vue, no SPA framework)
- Must open and run directly in **Visual Studio** as one solution, MVC and API projects
  both included and referencing a shared Data Access layer

**Frontend–backend connection rule:** The MVC app is not just static views — its
controllers call the Web API (via `HttpClient`) or the shared Data Access layer for every
piece of dynamic data (skills, gap analysis, roadmap, applications, dashboards, etc.).
No page should show hardcoded/mock data; every dashboard, table, badge, and chart in
Section 4 (UI) must be backed by a real MySQL query defined in Section 2/3 (backend).

---

## 2. User Roles

1. **Student** — Main user
2. **Company / Recruiter** — Posts internships/jobs, evaluates candidates
3. **Faculty / Placement Officer** — Monitors student readiness and placement activity
4. **Admin** — Controls the entire platform

---

## 3. Core Workflow

```
Create Profile → Add Education → Add Skills → Add Projects → Add Certifications
→ Select Career Goal → Skill Gap Analysis (Missing / Existing Skills)
→ Learning Roadmap → Project Suggestions → Build Project → Upload Project Proof
→ Skill Verification → Internship Matching → Apply for Internship
→ Recruiter Evaluation (Selected / Rejected) → Internship → Final Evaluation
→ Student Portfolio → Placement Readiness
```

---

## 4. Functional Requirements

### 4.1 Baseline Features (already scoped)

- Student registration/login
- Internship programs
- Project-based learning
- Progress tracking
- Project submission
- Reviews
- Certificates
- Certificate verification
- Support tickets
- Mentorship/collaboration
- Internship applications
- Company/admin management

### 4.2 Skill Gap Analysis

Student selects a target career (e.g. **ASP.NET Developer**). System stores required
skills per career in MySQL and compares against the student's own skills table.

| Skill | Required | Student Has It? |
|---|---|---|
| C# | Required | ✓ |
| ASP.NET Core | Required | ✗ |
| SQL Server | Required | ✓ |
| Web API | Required | ✗ |
| Git | Required | ✓ |
| Entity Framework | Required | ✗ |
| HTML/CSS | Required | ✓ |

Output:
- **YOUR SKILL GAP** (skills held, with a confidence/strength %): C#, SQL Server, Git, HTML/CSS
- **NEEDS IMPROVEMENT** (missing skills, with a % score, e.g. based on partial evidence): ASP.NET Core, Web API, Entity Framework

### 4.3 Career Roadmap

Rule-based (no AI needed), driven by a `CareerRoadmapLevels` table mapping skills to levels:

- **Level 1**: C# Fundamentals, OOP, Collections
- **Level 2**: SQL, HTML/CSS, JavaScript
- **Level 3**: ASP.NET Core, MVC, Web API
- **Level 4**: Entity Framework, Authentication, Deployment
- **Level 5**: Build Major Project, Internship, Placement

Each skill in each level shows ✓ (done) or → (pending) based on the student's skill table.

### 4.4 Project Recommendation Engine

Compares Student Skills + Project Required Skills → Skill Match %.

- **Beginner**: Student Management System
- **Intermediate**: Online Book Marketplace
- **Advanced**: Hospital Queue Management
- **Expert**: Multi-tenant SaaS Application

Example — Hospital Queue System: Skill Match 72%, Missing: Web API, SignalR, Advanced SQL.

### 4.5 Internship Matching

Only show internships above a relevance threshold, based on required-skill overlap.

Example — **.NET Developer Intern** (Required: C#, ASP.NET Core, SQL, Git):
Student A (has all four) → 100% match. Student B → 55% match.

### 4.6 Recruiter Evaluation — Auto Selected / Rejected (new requirement)

- Every internship posting has a **Minimum Match %** set by the recruiter
- On application, system computes the applicant's Skill Match % against required skills
- **Auto-decision rule:**
  - Match % ≥ Minimum Match % → **Selected**
  - Match % < Minimum Match % → **Rejected**
- Recruiter can manually override the auto-decision in either direction — auto-decision
  is a default, not a lock
- `Applications` table stores: Match %, Auto-Decision, Final Decision, Decision Date

### 4.7 Student Readiness Score

Weighted composite score, computed and stored per student:

- Skills — e.g. 80%
- Projects — e.g. 70%
- Certifications — e.g. 60%
- Internships — e.g. 40%
- Problem Solving — e.g. 75%
- GitHub Activity — e.g. 65%
- **Overall Readiness** — weighted average, e.g. 68%

### 4.8 Student Portfolio (public page)

Public profile showing Skills, Projects, Certifications, Internships — the student's
digital proof-of-work. Publicly shareable by student ID/slug.

### 4.9 Faculty Dashboard

- Total Students, Placement Ready, Needs Improvement, Not Started (counts)
- **Top Missing Skills** — aggregated across all students, with student counts
  (e.g. ASP.NET Core — 128 students, Python — 96, SQL — 82, Git — 74, React — 68)
- This drives real institutional decisions ("run an ASP.NET Core workshop")

### 4.10 Recruiter Dashboard

- Recruiter posts a job with Required Skills + Minimum Match %
- System ranks candidates by match %, recruiter shortlists from ranked list
- Example: Student A 96%, Student B 91%, Student C 87%, Student D 76%

### 4.11 Skill Verification System

A skill is not "verified" just because the student claims it. Verification requires
evidence, tracked per skill per student:

**Skill: React** — Evidence: ✓ Project, ✓ Certificate, ✓ Assessment, ✓ GitHub Repository,
✓ Faculty Verification → **Skill Verification: VERIFIED**

### 4.12 Authentication — Login & Register (new requirement, built from scratch)

- One Register page with role selection at signup: Student / Recruiter / Faculty / Admin
- One Login page for all roles; redirect to the correct role dashboard after login
- Passwords hashed (never plain text) in the MySQL `Users` table, written/read via ADO.NET
- Session/cookie-based auth shared between the MVC app and the Web API, so API calls
  made by MVC controllers are authenticated using the same login session
- Fields: Name, Email, Password, Role, Phone Number
- Logout for all roles

### 4.13 Certification Upload + Verification (new requirement)

- Students upload certification files (PDF/image) tied to a specific skill/credential
- Record fields: Title, Issuing Organization, Date, File, Linked Skill, Verification Status
- Verification Status: **Pending → Verified / Rejected**, set by Faculty/Admin
- A Verified certificate counts as one evidence checkmark in the Skill Verification system (4.11)

### 4.14 Mentorship Arrangement (new requirement)

- Students request mentorship (general, or tied to a specific skill/career goal)
- Faculty/mentors view pending requests and accept/assign themselves
- Record fields: Student, Mentor, Topic/Skill, Status (Requested / Active / Completed), Notes
- A simple notes/message thread per mentorship record — no real-time chat needed

### 4.15 Verification Badges Across All Modules (new requirement)

Every role's profile (Student, Recruiter, Faculty, Admin) gets contact/social fields,
each with its own verified/unverified flag:

- Email — verified via confirmation link/OTP
- Phone Number — verified via OTP
- LinkedIn URL — verified (manual admin check or link-validity check)
- GitHub URL — verified (manual admin check or link-validity check)

Verified badges display next to these fields on profile and portfolio pages.

---

## 5. Module Tree

```
SKILLBRIDGE
│
├── Authentication
│   ├── Register (role selection)
│   ├── Login
│   └── Logout
│
├── Student
│   ├── Dashboard
│   ├── Profile (+ verification badges)
│   ├── Skills
│   ├── Certifications (+ upload/verification)
│   ├── Projects
│   ├── Career Goal
│   ├── Skill Gap
│   ├── Roadmap
│   ├── Recommendations
│   ├── Internships
│   ├── Applications
│   ├── Mentorship
│   └── Portfolio (public)
│
├── Recruiter
│   ├── Dashboard
│   ├── Company Profile (+ verification badges)
│   ├── Post Internship
│   ├── Applicants
│   ├── Candidate Matching
│   └── Evaluation (Selected / Rejected, auto + manual override)
│
├── Faculty
│   ├── Dashboard
│   ├── Students
│   ├── Skill Analytics
│   ├── Placement Readiness
│   ├── Certification Review (verify/reject uploads)
│   ├── Mentorship (accept/assign requests)
│   └── Reports
│
└── Admin
    ├── Users
    ├── Skills
    ├── Careers
    ├── Projects
    ├── Internships
    ├── Companies
    └── Reports
```

---

## 6. UI / UX Design Direction

Reference for visual and structural inspiration: **leapath.tech**, a placement
intelligence SaaS. Overall feel should be **enterprise SaaS, data-forward, minimal
decoration** — SkillBridge should look like a funded product, not a college project.
Numbers and live-looking dashboards do the persuading, not stock photos or bright
illustration.

### 6.1 Visual language borrowed from the reference

- **Hero pattern:** confident 5–7 word headline, one clarifying sentence, a primary
  solid CTA + secondary text link, a trust line under the CTAs, 4 key stats in a row
  (large number + small label), and a floating, slightly angled dashboard mockup with
  small floating "alert card" overlays to imply live software
- **"Perspectives" section:** one card per role (Student, Recruiter, Faculty, Admin —
  four for SkillBridge), each with a photo, one-line value prop, short paragraph,
  4-item bullet list, and a link ("Explore X features →")
- **Feature deep-dive:** numbered list (1–5) down the left with bold titles and
  1–2 sentence descriptions, matching realistic screenshot mockup on the right,
  alternating image-left/image-right
- **Social proof:** star rating + review count, horizontal scroll of testimonial cards
  (5-star row, specific quote, initials avatar, name/title/org) — optional for an
  early-stage product; use clearly-marked placeholders if included
- **Trust/verification visual:** a "Verified ✓" certificate mockup card with name,
  score, and a monospace hash string (e.g. `SHA-256 · 8f3a1c...9e02b4`) — reused
  directly for SkillBridge's Skill Verification and Portfolio pages
- **Color & type:**
  - Dark navy/charcoal dashboard UI (sidebar + panels) against a light, airy marketing
    page (white/very light grey) — this contrast reads as premium
  - Status colors: green = Ready/positive, amber/orange = At Risk/In Progress,
    red sparingly for negative deltas only
  - Small rounded-full badge pills everywhere (status, tags, deltas), low-opacity
    colored background
  - Numbers as the hero of the visual design: large, bold, tabular-nums, with small
    ▲/▼ deltas in green/red
  - Clean geometric/grotesk sans-serif (Inter or similar) throughout; generous
    line-height on body copy, tight tracking on large numbers/headlines
  - Generous whitespace on marketing pages; dense, information-rich layout inside
    dashboards (spacious to sell, dense to prove it's real software)
- **Closing CTA band:** full-width band before the footer, mirroring the hero
  (two buttons + trust line), then a standard multi-column footer

### 6.2 Page-by-page direction for SkillBridge

**Public landing page** (`/Home/Index`, unauthenticated)
- Hero headline centers on the differentiator: data-driven skill gap analysis +
  structured roadmaps + verified proof-of-work — not "just another job board."
  Stat row uses clearly-marked placeholder numbers (e.g. "X skills tracked",
  "Y career paths", "Z% average skill match improvement")
- Floating mockup: Student dashboard snapshot — readiness score ring, skill gap
  mini-chart, roadmap progress bar
- Four-role "perspectives" section (Student / Recruiter / Faculty / Admin)
- Feature deep-dive, in this order:
  1. Skill Gap Analysis — bar breakdown, Strong / Needs Improvement / Missing
  2. Career Roadmap — 5-level step tracker with ✓/→ icons
  3. Project & Internship Matching — match % badges on a ranked list
  4. Verified Portfolio — "Verified ✓ certificate with hash" visual
  5. Faculty Analytics — readiness distribution chart + "Top Missing Skills" bar list
- Optional testimonials section (placeholder quotes only)
- Closing CTA band ("See your own Skill Gap Analysis" / "Request a Demo") → Register/Login

**Student Dashboard** (`/Student/Dashboard`)
- Dark sidebar: Dashboard, Profile, Skills, Certifications, Projects, Career Goal,
  Skill Gap, Roadmap, Recommendations, Internships, Applications, Mentorship,
  Portfolio, Readiness
- Top row, 4–6 stat cards (number first, label + delta below): Career Readiness
  (ring/gauge %), Skill Gap (% match to career goal), Roadmap Progress (X of 5
  levels), Active Applications (count)
- Below: readiness component breakdown (Skills/Projects/Certs/Internships/
  Problem Solving/GitHub) as horizontal bars

**Recruiter Dashboard**
- Sidebar: Dashboard, Company Profile, Post Internship, Applicants, Candidate
  Matching, Evaluation
- Main: open postings count, total applicants, top candidate match %, ranked
  candidate list with match-% badges (green ≥ posting minimum, amber below)

**Faculty Dashboard**
- Sidebar: Dashboard, Students, Skill Analytics, Placement Readiness,
  Certification Review, Mentorship, Reports
- Main: Total Students / Placement Ready / Needs Improvement / Not Started as
  4 stat cards, plus a "Top Missing Skills" horizontal bar chart (N students label format)

**Admin Dashboard**
- Sidebar: Users, Skills, Careers, Projects, Internships, Companies, Reports
- Main: platform-wide stat cards (Total Users, Pending Verifications, Open
  Internships, etc.), same card style as the other three dashboards for consistency

**Public Portfolio page** (`/Portfolio/Index/{id}`)
- Treat as the single most premium-looking page in the product (it's the page
  recruiters/family/LinkedIn will actually see)
- Verified ✓ hash-style badge per certification, clean skills chip row, project
  grid with tech-stack tags, verified LinkedIn/GitHub badges next to links

### 6.3 Design deliverables

1. Design token set: color palette (hex values), type scale, spacing scale,
   border-radius scale, shadow styles
2. Component sketch: stat card, status badge, progress/roadmap stepper,
   match-% badge, sidebar nav item (active/inactive), data table row,
   verified-certificate card
3. Full-page mockups (desktop, ~1440px): public landing page, Student Dashboard,
   one secondary page (e.g. Skill Gap Analysis), Faculty Skill Analytics,
   public Portfolio page
4. Short rationale note: how each choice maps to "premium, professional,
   trustworthy" rather than "playful student app"

### 6.4 Technical constraint for implementation

Output is implemented as **server-rendered ASP.NET MVC Razor views (`.cshtml`)**
with **Bootstrap 5 + plain CSS** — not React or any JS framework. Favor
CSS Grid/Flexbox layouts, CSS-only progress bars/gauges where possible, and
vanilla JS or Chart.js for bar/line charts. Every dynamic number, badge, table
row, and chart on these pages is populated from the Web API / Data Access layer
described in Section 7 — none of it is static/hardcoded HTML.

---

## 7. Solution / Folder Structure (Visual Studio)

One solution, four projects, so MVC and Web API share the same data access code
and both talk to the same MySQL database — this is what keeps frontend and
backend connected end-to-end.

```
SkillBridge.sln
│
├── SkillBridge.Web                     [ASP.NET MVC — the front-end]
│   ├── Controllers
│   │   ├── AccountController.cs        (Login/Register/Logout)
│   │   ├── HomeController.cs           (public landing page)
│   │   ├── StudentController.cs
│   │   ├── RecruiterController.cs
│   │   ├── FacultyController.cs
│   │   ├── AdminController.cs
│   │   └── PortfolioController.cs      (public portfolio page)
│   ├── Models                          (MVC view models / DTOs for Razor views)
│   ├── Views
│   │   ├── Home
│   │   ├── Account
│   │   │   ├── Login.cshtml
│   │   │   └── Register.cshtml
│   │   ├── Student
│   │   │   ├── Dashboard.cshtml
│   │   │   ├── Profile.cshtml
│   │   │   ├── Skills.cshtml
│   │   │   ├── Certifications.cshtml
│   │   │   ├── Projects.cshtml
│   │   │   ├── CareerGoal.cshtml
│   │   │   ├── SkillGap.cshtml
│   │   │   ├── Roadmap.cshtml
│   │   │   ├── Recommendations.cshtml
│   │   │   ├── Internships.cshtml
│   │   │   ├── Applications.cshtml
│   │   │   └── Mentorship.cshtml
│   │   ├── Recruiter
│   │   │   ├── Dashboard.cshtml
│   │   │   ├── CompanyProfile.cshtml
│   │   │   ├── PostInternship.cshtml
│   │   │   ├── Applicants.cshtml
│   │   │   ├── CandidateMatching.cshtml
│   │   │   └── Evaluation.cshtml
│   │   ├── Faculty
│   │   │   ├── Dashboard.cshtml
│   │   │   ├── Students.cshtml
│   │   │   ├── SkillAnalytics.cshtml
│   │   │   ├── PlacementReadiness.cshtml
│   │   │   ├── CertificationReview.cshtml
│   │   │   ├── Mentorship.cshtml
│   │   │   └── Reports.cshtml
│   │   ├── Admin
│   │   │   ├── Users.cshtml
│   │   │   ├── Skills.cshtml
│   │   │   ├── Careers.cshtml
│   │   │   ├── Projects.cshtml
│   │   │   ├── Internships.cshtml
│   │   │   ├── Companies.cshtml
│   │   │   └── Reports.cshtml
│   │   ├── Portfolio
│   │   │   └── Index.cshtml
│   │   └── Shared
│   │       ├── _Layout.cshtml
│   │       ├── _SidebarStudent.cshtml
│   │       ├── _SidebarRecruiter.cshtml
│   │       ├── _SidebarFaculty.cshtml
│   │       ├── _SidebarAdmin.cshtml
│   │       ├── _StatCard.cshtml
│   │       ├── _StatusBadge.cshtml
│   │       ├── _MatchBadge.cshtml
│   │       └── _VerifiedBadge.cshtml
│   ├── Content
│   │   ├── css
│   │   │   ├── site.css
│   │   │   ├── tokens.css              (design tokens: colors, spacing, radius, shadows)
│   │   │   └── dashboard.css
│   │   └── uploads                     (certification files, project proofs)
│   ├── Scripts
│   │   ├── site.js
│   │   ├── charts.js                   (Chart.js setup for gap/roadmap/analytics charts)
│   │   └── api-client.js               (fetch wrapper calling SkillBridge.API)
│   ├── App_Start
│   │   ├── RouteConfig.cs
│   │   ├── BundleConfig.cs
│   │   └── FilterConfig.cs
│   ├── Global.asax / Global.asax.cs
│   └── Web.config                      (points to SkillBridge.API base URL + connection string)
│
├── SkillBridge.API                     [ASP.NET Web API — data/business logic layer]
│   ├── Controllers
│   │   ├── AuthController.cs           (login/register/token or session issuing)
│   │   ├── StudentController.cs
│   │   ├── SkillController.cs          (skill gap computation)
│   │   ├── RoadmapController.cs
│   │   ├── ProjectController.cs
│   │   ├── CertificationController.cs  (upload + verification status)
│   │   ├── InternshipController.cs
│   │   ├── ApplicationController.cs    (auto Selected/Rejected logic lives here)
│   │   ├── MentorshipController.cs
│   │   ├── RecruiterController.cs
│   │   ├── FacultyController.cs
│   │   ├── AdminController.cs
│   │   └── PortfolioController.cs
│   ├── Models                          (API request/response DTOs)
│   ├── App_Start
│   │   ├── WebApiConfig.cs
│   │   └── AuthConfig.cs               (shared session/cookie or token validation)
│   ├── Global.asax / Global.asax.cs
│   └── Web.config                      (connection string, CORS config for SkillBridge.Web)
│
├── SkillBridge.DataAccess              [Class Library — ADO.NET only, shared by Web + API]
│   ├── DBHelper.cs                     (MySqlConnection/MySqlCommand wrapper)
│   ├── Entities
│   │   ├── User.cs
│   │   ├── StudentProfile.cs
│   │   ├── Skill.cs
│   │   ├── StudentSkill.cs
│   │   ├── Certification.cs
│   │   ├── Project.cs
│   │   ├── CareerGoal.cs
│   │   ├── RoadmapLevel.cs
│   │   ├── Internship.cs
│   │   ├── Application.cs
│   │   ├── Mentorship.cs
│   │   ├── Company.cs
│   │   └── VerificationBadge.cs
│   └── Repositories
│       ├── UserRepository.cs
│       ├── SkillRepository.cs
│       ├── CertificationRepository.cs
│       ├── ProjectRepository.cs
│       ├── RoadmapRepository.cs
│       ├── InternshipRepository.cs
│       ├── ApplicationRepository.cs    (match % calc + auto decision)
│       ├── MentorshipRepository.cs
│       ├── ReadinessRepository.cs      (readiness score calc)
│       └── ReportRepository.cs         (faculty/admin analytics queries)
│
├── SkillBridge.Common                  [Class Library — shared helpers/constants]
│   ├── PasswordHasher.cs
│   ├── RoleConstants.cs
│   ├── MatchPercentageCalculator.cs
│   └── ApiResponse.cs                  (standard API response wrapper)
│
└── Database
    ├── 01_SkillBridge_Schema.sql       (all MySQL tables: Users, Skills, StudentSkills,
    │                                     Certifications, Projects, CareerGoals,
    │                                     RoadmapLevels, Internships, Applications,
    │                                     Mentorships, Companies, VerificationBadges, etc.)
    ├── 02_SkillBridge_SeedData.sql      (skills master list, career-goal skill maps,
    │                                     roadmap levels, sample projects)
    └── 03_SkillBridge_StoredProcs.sql   (optional: skill match %, readiness score procs)
```

**Why this structure keeps frontend and backend connected:**
- `SkillBridge.DataAccess` is the *only* place ADO.NET/MySQL calls are written —
  both `SkillBridge.Web` (for server-side page data) and `SkillBridge.API` (for
  AJAX/JSON calls from Razor views' JS) reference this same project, so there is
  one source of truth for every query (skill gap %, match %, readiness score, etc.)
- `SkillBridge.Web`'s `Scripts/api-client.js` calls `SkillBridge.API` for anything
  that needs to update without a full page reload (e.g. accepting a mentorship
  request, changing an evaluation decision) — auth session/cookie is shared so
  these calls are authenticated the same way as the MVC pages
- All four dashboards, the portfolio page, and every stat card/badge/chart
  described in Section 6 are rendered by their MVC controller pulling real data
  from `SkillBridge.DataAccess` (directly, or via a call to `SkillBridge.API`) —
  nothing in the UI is hardcoded

---

## 8. Deliverable Checklist

- [ ] Full Visual Studio solution matching the structure in Section 7, opens and
      runs with no missing references
- [ ] MySQL schema + seed data scripts (Section 7 → `Database/`)
- [ ] Role-based Register/Login/Logout, hashed passwords, session shared across
      MVC + API (Section 4.12)
- [ ] Skill Gap Analysis, Roadmap, Recommendations, Internship Matching working
      off real MySQL data (Sections 4.2–4.5)
- [ ] Recruiter Evaluation with auto Selected/Rejected + manual override (4.6)
- [ ] Readiness Score calculation (4.7)
- [ ] Public Student Portfolio page (4.8, 6.2)
- [ ] Faculty Dashboard with Top Missing Skills analytics (4.9)
- [ ] Recruiter Dashboard with ranked candidate list (4.10)
- [ ] Skill Verification with evidence checklist (4.11)
- [ ] Certification upload + Pending/Verified/Rejected workflow (4.13)
- [ ] Mentorship request/accept/notes flow (4.14)
- [ ] Verification badges (email/phone/LinkedIn/GitHub) on all four role profiles (4.15)
- [ ] UI matching Section 6 direction, implemented in Razor + Bootstrap 5 + Chart.js
- [ ] Every dashboard/page pulling live data — no hardcoded numbers in the final build
