using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;
using SkillBridge.DataAccess.Repositories;

namespace SkillBridge.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

public class AccountController : Controller
{
    private readonly UserRepository _userRepo;

    public AccountController(UserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.Name);

        if (user.Role == RoleConstants.Student)
        {
            var studentProfile = await _userRepo.GetStudentProfileAsync(user.Id);
            if (studentProfile != null) HttpContext.Session.SetInt32("StudentId", studentProfile.Id);
            return RedirectToAction("Dashboard", "Student");
        }
        else if (user.Role == RoleConstants.Recruiter) return RedirectToAction("Dashboard", "Recruiter");
        else if (user.Role == RoleConstants.Faculty) return RedirectToAction("Dashboard", "Faculty");
        else return RedirectToAction("Users", "Admin");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string name, string email, string password, string role, string? phoneNumber)
    {
        var existing = await _userRepo.GetByEmailAsync(email);
        if (existing != null)
        {
            ViewBag.Error = "Email is already registered.";
            return View();
        }

        await _userRepo.RegisterAsync(name, email, password, role, phoneNumber);
        TempData["Success"] = "Account created successfully! Please sign in.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}

public class StudentController : Controller
{
    private readonly UserRepository _userRepo;
    private readonly SkillRepository _skillRepo;
    private readonly RoadmapRepository _roadmapRepo;
    private readonly ProjectRepository _projectRepo;
    private readonly CertificationRepository _certRepo;
    private readonly InternshipRepository _internshipRepo;
    private readonly ApplicationRepository _appRepo;
    private readonly MentorshipRepository _mentorshipRepo;
    private readonly ReadinessRepository _readinessRepo;

    public StudentController(
        UserRepository userRepo, SkillRepository skillRepo, RoadmapRepository roadmapRepo,
        ProjectRepository projectRepo, CertificationRepository certRepo, InternshipRepository internshipRepo,
        ApplicationRepository appRepo, MentorshipRepository mentorshipRepo, ReadinessRepository readinessRepo)
    {
        _userRepo = userRepo;
        _skillRepo = skillRepo;
        _roadmapRepo = roadmapRepo;
        _projectRepo = projectRepo;
        _certRepo = certRepo;
        _internshipRepo = internshipRepo;
        _appRepo = appRepo;
        _mentorshipRepo = mentorshipRepo;
        _readinessRepo = readinessRepo;
    }

    private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 1;

    public async Task<IActionResult> Dashboard()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        if (profile == null) return RedirectToAction("Login", "Account");

        var readiness = await _readinessRepo.CalculateReadinessScoreAsync(profile.Id);
        int careerGoalId = profile.TargetCareerGoalId ?? 1;
        var gap = await _skillRepo.GetSkillGapAnalysisAsync(profile.Id, careerGoalId);
        var apps = await _appRepo.GetStudentApplicationsAsync(profile.Id);
        var roadmap = await _roadmapRepo.GetRoadmapForStudentAsync(profile.Id, careerGoalId);

        ViewBag.Profile = profile;
        ViewBag.Readiness = readiness;
        ViewBag.SkillGapMatch = gap.OverallMatchPercentage;
        ViewBag.ActiveAppsCount = apps.Count;
        ViewBag.RoadmapCompletedCount = roadmap.Count(r => r.IsCompleted);
        ViewBag.RoadmapTotalCount = roadmap.Count;
        ViewBag.StudentSkills = await _skillRepo.GetStudentSkillsAsync(profile.Id);

        return View();
    }

    public async Task<IActionResult> Profile()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        var badges = await _userRepo.GetBadgesAsync(userId);
        ViewBag.Badges = badges;
        ViewBag.CareerGoals = await _skillRepo.GetAllCareerGoalsAsync();
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string? bio, string? college, string? degree, int? graduationYear, int? targetCareerGoalId, string? linkedInUrl, string? gitHubUrl)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        if (profile != null)
        {
            await _userRepo.UpdateStudentProfileAsync(profile.Id, bio, college, degree, graduationYear, targetCareerGoalId, linkedInUrl, gitHubUrl);
            TempData["Success"] = "Profile updated successfully!";
        }
        return RedirectToAction("Profile");
    }

    public async Task<IActionResult> Skills()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        ViewBag.MasterSkills = await _skillRepo.GetAllMasterSkillsAsync();
        var studentSkills = await _skillRepo.GetStudentSkillsAsync(profile!.Id);
        return View(studentSkills);
    }

    [HttpPost]
    public async Task<IActionResult> AddSkill(int skillId, int confidencePercentage)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        await _skillRepo.AddOrUpdateStudentSkillAsync(profile!.Id, skillId, confidencePercentage);
        TempData["Success"] = "Skill updated successfully!";
        return RedirectToAction("Skills");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSkill(int skillId)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        if (profile != null)
        {
            await _skillRepo.DeleteStudentSkillAsync(profile.Id, skillId);
            TempData["Success"] = "Skill removed successfully!";
        }
        return RedirectToAction("Skills");
    }

    public async Task<IActionResult> CareerGoal()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        ViewBag.CareerGoals = await _skillRepo.GetAllCareerGoalsAsync();
        return View(profile);
    }

    public async Task<IActionResult> SkillGap()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        int careerGoalId = profile?.TargetCareerGoalId ?? 1;
        var gapResult = await _skillRepo.GetSkillGapAnalysisAsync(profile!.Id, careerGoalId);
        ViewBag.Profile = profile;
        return View(gapResult);
    }

    public async Task<IActionResult> Roadmap()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        int careerGoalId = profile?.TargetCareerGoalId ?? 1;
        var levels = await _roadmapRepo.GetRoadmapForStudentAsync(profile!.Id, careerGoalId);
        ViewBag.Profile = profile;
        return View(levels);
    }

    public async Task<IActionResult> Recommendations()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        var recs = await _projectRepo.GetProjectRecommendationsAsync(profile!.Id, profile.TargetCareerGoalId);
        return View(recs);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitProject(int projectId, string repositoryUrl, string? liveDemoUrl, string? proofFilePath)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        await _projectRepo.SubmitStudentProjectAsync(profile!.Id, projectId, repositoryUrl, liveDemoUrl, proofFilePath);
        TempData["Success"] = "Project evidence submitted successfully!";
        return RedirectToAction("Recommendations");
    }

    public async Task<IActionResult> Certifications()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        ViewBag.MasterSkills = await _skillRepo.GetAllMasterSkillsAsync();
        var certs = await _certRepo.GetStudentCertificationsAsync(profile!.Id);
        return View(certs);
    }

    [HttpPost]
    public async Task<IActionResult> UploadCert(string title, string issuingOrganization, DateTime issueDate, string filePath, int linkedSkillId)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        await _certRepo.AddCertificationAsync(profile!.Id, title, issuingOrganization, issueDate, filePath, linkedSkillId);
        TempData["Success"] = "Certification uploaded! Awaiting faculty verification.";
        return RedirectToAction("Certifications");
    }

    public async Task<IActionResult> Internships()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        var matching = await _internshipRepo.GetInternshipsForStudentAsync(profile!.Id);
        return View(matching);
    }

    public async Task<IActionResult> Apply(int internshipId)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        await _appRepo.ApplyAsync(internshipId, profile!.Id);
        TempData["Success"] = "Application submitted! Auto-evaluation complete.";
        return RedirectToAction("Applications");
    }

    public async Task<IActionResult> Applications()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        var apps = await _appRepo.GetStudentApplicationsAsync(profile!.Id);
        return View(apps);
    }

    public async Task<IActionResult> Mentorship()
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        var list = await _mentorshipRepo.GetStudentMentorshipsAsync(profile!.Id);
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> RequestMentorship(string topic, string? initialNotes)
    {
        int userId = GetCurrentUserId();
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        await _mentorshipRepo.RequestMentorshipAsync(profile!.Id, topic, initialNotes);
        TempData["Success"] = "Mentorship requested!";
        return RedirectToAction("Mentorship");
    }
}

public class RecruiterController : Controller
{
    private readonly InternshipRepository _internshipRepo;
    private readonly ApplicationRepository _appRepo;
    private readonly SkillRepository _skillRepo;
    private readonly UserRepository _userRepo;

    public RecruiterController(InternshipRepository internshipRepo, ApplicationRepository appRepo, SkillRepository skillRepo, UserRepository userRepo)
    {
        _internshipRepo = internshipRepo;
        _appRepo = appRepo;
        _skillRepo = skillRepo;
        _userRepo = userRepo;
    }

    private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 2;

    public async Task<IActionResult> Dashboard()
    {
        int recruiterId = GetCurrentUserId();
        var internships = await _internshipRepo.GetInternshipsByRecruiterAsync(recruiterId);
        var apps = await _appRepo.GetApplicantsForRecruiterAsync(recruiterId);

        ViewBag.InternshipsCount = internships.Count;
        ViewBag.ApplicantsCount = apps.Count;
        ViewBag.SelectedCount = apps.Count(a => a.FinalDecision == "Selected");
        ViewBag.TopCandidates = apps.Take(5).ToList();

        return View(internships);
    }

    public async Task<IActionResult> CompanyProfile()
    {
        int userId = GetCurrentUserId();
        var user = await _userRepo.GetByIdAsync(userId);
        var company = await _userRepo.GetCompanyByUserIdAsync(userId);
        var badges = await _userRepo.GetBadgesAsync(userId);

        ViewBag.User = user;
        ViewBag.Badges = badges;
        return View(company);
    }

    public async Task<IActionResult> PostInternship()
    {
        ViewBag.MasterSkills = await _skillRepo.GetAllMasterSkillsAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PostInternship(string title, string companyName, string location, string? stipend, string? description, int[] requiredSkillIds, int minimumMatchPercentage)
    {
        int recruiterId = GetCurrentUserId();
        string reqSkillIdsStr = string.Join(",", requiredSkillIds);
        await _internshipRepo.PostInternshipAsync(recruiterId, title, companyName, location, stipend, description, reqSkillIdsStr, minimumMatchPercentage);
        TempData["Success"] = "Internship job posted successfully!";
        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> Applicants()
    {
        int recruiterId = GetCurrentUserId();
        var apps = await _appRepo.GetApplicantsForRecruiterAsync(recruiterId);
        return View(apps);
    }

    public async Task<IActionResult> CandidateMatching()
    {
        int recruiterId = GetCurrentUserId();
        var apps = await _appRepo.GetApplicantsForRecruiterAsync(recruiterId);
        return View(apps);
    }

    public async Task<IActionResult> Evaluation()
    {
        int recruiterId = GetCurrentUserId();
        var apps = await _appRepo.GetApplicantsForRecruiterAsync(recruiterId);
        return View(apps);
    }

    [HttpPost]
    public async Task<IActionResult> OverrideDecision(int applicationId, string decision, string? notes)
    {
        await _appRepo.UpdateDecisionAsync(applicationId, decision, notes);
        TempData["Success"] = "Recruiter decision updated successfully!";
        return RedirectToAction("Evaluation");
    }
}

public class FacultyController : Controller
{
    private readonly ReportRepository _reportRepo;
    private readonly CertificationRepository _certRepo;
    private readonly MentorshipRepository _mentorshipRepo;
    private readonly UserRepository _userRepo;

    public FacultyController(ReportRepository reportRepo, CertificationRepository certRepo, MentorshipRepository mentorshipRepo, UserRepository userRepo)
    {
        _reportRepo = reportRepo;
        _certRepo = certRepo;
        _mentorshipRepo = mentorshipRepo;
        _userRepo = userRepo;
    }

    private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 3;

    public async Task<IActionResult> Dashboard()
    {
        var analytics = await _reportRepo.GetFacultyAnalyticsSummaryAsync();
        ViewBag.Analytics = analytics;
        ViewBag.PendingCerts = await _certRepo.GetPendingCertificationsAsync();
        return View();
    }

    public async Task<IActionResult> SkillAnalytics()
    {
        var analytics = await _reportRepo.GetFacultyAnalyticsSummaryAsync();
        return View(analytics);
    }

    public async Task<IActionResult> PlacementReadiness()
    {
        var analytics = await _reportRepo.GetFacultyAnalyticsSummaryAsync();
        return View(analytics);
    }

    public async Task<IActionResult> CertificationReview()
    {
        var pending = await _certRepo.GetPendingCertificationsAsync();
        return View(pending);
    }

    [HttpPost]
    public async Task<IActionResult> ReviewCert(int certId, string status, string? notes)
    {
        int facultyId = GetCurrentUserId();
        await _certRepo.ReviewCertificationAsync(certId, facultyId, status, notes);
        TempData["Success"] = $"Certification {status} successfully.";
        return RedirectToAction("CertificationReview");
    }

    public async Task<IActionResult> Mentorship()
    {
        var list = await _mentorshipRepo.GetAllMentorshipRequestsAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> AcceptMentorship(int mentorshipId, string status, string? notes)
    {
        int facultyId = GetCurrentUserId();
        await _mentorshipRepo.AcceptOrUpdateMentorshipAsync(mentorshipId, facultyId, status, notes);
        TempData["Success"] = "Mentorship request updated!";
        return RedirectToAction("Mentorship");
    }

    public async Task<IActionResult> MentorAssignments() => await Mentorship();
    public async Task<IActionResult> Students()
    {
        var students = await _userRepo.GetAllStudentProfilesAsync();
        return View(students);
    }
    public async Task<IActionResult> Reports() => View(await _reportRepo.GetFacultyAnalyticsSummaryAsync());
}

public class AdminController : Controller
{
    private readonly ReportRepository _reportRepo;
    private readonly SkillRepository _skillRepo;
    private readonly UserRepository _userRepo;

    public AdminController(ReportRepository reportRepo, SkillRepository skillRepo, UserRepository userRepo)
    {
        _reportRepo = reportRepo;
        _skillRepo = skillRepo;
        _userRepo = userRepo;
    }

    public async Task<IActionResult> Users()
    {
        var stats = await _reportRepo.GetAdminStatsAsync();
        var users = await _userRepo.GetAllUsersAsync();
        ViewBag.Stats = stats;
        return View(users);
    }
    public async Task<IActionResult> Skills() => View(await _skillRepo.GetAllMasterSkillsAsync());
    public async Task<IActionResult> Careers() => View(await _skillRepo.GetAllCareerGoalsAsync());
    public async Task<IActionResult> Projects() => View();
    public async Task<IActionResult> Internships() => View();
    public async Task<IActionResult> Companies() => View();
    public async Task<IActionResult> Reports() => View(await _reportRepo.GetAdminStatsAsync());
    public async Task<IActionResult> SystemStats() => View(await _reportRepo.GetAdminStatsAsync());
    public async Task<IActionResult> VerifyCertification() => RedirectToAction(nameof(Users));
}

public class PortfolioController : Controller
{
    private readonly UserRepository _userRepo;
    private readonly SkillRepository _skillRepo;
    private readonly ProjectRepository _projectRepo;
    private readonly CertificationRepository _certRepo;

    public PortfolioController(UserRepository userRepo, SkillRepository skillRepo, ProjectRepository projectRepo, CertificationRepository certRepo)
    {
        _userRepo = userRepo;
        _skillRepo = skillRepo;
        _projectRepo = projectRepo;
        _certRepo = certRepo;
    }

    [HttpGet("Portfolio/Index/{id}")]
    public async Task<IActionResult> Index(int id)
    {
        var profile = await _userRepo.GetStudentProfileByStudentIdAsync(id);
        if (profile == null) return NotFound("Student portfolio not found.");

        var user = await _userRepo.GetByIdAsync(profile.UserId);
        var badges = await _userRepo.GetBadgesAsync(profile.UserId);
        var skills = await _skillRepo.GetStudentSkillsAsync(id);
        var certs = await _certRepo.GetStudentCertificationsAsync(id);
        var projects = await _projectRepo.GetProjectRecommendationsAsync(id, profile.TargetCareerGoalId);

        ViewBag.Profile = profile;
        ViewBag.User = user;
        ViewBag.Badges = badges;
        ViewBag.Skills = skills;
        ViewBag.Certifications = certs;
        ViewBag.Projects = projects;

        return View();
    }
}
