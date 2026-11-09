using System.Data;
using MySqlConnector;

namespace SkillBridge.DataAccess.Repositories;

public class ReadinessRepository
{
    private readonly DBHelper _db;

    public ReadinessRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<StudentReadinessBreakdown> CalculateReadinessScoreAsync(int studentId)
    {
        // 1. Skills score
        string sqlSkills = "SELECT IFNULL(AVG(ConfidencePercentage), 0) FROM StudentSkills WHERE StudentId = @StudentId";
        var skillsObj = await _db.ExecuteScalarAsync(sqlSkills, new MySqlParameter("@StudentId", studentId));
        double skillsScore = Convert.ToDouble(skillsObj);

        // 2. Projects score
        string sqlProj = "SELECT COUNT(*) FROM StudentProjects WHERE StudentId = @StudentId";
        var projObj = await _db.ExecuteScalarAsync(sqlProj, new MySqlParameter("@StudentId", studentId));
        double projScore = Math.Min(100.0, Convert.ToInt32(projObj) * 25.0);

        // 3. Certifications score
        string sqlCerts = "SELECT COUNT(*) FROM Certifications WHERE StudentId = @StudentId AND VerificationStatus = 'Verified'";
        var certsObj = await _db.ExecuteScalarAsync(sqlCerts, new MySqlParameter("@StudentId", studentId));
        double certsScore = Math.Min(100.0, Convert.ToInt32(certsObj) * 30.0);

        // 4. Internships score
        string sqlIntern = "SELECT COUNT(*) FROM Applications WHERE StudentId = @StudentId AND FinalDecision = 'Selected'";
        var internObj = await _db.ExecuteScalarAsync(sqlIntern, new MySqlParameter("@StudentId", studentId));
        double internScore = Math.Min(100.0, Convert.ToInt32(internObj) * 50.0);

        // 5. Default problem solving & github activity metrics
        double problemSolvingScore = 75.0;
        double gitHubScore = 65.0;

        // Weighted Average Calculation
        double overallScore = (skillsScore * 0.30) + (projScore * 0.25) + (certsScore * 0.15) + 
                             (internScore * 0.15) + (problemSolvingScore * 0.075) + (gitHubScore * 0.075);

        overallScore = Math.Round(overallScore, 2);

        // Update database score
        string sqlUpdate = "UPDATE StudentProfiles SET ReadinessScore = @Score WHERE Id = @StudentId";
        await _db.ExecuteNonQueryAsync(sqlUpdate,
            new MySqlParameter("@Score", overallScore),
            new MySqlParameter("@StudentId", studentId));

        return new StudentReadinessBreakdown
        {
            OverallReadiness = overallScore,
            SkillsScore = Math.Round(skillsScore, 1),
            ProjectsScore = Math.Round(projScore, 1),
            CertificationsScore = Math.Round(certsScore, 1),
            InternshipsScore = Math.Round(internScore, 1),
            ProblemSolvingScore = problemSolvingScore,
            GitHubScore = gitHubScore
        };
    }
}

public class StudentReadinessBreakdown
{
    public double OverallReadiness { get; set; }
    public double SkillsScore { get; set; }
    public double ProjectsScore { get; set; }
    public double CertificationsScore { get; set; }
    public double InternshipsScore { get; set; }
    public double ProblemSolvingScore { get; set; }
    public double GitHubScore { get; set; }
}
