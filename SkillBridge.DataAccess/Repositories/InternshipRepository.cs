using System.Data;
using MySqlConnector;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class InternshipRepository
{
    private readonly DBHelper _db;
    private readonly SkillRepository _skillRepo;

    public InternshipRepository(DBHelper db, SkillRepository skillRepo)
    {
        _db = db;
        _skillRepo = skillRepo;
    }

    public async Task<int> PostInternshipAsync(int recruiterId, string title, string companyName, string location, string? stipend, string? description, string reqSkillIds, int minMatchPct)
    {
        string sql = @"INSERT INTO Internships (RecruiterId, Title, CompanyName, Location, Stipend, Description, RequiredSkillIds, MinimumMatchPercentage, CreatedAt)
                       VALUES (@RecruiterId, @Title, @CompanyName, @Location, @Stipend, @Description, @ReqSkillIds, @MinMatchPct, NOW());
                       SELECT LAST_INSERT_ID();";

        var idObj = await _db.ExecuteScalarAsync(sql,
            new MySqlParameter("@RecruiterId", recruiterId),
            new MySqlParameter("@Title", title),
            new MySqlParameter("@CompanyName", companyName),
            new MySqlParameter("@Location", location),
            new MySqlParameter("@Stipend", (object?)stipend ?? DBNull.Value),
            new MySqlParameter("@Description", (object?)description ?? DBNull.Value),
            new MySqlParameter("@ReqSkillIds", reqSkillIds),
            new MySqlParameter("@MinMatchPct", minMatchPct));

        return Convert.ToInt32(idObj);
    }

    public async Task<List<Internship>> GetInternshipsForStudentAsync(int studentId)
    {
        string sql = "SELECT * FROM Internships ORDER BY CreatedAt DESC";
        var dt = await _db.ExecuteQueryAsync(sql);
        var internships = MapList(dt);

        var studentSkills = await _skillRepo.GetStudentSkillsAsync(studentId);
        var studentSkillIds = studentSkills.Select(s => s.SkillId).ToHashSet();

        foreach (var i in internships)
        {
            var reqSkillIds = i.RequiredSkillIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();

            i.CandidateMatchPercentage = MatchPercentageCalculator.Calculate(reqSkillIds, studentSkillIds);
        }

        return internships.OrderByDescending(i => i.CandidateMatchPercentage).ToList();
    }

    public async Task<List<Internship>> GetInternshipsByRecruiterAsync(int recruiterId)
    {
        string sql = "SELECT * FROM Internships WHERE RecruiterId = @RecruiterId ORDER BY CreatedAt DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@RecruiterId", recruiterId));
        return MapList(dt);
    }

    public async Task<Internship?> GetByIdAsync(int internshipId)
    {
        string sql = "SELECT * FROM Internships WHERE Id = @Id LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@Id", internshipId));
        if (dt.Rows.Count == 0) return null;
        return MapList(dt)[0];
    }

    private static List<Internship> MapList(DataTable dt)
    {
        var list = new List<Internship>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Internship
            {
                Id = Convert.ToInt32(r["Id"]),
                RecruiterId = Convert.ToInt32(r["RecruiterId"]),
                Title = r["Title"].ToString()!,
                CompanyName = r["CompanyName"].ToString()!,
                Location = r["Location"].ToString()!,
                Stipend = r["Stipend"] == DBNull.Value ? null : r["Stipend"].ToString(),
                Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
                RequiredSkillIds = r["RequiredSkillIds"].ToString()!,
                MinimumMatchPercentage = Convert.ToInt32(r["MinimumMatchPercentage"]),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"])
            });
        }
        return list;
    }
}
