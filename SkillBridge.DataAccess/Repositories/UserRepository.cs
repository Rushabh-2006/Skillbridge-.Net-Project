using System.Data;
using MySqlConnector;
using SkillBridge.Common;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class UserRepository
{
    private readonly DBHelper _db;

    public UserRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        string sql = "SELECT * FROM Users WHERE Email = @Email LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@Email", email));
        if (dt.Rows.Count == 0) return null;

        var row = dt.Rows[0];
        return MapUser(row);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        string sql = "SELECT * FROM Users WHERE Id = @Id LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@Id", id));
        if (dt.Rows.Count == 0) return null;

        return MapUser(dt.Rows[0]);
    }

    public async Task<int> RegisterAsync(string name, string email, string password, string role, string? phoneNumber)
    {
        string hash = PasswordHasher.HashPassword(password);
        string sqlUser = @"INSERT INTO Users (Name, Email, PasswordHash, Role, PhoneNumber, CreatedAt) 
                           VALUES (@Name, @Email, @Hash, @Role, @Phone, NOW()); 
                           SELECT LAST_INSERT_ID();";

        var userIdObj = await _db.ExecuteScalarAsync(sqlUser,
            new MySqlParameter("@Name", name),
            new MySqlParameter("@Email", email),
            new MySqlParameter("@Hash", hash),
            new MySqlParameter("@Role", role),
            new MySqlParameter("@Phone", (object?)phoneNumber ?? DBNull.Value));

        int userId = Convert.ToInt32(userIdObj);

        // Initialize verification badges
        string sqlBadge = @"INSERT INTO VerificationBadges (UserId, IsEmailVerified, IsPhoneVerified, IsLinkedInVerified, IsGitHubVerified) 
                            VALUES (@UserId, TRUE, FALSE, FALSE, FALSE);";
        await _db.ExecuteNonQueryAsync(sqlBadge, new MySqlParameter("@UserId", userId));

        // Role specific profile setup
        if (role == RoleConstants.Student)
        {
            string sqlStudent = @"INSERT INTO StudentProfiles (UserId, ReadinessScore) VALUES (@UserId, 0.00);";
            await _db.ExecuteNonQueryAsync(sqlStudent, new MySqlParameter("@UserId", userId));
        }
        else if (role == RoleConstants.Recruiter)
        {
            string sqlCompany = @"INSERT INTO Companies (UserId, CompanyName) VALUES (@UserId, @CompanyName);";
            await _db.ExecuteNonQueryAsync(sqlCompany,
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@CompanyName", name + " Organization"));
        }

        return userId;
    }

    public async Task<VerificationBadge?> GetBadgesAsync(int userId)
    {
        string sql = "SELECT * FROM VerificationBadges WHERE UserId = @UserId LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@UserId", userId));
        if (dt.Rows.Count == 0) return null;

        var row = dt.Rows[0];
        return new VerificationBadge
        {
            Id = Convert.ToInt32(row["Id"]),
            UserId = Convert.ToInt32(row["UserId"]),
            IsEmailVerified = Convert.ToBoolean(row["IsEmailVerified"]),
            IsPhoneVerified = Convert.ToBoolean(row["IsPhoneVerified"]),
            IsLinkedInVerified = Convert.ToBoolean(row["IsLinkedInVerified"]),
            IsGitHubVerified = Convert.ToBoolean(row["IsGitHubVerified"])
        };
    }

    public async Task<StudentProfile?> GetStudentProfileAsync(int userId)
    {
        string sql = @"SELECT sp.*, u.Name as StudentName, u.Email as StudentEmail, cg.Title as TargetCareerTitle 
                       FROM StudentProfiles sp
                       JOIN Users u ON sp.UserId = u.Id
                       LEFT JOIN CareerGoals cg ON sp.TargetCareerGoalId = cg.Id
                       WHERE sp.UserId = @UserId LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@UserId", userId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return MapStudentProfile(r);
    }

    public async Task<StudentProfile?> GetStudentProfileByStudentIdAsync(int studentId)
    {
        string sql = @"SELECT sp.*, u.Name as StudentName, u.Email as StudentEmail, cg.Title as TargetCareerTitle 
                       FROM StudentProfiles sp
                       JOIN Users u ON sp.UserId = u.Id
                       LEFT JOIN CareerGoals cg ON sp.TargetCareerGoalId = cg.Id
                       WHERE sp.Id = @StudentId LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@StudentId", studentId));
        if (dt.Rows.Count == 0) return null;
        return MapStudentProfile(dt.Rows[0]);
    }

    public async Task UpdateStudentProfileAsync(int studentId, string? bio, string? college, string? degree, int? gradYear, int? careerGoalId, string? linkedIn, string? gitHub)
    {
        string sql = @"UPDATE StudentProfiles 
                       SET Bio = @Bio, College = @College, Degree = @Degree, 
                           GraduationYear = @GradYear, TargetCareerGoalId = @CareerGoalId, 
                           LinkedInUrl = @LinkedIn, GitHubUrl = @GitHub 
                       WHERE Id = @StudentId";
        await _db.ExecuteNonQueryAsync(sql,
            new MySqlParameter("@Bio", (object?)bio ?? DBNull.Value),
            new MySqlParameter("@College", (object?)college ?? DBNull.Value),
            new MySqlParameter("@Degree", (object?)degree ?? DBNull.Value),
            new MySqlParameter("@GradYear", (object?)gradYear ?? DBNull.Value),
            new MySqlParameter("@CareerGoalId", (object?)careerGoalId ?? DBNull.Value),
            new MySqlParameter("@LinkedIn", (object?)linkedIn ?? DBNull.Value),
            new MySqlParameter("@GitHub", (object?)gitHub ?? DBNull.Value),
            new MySqlParameter("@StudentId", studentId));
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        string sql = "SELECT * FROM Users ORDER BY Id ASC";
        var dt = await _db.ExecuteQueryAsync(sql);
        var list = new List<User>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(MapUser(r));
        }
        return list;
    }

    public async Task<List<StudentProfile>> GetAllStudentProfilesAsync()
    {
        string sql = @"SELECT sp.*, u.Name as StudentName, u.Email as StudentEmail, cg.Title as TargetCareerTitle 
                       FROM StudentProfiles sp
                       JOIN Users u ON sp.UserId = u.Id
                       LEFT JOIN CareerGoals cg ON sp.TargetCareerGoalId = cg.Id
                       ORDER BY sp.Id ASC";
        var dt = await _db.ExecuteQueryAsync(sql);
        var list = new List<StudentProfile>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(MapStudentProfile(r));
        }
        return list;
    }

    public async Task<Company?> GetCompanyByUserIdAsync(int userId)
    {
        string sql = "SELECT * FROM Companies WHERE UserId = @UserId LIMIT 1";
        var dt = await _db.ExecuteQueryAsync(sql, new MySqlParameter("@UserId", userId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new Company
        {
            Id = Convert.ToInt32(r["Id"]),
            UserId = Convert.ToInt32(r["UserId"]),
            CompanyName = r["CompanyName"].ToString()!,
            Industry = r["Industry"] == DBNull.Value ? null : r["Industry"].ToString(),
            Website = r["Website"] == DBNull.Value ? null : r["Website"].ToString(),
            Location = r["Location"] == DBNull.Value ? null : r["Location"].ToString(),
            Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString()
        };
    }

    private static User MapUser(DataRow r)
    {
        return new User
        {
            Id = Convert.ToInt32(r["Id"]),
            Name = r["Name"].ToString()!,
            Email = r["Email"].ToString()!,
            PasswordHash = r["PasswordHash"].ToString()!,
            Role = r["Role"].ToString()!,
            PhoneNumber = r["PhoneNumber"] == DBNull.Value ? null : r["PhoneNumber"].ToString(),
            CreatedAt = Convert.ToDateTime(r["CreatedAt"])
        };
    }

    private static StudentProfile MapStudentProfile(DataRow r)
    {
        return new StudentProfile
        {
            Id = Convert.ToInt32(r["Id"]),
            UserId = Convert.ToInt32(r["UserId"]),
            Bio = r["Bio"] == DBNull.Value ? null : r["Bio"].ToString(),
            College = r["College"] == DBNull.Value ? null : r["College"].ToString(),
            Degree = r["Degree"] == DBNull.Value ? null : r["Degree"].ToString(),
            GraduationYear = r["GraduationYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["GraduationYear"]),
            TargetCareerGoalId = r["TargetCareerGoalId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["TargetCareerGoalId"]),
            ReadinessScore = Convert.ToDecimal(r["ReadinessScore"]),
            LinkedInUrl = r["LinkedInUrl"] == DBNull.Value ? null : r["LinkedInUrl"].ToString(),
            GitHubUrl = r["GitHubUrl"] == DBNull.Value ? null : r["GitHubUrl"].ToString(),
            StudentName = r["StudentName"].ToString()!,
            StudentEmail = r["StudentEmail"].ToString()!,
            TargetCareerTitle = r["TargetCareerTitle"] == DBNull.Value ? null : r["TargetCareerTitle"].ToString()
        };
    }
}
