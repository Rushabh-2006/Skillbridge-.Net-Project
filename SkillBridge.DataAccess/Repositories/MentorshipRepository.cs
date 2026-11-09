using System.Data;
using MySqlConnector;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class MentorshipRepository
{
    private readonly DBHelper _db;

    public MentorshipRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<int> RequestMentorshipAsync(int studentId, string topic, string? initialNotes)
    {
        string sql = @"INSERT INTO Mentorships (StudentId, Topic, Status, Notes, RequestedAt)
                       VALUES (@StudentId, @Topic, 'Requested', @Notes, NOW());
                       SELECT LAST_INSERT_ID();";

        var idObj = await _db.ExecuteScalarAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@Topic", topic),
            new MySqlParameter("@Notes", (object?)initialNotes ?? DBNull.Value));

        return Convert.ToInt32(idObj);
    }

    public async Task<List<Mentorship>> GetStudentMentorshipsAsync(int studentId)
    {
        string sql = @"SELECT m.*, u.Name as StudentName, fu.Name as FacultyName
                       FROM Mentorships m
                       JOIN StudentProfiles sp ON m.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       LEFT JOIN Users fu ON m.FacultyId = fu.Id
                       WHERE m.StudentId = @StudentId
                       ORDER BY m.RequestedAt DESC";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@StudentId", studentId));
        return MapList(dt);
    }

    public async Task<List<Mentorship>> GetAllMentorshipRequestsAsync()
    {
        string sql = @"SELECT m.*, u.Name as StudentName, fu.Name as FacultyName
                       FROM Mentorships m
                       JOIN StudentProfiles sp ON m.StudentId = sp.Id
                       JOIN Users u ON sp.UserId = u.Id
                       LEFT JOIN Users fu ON m.FacultyId = fu.Id
                       ORDER BY m.RequestedAt DESC";
        var dt = await _db.ExecuteQueryAsync(sql);
        return MapList(dt);
    }

    public async Task AcceptOrUpdateMentorshipAsync(int mentorshipId, int facultyId, string status, string? updatedNotes)
    {
        string sql = @"UPDATE Mentorships
                       SET FacultyId = @FacultyId, Status = @Status, Notes = CONCAT_WS('\n', Notes, @Notes)
                       WHERE Id = @Id;";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@FacultyId", facultyId),
            new MySqlParameter("@Status", status),
            new MySqlParameter("@Notes", (object?)updatedNotes ?? DBNull.Value),
            new MySqlParameter("@Id", mentorshipId));
    }

    private static List<Mentorship> MapList(DataTable dt)
    {
        var list = new List<Mentorship>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Mentorship
            {
                Id = Convert.ToInt32(r["Id"]),
                StudentId = Convert.ToInt32(r["StudentId"]),
                FacultyId = r["FacultyId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["FacultyId"]),
                Topic = r["Topic"].ToString()!,
                Status = r["Status"].ToString()!,
                Notes = r["Notes"] == DBNull.Value ? null : r["Notes"].ToString(),
                RequestedAt = Convert.ToDateTime(r["RequestedAt"]),
                StudentName = r.Table.Columns.Contains("StudentName") ? r["StudentName"].ToString()! : "",
                FacultyName = r.Table.Columns.Contains("FacultyName") && r["FacultyName"] != DBNull.Value ? r["FacultyName"].ToString() : null
            });
        }
        return list;
    }
}
