using System.Data;
using MySqlConnector;

namespace SkillBridge.DataAccess.Repositories;

public class ReportRepository
{
    private readonly DBHelper _db;

    public ReportRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<FacultyAnalyticsSummary> GetFacultyAnalyticsSummaryAsync()
    {
        string sqlStudents = "SELECT ReadinessScore FROM StudentProfiles";
        var dt = await _db.ExecuteQueryAsync(sqlStudents);

        int total = dt.Rows.Count;
        int placementReady = 0;
        int needsImprovement = 0;
        int notStarted = 0;

        foreach (DataRow r in dt.Rows)
        {
            double score = Convert.ToDouble(r["ReadinessScore"]);
            if (score >= 70.0) placementReady++;
            else if (score >= 40.0) needsImprovement++;
            else notStarted++;
        }

        // Top Missing Skills query
        string sqlMissing = @"SELECT s.Name as SkillName, COUNT(cs.StudentId) as MissingCount
                              FROM (
                                  SELECT sp.Id as StudentId, cs.SkillId
                                  FROM StudentProfiles sp
                                  JOIN CareerSkills cs ON sp.TargetCareerGoalId = cs.CareerGoalId
                                  LEFT JOIN StudentSkills ss ON sp.Id = ss.StudentId AND cs.SkillId = ss.SkillId
                                  WHERE ss.Id IS NULL OR ss.ConfidencePercentage < 50
                              ) cs
                              JOIN Skills s ON cs.SkillId = s.Id
                              GROUP BY s.Name
                              ORDER BY MissingCount DESC
                              LIMIT 8";

        var dtMissing = await _db.ExecuteQueryAsync(sqlMissing);
        var missingSkillsList = new List<SkillMissingStat>();
        foreach (DataRow r in dtMissing.Rows)
        {
            missingSkillsList.Add(new SkillMissingStat
            {
                SkillName = r["SkillName"].ToString()!,
                StudentCount = Convert.ToInt32(r["MissingCount"])
            });
        }

        return new FacultyAnalyticsSummary
        {
            TotalStudents = total,
            PlacementReadyCount = placementReady,
            NeedsImprovementCount = needsImprovement,
            NotStartedCount = notStarted,
            TopMissingSkills = missingSkillsList
        };
    }

    public async Task<AdminStatsSummary> GetAdminStatsAsync()
    {
        int totalUsers = Convert.ToInt32(await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Users"));
        int pendingCerts = Convert.ToInt32(await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Certifications WHERE VerificationStatus = 'Pending'"));
        int openInternships = Convert.ToInt32(await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Internships"));
        int totalApps = Convert.ToInt32(await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Applications"));

        return new AdminStatsSummary
        {
            TotalUsers = totalUsers,
            PendingVerifications = pendingCerts,
            OpenInternships = openInternships,
            TotalApplications = totalApps
        };
    }
}

public class FacultyAnalyticsSummary
{
    public int TotalStudents { get; set; }
    public int PlacementReadyCount { get; set; }
    public int NeedsImprovementCount { get; set; }
    public int NotStartedCount { get; set; }
    public List<SkillMissingStat> TopMissingSkills { get; set; } = new();
}

public class SkillMissingStat
{
    public string SkillName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}

public class AdminStatsSummary
{
    public int TotalUsers { get; set; }
    public int PendingVerifications { get; set; }
    public int OpenInternships { get; set; }
    public int TotalApplications { get; set; }
}
