using System.Data;
using MySqlConnector;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class ApplicationRepository
{
    private readonly DBHelper _db;
    private readonly InternshipRepository _internshipRepo;
    private readonly SkillRepository _skillRepo;

    public ApplicationRepository(DBHelper db, InternshipRepository internshipRepo, SkillRepository skillRepo)
    {
        _db = db;
        _internshipRepo = internshipRepo;
        _skillRepo = skillRepo;
    }

    public async Task<int> ApplyAsync(int internshipId, int studentId)
    {
        var internship = await _internshipRepo.GetByIdAsync(internshipId);
        if (internship == null) throw new InvalidOperationException("Internship not found.");

        var studentSkills = await _skillRepo.GetStudentSkillsAsync(studentId);
        var studentSkillIds = studentSkills.Select(s => s.SkillId).ToHashSet();

        var reqSkillIds = internship.RequiredSkillIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToList();

        int matchPct = MatchPercentageCalculator.Calculate(reqSkillIds, studentSkillIds);

        // Auto Decision Rule: Match % >= Minimum Match % -> Selected, else Rejected
        string autoDecision = matchPct >= internship.MinimumMatchPercentage ? "Selected" : "Rejected";
        string finalDecision = autoDecision;

        string sql = @"INSERT INTO Applications (InternshipId, StudentId, MatchPercentage, AutoDecision, FinalDecision, DecisionDate)
                       VALUES (@InternshipId, @StudentId, @MatchPct, @AutoDec, @FinalDec, NOW())
                       ON DUPLICATE KEY UPDATE MatchPercentage = @MatchPct, AutoDecision = @AutoDec, FinalDecision = @FinalDec, DecisionDate = NOW();
                       SELECT LAST_INSERT_ID();";

        var idObj = await _db.ExecuteScalarAsync(sql,
            new MySqlParameter("@InternshipId", internshipId),
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@MatchPct", matchPct),
            new MySqlParameter("@AutoDec", autoDecision),
            new MySqlParameter("@FinalDec", finalDecision));

        return Convert.ToInt32(idObj);
    }

    public async Task UpdateDecisionAsync(int applicationId, string decision, string? notes)
    {
        string sql = @"UPDATE Applications 
                       SET FinalDecision = @Decision, Notes = @Notes, DecisionDate = NOW() 
                       WHERE Id = @Id;";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@Decision", decision),
            new MySqlParameter("@Notes", (object?)notes ?? DBNull.Value),
            new MySqlParameter("@Id", applicationId));
    }

    public async Task<List<Application>> GetStudentApplicationsAsync(int studentId)
    {
        string sql = @"SELECT a.*, i.Title as InternshipTitle, i.CompanyName, i.MinimumMatchPercentage
                       FROM Applications a
                       JOIN Internships i ON a.InternshipId = i.Id
                       WHERE a.StudentId = @StudentId
                       ORDER BY a.DecisionDate DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@StudentId", studentId));
        return MapList(dt);
    }

    public async Task<List<Application>> GetApplicantsForInternshipAsync(int internshipId)
    {
        string sql = @"SELECT a.*, i.Title as InternshipTitle, i.CompanyName, i.MinimumMatchPercentage,
                              u.Name as StudentName, u.Email as StudentEmail
                       FROM Applications a
                       JOIN Internships i ON a.InternshipId = i.Id
                       JOIN StudentProfiles sp ON a.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       WHERE a.InternshipId = @InternshipId
                       ORDER BY a.MatchPercentage DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@InternshipId", internshipId));
        return MapList(dt);
    }

    public async Task<List<Application>> GetApplicantsForRecruiterAsync(int recruiterId)
    {
        string sql = @"SELECT a.*, i.Title as InternshipTitle, i.CompanyName, i.MinimumMatchPercentage,
                              u.Name as StudentName, u.Email as StudentEmail
                       FROM Applications a
                       JOIN Internships i ON a.InternshipId = i.Id
                       JOIN StudentProfiles sp ON a.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       WHERE i.RecruiterId = @RecruiterId
                       ORDER BY a.MatchPercentage DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@RecruiterId", recruiterId));
        return MapList(dt);
    }

    private static List<Application> MapList(DataTable dt)
    {
        var list = new List<Application>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Application
            {
                Id = Convert.ToInt32(r["Id"]),
                InternshipId = Convert.ToInt32(r["InternshipId"]),
                StudentId = Convert.ToInt32(r["StudentId"]),
                MatchPercentage = Convert.ToInt32(r["MatchPercentage"]),
                AutoDecision = r["AutoDecision"].ToString()!,
                FinalDecision = r["FinalDecision"].ToString()!,
                DecisionDate = Convert.ToDateTime(r["DecisionDate"]),
                Notes = r["Notes"] == DBNull.Value ? null : r["Notes"].ToString(),
                InternshipTitle = r.Table.Columns.Contains("InternshipTitle") ? r["InternshipTitle"].ToString()! : "",
                CompanyName = r.Table.Columns.Contains("CompanyName") ? r["CompanyName"].ToString()! : "",
                MinimumMatchPercentage = r.Table.Columns.Contains("MinimumMatchPercentage") ? Convert.ToInt32(r["MinimumMatchPercentage"]) : 0,
                StudentName = r.Table.Columns.Contains("StudentName") ? r["StudentName"].ToString()! : "",
                StudentEmail = r.Table.Columns.Contains("StudentEmail") ? r["StudentEmail"].ToString()! : ""
            });
        }
        return list;
    }
}
