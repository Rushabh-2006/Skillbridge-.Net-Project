using System.Data;
using MySqlConnector;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class SkillRepository
{
    private readonly DBHelper _db;

    public SkillRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<List<Skill>> GetAllMasterSkillsAsync()
    {
        string sql = "SELECT * FROM Skills ORDER BY Category, Name";
        var dt = await _db.ExecuteQueryAsync(sql);
        var list = new List<Skill>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Skill
            {
                Id = Convert.ToInt32(r["Id"]),
                Name = r["Name"].ToString()!,
                Category = r["Category"].ToString()!,
                Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString()
            });
        }
        return list;
    }

    public async Task<List<StudentSkill>> GetStudentSkillsAsync(int studentId)
    {
        string sql = @"SELECT ss.*, s.Name as SkillName, s.Category as SkillCategory 
                       FROM StudentSkills ss 
                       JOIN Skills s ON ss.SkillId = s.Id 
                       WHERE ss.StudentId = @StudentId 
                       ORDER BY s.Category, s.Name";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@StudentId", studentId));
        var list = new List<StudentSkill>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(MapStudentSkill(r));
        }
        return list;
    }

    public async Task AddOrUpdateStudentSkillAsync(int studentId, int skillId, int confidencePercentage)
    {
        string sql = @"INSERT INTO StudentSkills (StudentId, SkillId, ConfidencePercentage, VerificationStatus) 
                       VALUES (@StudentId, @SkillId, @Confidence, 'UNVERIFIED')
                       ON DUPLICATE KEY UPDATE ConfidencePercentage = @Confidence;";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@SkillId", skillId),
            new MySqlParameter("@Confidence", confidencePercentage));
    }

    public async Task DeleteStudentSkillAsync(int studentId, int skillId)
    {
        string sql = "DELETE FROM StudentSkills WHERE StudentId = @StudentId AND SkillId = @SkillId";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@SkillId", skillId));
    }

    public async Task<List<CareerGoal>> GetAllCareerGoalsAsync()
    {
        string sql = "SELECT * FROM CareerGoals ORDER BY Title";
        var dt = await _db.ExecuteQueryAsync(sql);
        var list = new List<CareerGoal>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new CareerGoal
            {
                Id = Convert.ToInt32(r["Id"]),
                Title = r["Title"].ToString()!,
                Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString()
            });
        }
        return list;
    }

    public async Task<SkillGapAnalysisResult> GetSkillGapAnalysisAsync(int studentId, int careerGoalId)
    {
        // 1. Get required skills for career goal
        string sqlReq = @"SELECT s.Id, s.Name, s.Category 
                          FROM CareerSkills cs 
                          JOIN Skills s ON cs.SkillId = s.Id 
                          WHERE cs.CareerGoalId = @CareerGoalId";
        var dtReq = await _db.ExecuteQueryAsync(sqlReq, new MySqlParameter("@CareerGoalId", careerGoalId));
        var requiredSkills = new List<Skill>();
        foreach (DataRow r in dtReq.Rows)
        {
            requiredSkills.Add(new Skill { Id = Convert.ToInt32(r["Id"]), Name = r["Name"].ToString()!, Category = r["Category"].ToString()! });
        }

        // 2. Get student skills
        var studentSkills = await GetStudentSkillsAsync(studentId);
        var studentSkillMap = studentSkills.ToDictionary(ss => ss.SkillId);

        var result = new SkillGapAnalysisResult();
        int totalRequired = requiredSkills.Count;
        int matchedCount = 0;

        foreach (var req in requiredSkills)
        {
            if (studentSkillMap.TryGetValue(req.Id, out var studentSkill))
            {
                matchedCount++;
                result.HeldSkills.Add(new SkillGapItem
                {
                    SkillId = req.Id,
                    SkillName = req.Name,
                    Category = req.Category,
                    IsRequired = true,
                    StudentHasIt = true,
                    ConfidencePercentage = studentSkill.ConfidencePercentage,
                    VerificationStatus = studentSkill.VerificationStatus
                });
            }
            else
            {
                result.MissingSkills.Add(new SkillGapItem
                {
                    SkillId = req.Id,
                    SkillName = req.Name,
                    Category = req.Category,
                    IsRequired = true,
                    StudentHasIt = false,
                    ConfidencePercentage = 0,
                    VerificationStatus = "UNVERIFIED"
                });
            }
        }

        result.OverallMatchPercentage = totalRequired == 0 ? 100 : (int)Math.Round(((double)matchedCount / totalRequired) * 100.0);
        return result;
    }

    public async Task VerifySkillEvidenceAsync(int studentId, int skillId, string evidenceType)
    {
        string colName = evidenceType switch
        {
            "Certificate" => "HasCertificateEvidence",
            "Project" => "HasProjectEvidence",
            "Assessment" => "HasAssessmentEvidence",
            "GitHub" => "HasGitHubEvidence",
            "Faculty" => "HasFacultyVerification",
            _ => ""
        };

        if (string.IsNullOrEmpty(colName)) return;

        string sql = $@"UPDATE StudentSkills 
                       SET {colName} = TRUE,
                           VerificationStatus = IF(HasCertificateEvidence OR HasFacultyVerification OR HasProjectEvidence, 'VERIFIED', 'UNVERIFIED')
                       WHERE StudentId = @StudentId AND SkillId = @SkillId;";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@SkillId", skillId));
    }

    private static StudentSkill MapStudentSkill(DataRow r)
    {
        return new StudentSkill
        {
            Id = Convert.ToInt32(r["Id"]),
            StudentId = Convert.ToInt32(r["StudentId"]),
            SkillId = Convert.ToInt32(r["SkillId"]),
            ConfidencePercentage = Convert.ToInt32(r["ConfidencePercentage"]),
            HasProjectEvidence = Convert.ToBoolean(r["HasProjectEvidence"]),
            HasCertificateEvidence = Convert.ToBoolean(r["HasCertificateEvidence"]),
            HasAssessmentEvidence = Convert.ToBoolean(r["HasAssessmentEvidence"]),
            HasGitHubEvidence = Convert.ToBoolean(r["HasGitHubEvidence"]),
            HasFacultyVerification = Convert.ToBoolean(r["HasFacultyVerification"]),
            VerificationStatus = r["VerificationStatus"].ToString()!,
            SkillName = r["SkillName"].ToString()!,
            SkillCategory = r["SkillCategory"].ToString()!
        };
    }
}

public class SkillGapAnalysisResult
{
    public int OverallMatchPercentage { get; set; }
    public List<SkillGapItem> HeldSkills { get; set; } = new();
    public List<SkillGapItem> MissingSkills { get; set; } = new();
}

public class SkillGapItem
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool StudentHasIt { get; set; }
    public int ConfidencePercentage { get; set; }
    public string VerificationStatus { get; set; } = "UNVERIFIED";
}
