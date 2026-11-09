using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class CertificationRepository
{
    private readonly DBHelper _db;
    private readonly SkillRepository _skillRepo;

    public CertificationRepository(DBHelper db, SkillRepository skillRepo)
    {
        _db = db;
        _skillRepo = skillRepo;
    }

    public async Task<int> AddCertificationAsync(int studentId, string title, string org, DateTime issueDate, string filePath, int linkedSkillId)
    {
        string rawString = $"{studentId}:{title}:{org}:{issueDate:yyyy-MM-dd}:{DateTime.UtcNow.Ticks}";
        using var sha = SHA256.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawString))).ToLowerInvariant();

        string sql = @"INSERT INTO Certifications (StudentId, Title, IssuingOrganization, IssueDate, FilePath, LinkedSkillId, VerificationStatus, VerificationHash, CreatedAt)
                       VALUES (@StudentId, @Title, @Org, @IssueDate, @FilePath, @LinkedSkillId, 'Pending', @Hash, NOW());
                       SELECT LAST_INSERT_ID();";

        var idObj = await _db.ExecuteScalarAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@Title", title),
            new MySqlParameter("@Org", org),
            new MySqlParameter("@IssueDate", issueDate),
            new MySqlParameter("@FilePath", filePath),
            new MySqlParameter("@LinkedSkillId", linkedSkillId),
            new MySqlParameter("@Hash", hash));

        return Convert.ToInt32(idObj);
    }

    public async Task<List<Certification>> GetStudentCertificationsAsync(int studentId)
    {
        string sql = @"SELECT c.*, u.Name as StudentName, s.Name as LinkedSkillName 
                       FROM Certifications c 
                       JOIN StudentProfiles sp ON c.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       JOIN Skills s ON c.LinkedSkillId = s.Id
                       WHERE c.StudentId = @StudentId
                       ORDER BY c.CreatedAt DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@StudentId", studentId));
        return MapList(dt);
    }

    public async Task<List<Certification>> GetPendingCertificationsAsync()
    {
        string sql = @"SELECT c.*, u.Name as StudentName, s.Name as LinkedSkillName 
                       FROM Certifications c 
                       JOIN StudentProfiles sp ON c.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       JOIN Skills s ON c.LinkedSkillId = s.Id
                       WHERE c.VerificationStatus = 'Pending'
                       ORDER BY c.CreatedAt ASC";
        var dt = await _db.ExecuteQueryAsync(sql);
        return MapList(dt);
    }

    public async Task ReviewCertificationAsync(int certId, int facultyId, string status, string? notes)
    {
        string sql = @"UPDATE Certifications 
                       SET VerificationStatus = @Status, ReviewedByFacultyId = @FacultyId, ReviewNotes = @Notes
                       WHERE Id = @Id;";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@Status", status),
            new MySqlParameter("@FacultyId", facultyId),
            new MySqlParameter("@Notes", (object?)notes ?? DBNull.Value),
            new MySqlParameter("@Id", certId));

        if (status == "Verified")
        {
            var dt = await _db.ExecuteQueryAsync("SELECT StudentId, LinkedSkillId FROM Certifications WHERE Id = @Id", new MySqlParameter("@Id", certId));
            if (dt.Rows.Count > 0)
            {
                int studentId = Convert.ToInt32(dt.Rows[0]["StudentId"]);
                int skillId = Convert.ToInt32(dt.Rows[0]["LinkedSkillId"]);
                await _skillRepo.VerifySkillEvidenceAsync(studentId, skillId, "Certificate");
            }
        }
    }

    private static List<Certification> MapList(DataTable dt)
    {
        var list = new List<Certification>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Certification
            {
                Id = Convert.ToInt32(r["Id"]),
                StudentId = Convert.ToInt32(r["StudentId"]),
                Title = r["Title"].ToString()!,
                IssuingOrganization = r["IssuingOrganization"].ToString()!,
                IssueDate = Convert.ToDateTime(r["IssueDate"]),
                FilePath = r["FilePath"].ToString()!,
                LinkedSkillId = Convert.ToInt32(r["LinkedSkillId"]),
                VerificationStatus = r["VerificationStatus"].ToString()!,
                VerificationHash = r["VerificationHash"] == DBNull.Value ? null : r["VerificationHash"].ToString(),
                ReviewedByFacultyId = r["ReviewedByFacultyId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["ReviewedByFacultyId"]),
                ReviewNotes = r["ReviewNotes"] == DBNull.Value ? null : r["ReviewNotes"].ToString(),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                StudentName = r.Table.Columns.Contains("StudentName") ? r["StudentName"].ToString()! : "",
                LinkedSkillName = r.Table.Columns.Contains("LinkedSkillName") ? r["LinkedSkillName"].ToString()! : ""
            });
        }
        return list;
    }
}
