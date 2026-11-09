namespace SkillBridge.DataAccess.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

public class StudentProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Bio { get; set; }
    public string? College { get; set; }
    public string? Degree { get; set; }
    public int? GraduationYear { get; set; }
    public int? TargetCareerGoalId { get; set; }
    public decimal ReadinessScore { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }

    // Joined properties
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? TargetCareerTitle { get; set; }
}

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class StudentSkill
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SkillId { get; set; }
    public int ConfidencePercentage { get; set; }
    public bool HasProjectEvidence { get; set; }
    public bool HasCertificateEvidence { get; set; }
    public bool HasAssessmentEvidence { get; set; }
    public bool HasGitHubEvidence { get; set; }
    public bool HasFacultyVerification { get; set; }
    public string VerificationStatus { get; set; } = "UNVERIFIED";

    // Joined properties
    public string SkillName { get; set; } = string.Empty;
    public string SkillCategory { get; set; } = string.Empty;
}

public class CareerGoal
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RoadmapLevel
{
    public int Id { get; set; }
    public int CareerGoalId { get; set; }
    public int LevelNumber { get; set; }
    public string LevelTitle { get; set; } = string.Empty;
    public int SkillId { get; set; }
    public string? Description { get; set; }

    // Joined properties
    public string SkillName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RequiredSkillIds { get; set; } = string.Empty;
    
    // Computed property
    public int MatchPercentage { get; set; }
    public List<string> MissingSkillNames { get; set; } = new();
}

public class Certification
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int LinkedSkillId { get; set; }
    public string VerificationStatus { get; set; } = "Pending";
    public string? VerificationHash { get; set; }
    public int? ReviewedByFacultyId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Joined properties
    public string StudentName { get; set; } = string.Empty;
    public string LinkedSkillName { get; set; } = string.Empty;
}

public class Internship
{
    public int Id { get; set; }
    public int RecruiterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Stipend { get; set; }
    public string? Description { get; set; }
    public string RequiredSkillIds { get; set; } = string.Empty;
    public int MinimumMatchPercentage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Computed property
    public int CandidateMatchPercentage { get; set; }
}

public class Application
{
    public int Id { get; set; }
    public int InternshipId { get; set; }
    public int StudentId { get; set; }
    public int MatchPercentage { get; set; }
    public string AutoDecision { get; set; } = "Pending";
    public string FinalDecision { get; set; } = "Pending";
    public DateTime DecisionDate { get; set; }
    public string? Notes { get; set; }

    // Joined properties
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string InternshipTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int MinimumMatchPercentage { get; set; }
}

public class Mentorship
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int? FacultyId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = "Requested";
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }

    // Joined properties
    public string StudentName { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
}

public class VerificationBadge
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsLinkedInVerified { get; set; }
    public bool IsGitHubVerified { get; set; }
}
