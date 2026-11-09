using Microsoft.AspNetCore.Mvc;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;
using SkillBridge.DataAccess.Repositories;

namespace SkillBridge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepo;

    public AuthController(UserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userRepo.GetByEmailAsync(model.Email);
        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid email or password."));
        }

        var studentProfile = user.Role == RoleConstants.Student ? await _userRepo.GetStudentProfileAsync(user.Id) : null;
        var badges = await _userRepo.GetBadgesAsync(user.Id);

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            StudentId = studentProfile?.Id,
            Badges = badges
        }, "Login successful"));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var existing = await _userRepo.GetByEmailAsync(model.Email);
        if (existing != null)
        {
            return BadRequest(ApiResponse<object>.Fail("Email already registered."));
        }

        int userId = await _userRepo.RegisterAsync(model.Name, model.Email, model.Password, model.Role, model.PhoneNumber);
        return Ok(ApiResponse<object>.Ok(new { UserId = userId }, "Registration successful"));
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Name, string Email, string Password, string Role, string? PhoneNumber);

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly UserRepository _userRepo;
    private readonly ReadinessRepository _readinessRepo;

    public StudentController(UserRepository userRepo, ReadinessRepository readinessRepo)
    {
        _userRepo = userRepo;
        _readinessRepo = readinessRepo;
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetProfile(int userId)
    {
        var profile = await _userRepo.GetStudentProfileAsync(userId);
        if (profile == null) return NotFound(ApiResponse<object>.Fail("Student profile not found."));

        var badges = await _userRepo.GetBadgesAsync(userId);
        var readiness = await _readinessRepo.CalculateReadinessScoreAsync(profile.Id);

        return Ok(ApiResponse<object>.Ok(new { Profile = profile, Badges = badges, Readiness = readiness }));
    }

    [HttpPost("profile/update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        await _userRepo.UpdateStudentProfileAsync(req.StudentId, req.Bio, req.College, req.Degree, req.GraduationYear, req.TargetCareerGoalId, req.LinkedInUrl, req.GitHubUrl);
        return Ok(ApiResponse<string>.Ok("Profile updated successfully."));
    }
}

public record UpdateProfileRequest(int StudentId, string? Bio, string? College, string? Degree, int? GraduationYear, int? TargetCareerGoalId, string? LinkedInUrl, string? GitHubUrl);

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly SkillRepository _skillRepo;

    public SkillController(SkillRepository skillRepo)
    {
        _skillRepo = skillRepo;
    }

    [HttpGet("master")]
    public async Task<IActionResult> GetMasterSkills()
    {
        var list = await _skillRepo.GetAllMasterSkillsAsync();
        return Ok(ApiResponse<List<Skill>>.Ok(list));
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentSkills(int studentId)
    {
        var list = await _skillRepo.GetStudentSkillsAsync(studentId);
        return Ok(ApiResponse<List<StudentSkill>>.Ok(list));
    }

    [HttpPost("student/add-update")]
    public async Task<IActionResult> AddOrUpdateStudentSkill([FromBody] AddSkillRequest req)
    {
        await _skillRepo.AddOrUpdateStudentSkillAsync(req.StudentId, req.SkillId, req.ConfidencePercentage);
        return Ok(ApiResponse<string>.Ok("Skill updated."));
    }

    [HttpGet("gap/{studentId}/{careerGoalId}")]
    public async Task<IActionResult> GetSkillGap(int studentId, int careerGoalId)
    {
        var result = await _skillRepo.GetSkillGapAnalysisAsync(studentId, careerGoalId);
        return Ok(ApiResponse<SkillGapAnalysisResult>.Ok(result));
    }
}

public record AddSkillRequest(int StudentId, int SkillId, int ConfidencePercentage);

[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly RoadmapRepository _roadmapRepo;

    public RoadmapController(RoadmapRepository roadmapRepo)
    {
        _roadmapRepo = roadmapRepo;
    }

    [HttpGet("{studentId}/{careerGoalId}")]
    public async Task<IActionResult> GetRoadmap(int studentId, int careerGoalId)
    {
        var list = await _roadmapRepo.GetRoadmapForStudentAsync(studentId, careerGoalId);
        return Ok(ApiResponse<List<RoadmapLevel>>.Ok(list));
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ProjectRepository _projectRepo;

    public ProjectController(ProjectRepository projectRepo)
    {
        _projectRepo = projectRepo;
    }

    [HttpGet("recommendations/{studentId}")]
    public async Task<IActionResult> GetRecommendations(int studentId)
    {
        var list = await _projectRepo.GetProjectRecommendationsAsync(studentId);
        return Ok(ApiResponse<List<Project>>.Ok(list));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitProject([FromBody] SubmitProjectRequest req)
    {
        await _projectRepo.SubmitStudentProjectAsync(req.StudentId, req.ProjectId, req.RepositoryUrl, req.LiveDemoUrl, req.ProofFilePath);
        return Ok(ApiResponse<string>.Ok("Project proof submitted successfully."));
    }
}

public record SubmitProjectRequest(int StudentId, int ProjectId, string RepositoryUrl, string? LiveDemoUrl, string? ProofFilePath);

[ApiController]
[Route("api/[controller]")]
public class CertificationController : ControllerBase
{
    private readonly CertificationRepository _certRepo;

    public CertificationController(CertificationRepository certRepo)
    {
        _certRepo = certRepo;
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentCertifications(int studentId)
    {
        var list = await _certRepo.GetStudentCertificationsAsync(studentId);
        return Ok(ApiResponse<List<Certification>>.Ok(list));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingCertifications()
    {
        var list = await _certRepo.GetPendingCertificationsAsync();
        return Ok(ApiResponse<List<Certification>>.Ok(list));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCert([FromBody] UploadCertRequest req)
    {
        int id = await _certRepo.AddCertificationAsync(req.StudentId, req.Title, req.IssuingOrganization, req.IssueDate, req.FilePath, req.LinkedSkillId);
        return Ok(ApiResponse<int>.Ok(id, "Certification uploaded successfully (Pending verification)."));
    }

    [HttpPut("review")]
    public async Task<IActionResult> ReviewCert([FromBody] ReviewCertRequest req)
    {
        await _certRepo.ReviewCertificationAsync(req.CertificationId, req.FacultyId, req.Status, req.Notes);
        return Ok(ApiResponse<string>.Ok($"Certification status updated to {req.Status}."));
    }
}

public record UploadCertRequest(int StudentId, string Title, string IssuingOrganization, DateTime IssueDate, string FilePath, int LinkedSkillId);
public record ReviewCertRequest(int CertificationId, int FacultyId, string Status, string? Notes);

[ApiController]
[Route("api/[controller]")]
public class InternshipController : ControllerBase
{
    private readonly InternshipRepository _internshipRepo;

    public InternshipController(InternshipRepository internshipRepo)
    {
        _internshipRepo = internshipRepo;
    }

    [HttpGet("matching/{studentId}")]
    public async Task<IActionResult> GetMatchingInternships(int studentId)
    {
        var list = await _internshipRepo.GetInternshipsForStudentAsync(studentId);
        return Ok(ApiResponse<List<Internship>>.Ok(list));
    }

    [HttpGet("recruiter/{recruiterId}")]
    public async Task<IActionResult> GetRecruiterInternships(int recruiterId)
    {
        var list = await _internshipRepo.GetInternshipsByRecruiterAsync(recruiterId);
        return Ok(ApiResponse<List<Internship>>.Ok(list));
    }

    [HttpPost("post")]
    public async Task<IActionResult> PostInternship([FromBody] PostInternshipRequest req)
    {
        int id = await _internshipRepo.PostInternshipAsync(req.RecruiterId, req.Title, req.CompanyName, req.Location, req.Stipend, req.Description, req.RequiredSkillIds, req.MinimumMatchPercentage);
        return Ok(ApiResponse<int>.Ok(id, "Internship posted successfully."));
    }
}

public record PostInternshipRequest(int RecruiterId, string Title, string CompanyName, string Location, string? Stipend, string? Description, string RequiredSkillIds, int MinimumMatchPercentage);

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly ApplicationRepository _appRepo;

    public ApplicationController(ApplicationRepository appRepo)
    {
        _appRepo = appRepo;
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest req)
    {
        int id = await _appRepo.ApplyAsync(req.InternshipId, req.StudentId);
        return Ok(ApiResponse<int>.Ok(id, "Application submitted (Auto-evaluated)."));
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentApps(int studentId)
    {
        var list = await _appRepo.GetStudentApplicationsAsync(studentId);
        return Ok(ApiResponse<List<Application>>.Ok(list));
    }

    [HttpGet("recruiter/{recruiterId}")]
    public async Task<IActionResult> GetRecruiterApps(int recruiterId)
    {
        var list = await _appRepo.GetApplicantsForRecruiterAsync(recruiterId);
        return Ok(ApiResponse<List<Application>>.Ok(list));
    }

    [HttpPut("override-decision")]
    public async Task<IActionResult> OverrideDecision([FromBody] OverrideDecisionRequest req)
    {
        await _appRepo.UpdateDecisionAsync(req.ApplicationId, req.Decision, req.Notes);
        return Ok(ApiResponse<string>.Ok("Decision updated successfully."));
    }
}

public record ApplyRequest(int InternshipId, int StudentId);
public record OverrideDecisionRequest(int ApplicationId, string Decision, string? Notes);

[ApiController]
[Route("api/[controller]")]
public class MentorshipController : ControllerBase
{
    private readonly MentorshipRepository _mentorshipRepo;

    public MentorshipController(MentorshipRepository mentorshipRepo)
    {
        _mentorshipRepo = mentorshipRepo;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestMentorship([FromBody] RequestMentorshipModel req)
    {
        int id = await _mentorshipRepo.RequestMentorshipAsync(req.StudentId, req.Topic, req.InitialNotes);
        return Ok(ApiResponse<int>.Ok(id, "Mentorship requested."));
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentMentorships(int studentId)
    {
        var list = await _mentorshipRepo.GetStudentMentorshipsAsync(studentId);
        return Ok(ApiResponse<List<Mentorship>>.Ok(list));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllMentorships()
    {
        var list = await _mentorshipRepo.GetAllMentorshipRequestsAsync();
        return Ok(ApiResponse<List<Mentorship>>.Ok(list));
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateMentorship([FromBody] UpdateMentorshipModel req)
    {
        await _mentorshipRepo.AcceptOrUpdateMentorshipAsync(req.MentorshipId, req.FacultyId, req.Status, req.Notes);
        return Ok(ApiResponse<string>.Ok("Mentorship updated."));
    }
}

public record RequestMentorshipModel(int StudentId, string Topic, string? InitialNotes);
public record UpdateMentorshipModel(int MentorshipId, int FacultyId, string Status, string? Notes);

[ApiController]
[Route("api/[controller]")]
public class FacultyController : ControllerBase
{
    private readonly ReportRepository _reportRepo;

    public FacultyController(ReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var data = await _reportRepo.GetFacultyAnalyticsSummaryAsync();
        return Ok(ApiResponse<FacultyAnalyticsSummary>.Ok(data));
    }
}

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ReportRepository _reportRepo;

    public AdminController(ReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetAdminStats()
    {
        var stats = await _reportRepo.GetAdminStatsAsync();
        return Ok(ApiResponse<AdminStatsSummary>.Ok(stats));
    }
}

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
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

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetPublicPortfolio(int studentId)
    {
        var profile = await _userRepo.GetStudentProfileByStudentIdAsync(studentId);
        if (profile == null) return NotFound(ApiResponse<object>.Fail("Portfolio not found."));

        var user = await _userRepo.GetByIdAsync(profile.UserId);
        var badges = await _userRepo.GetBadgesAsync(profile.UserId);
        var skills = await _skillRepo.GetStudentSkillsAsync(studentId);
        var certs = await _certRepo.GetStudentCertificationsAsync(studentId);
        var projects = await _projectRepo.GetProjectRecommendationsAsync(studentId);

        return Ok(ApiResponse<object>.Ok(new
        {
            Student = profile,
            User = user,
            Badges = badges,
            Skills = skills,
            Certifications = certs,
            Projects = projects
        }));
    }
}
