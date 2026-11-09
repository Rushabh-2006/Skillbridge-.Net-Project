using System.Data;
using MySqlConnector;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class ProjectRepository
{
    private readonly DBHelper _db;
    private readonly SkillRepository _skillRepo;

    public ProjectRepository(DBHelper db, SkillRepository skillRepo)
    {
        _db = db;
        _skillRepo = skillRepo;
    }

    public async Task<List<Project>> GetAllMasterProjectsAsync()
    {
        string sql = "SELECT * FROM Projects ORDER BY Id";
        var dt = await _db.ExecuteQueryAsync(sql);
        var list = new List<Project>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(MapProject(r));
        }
        return list;
    }

    public async Task<List<Project>> GetProjectRecommendationsAsync(int studentId, int? targetCareerGoalId = null)
    {
        var masterProjects = await GetAllMasterProjectsAsync();
        var studentSkills = await _skillRepo.GetStudentSkillsAsync(studentId);
        var studentSkillIds = studentSkills.Select(s => s.SkillId).ToHashSet();
        var allSkills = (await _skillRepo.GetAllMasterSkillsAsync()).ToDictionary(s => s.Id, s => s.Name);

        HashSet<int>? careerSkillIds = null;
        if (targetCareerGoalId.HasValue && targetCareerGoalId.Value > 0)
        {
            string sqlCareerSkills = "SELECT SkillId FROM CareerSkills WHERE CareerGoalId = @CareerGoalId";
            var dtCareer = await _db.ExecuteQueryAsync(sqlCareerSkills, new MySqlParameter("@CareerGoalId", targetCareerGoalId.Value));
            careerSkillIds = new HashSet<int>();
            foreach (DataRow row in dtCareer.Rows)
            {
                careerSkillIds.Add(Convert.ToInt32(row["SkillId"]));
            }
        }

        foreach (var proj in masterProjects)
        {
            var reqSkillIds = proj.RequiredSkillIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();

            proj.MatchPercentage = MatchPercentageCalculator.Calculate(reqSkillIds, studentSkillIds);

            var missingIds = reqSkillIds.Where(id => !studentSkillIds.Contains(id)).ToList();
            proj.MissingSkillNames = missingIds
                .Where(id => allSkills.ContainsKey(id))
                .Select(id => allSkills[id])
                .ToList();
        }

        if (careerSkillIds != null && careerSkillIds.Count > 0)
        {
            // Rank projects by how many skills align with the target career goal first, then by match percentage
            return masterProjects.OrderByDescending(p =>
            {
                var reqIds = p.RequiredSkillIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(int.Parse);
                return reqIds.Count(id => careerSkillIds.Contains(id));
            }).ThenByDescending(p => p.MatchPercentage).ToList();
        }

        return masterProjects.OrderByDescending(p => p.MatchPercentage).ToList();
    }

    public async Task SubmitStudentProjectAsync(int studentId, int projectId, string repoUrl, string? liveUrl, string? proofPath)
    {
        string sql = @"INSERT INTO StudentProjects (StudentId, ProjectId, RepositoryUrl, LiveDemoUrl, ProofFilePath, SubmittedAt, Status)
                       VALUES (@StudentId, @ProjectId, @RepoUrl, @LiveUrl, @ProofPath, NOW(), 'Submitted');";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@ProjectId", projectId),
            new MySqlParameter("@RepoUrl", repoUrl),
            new MySqlParameter("@LiveUrl", (object?)liveUrl ?? DBNull.Value),
            new MySqlParameter("@ProofPath", (object?)proofPath ?? DBNull.Value));

        // Get required skills for this project and mark evidence on student skills
        var dt = await _db.ExecuteQueryAsync("SELECT RequiredSkillIds FROM Projects WHERE Id = @Id", new MySqlParameter("@Id", projectId));
        if (dt.Rows.Count > 0)
        {
            string reqIdsStr = dt.Rows[0]["RequiredSkillIds"].ToString()!;
            var reqIds = reqIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse);
            foreach (var sId in reqIds)
            {
                await _skillRepo.VerifySkillEvidenceAsync(studentId, sId, "Project");
            }
        }
    }

    private static Project MapProject(DataRow r)
    {
        return new Project
        {
            Id = Convert.ToInt32(r["Id"]),
            Title = r["Title"].ToString()!,
            Difficulty = r["Difficulty"].ToString()!,
            Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
            RequiredSkillIds = r["RequiredSkillIds"].ToString()!
        };
    }
}
