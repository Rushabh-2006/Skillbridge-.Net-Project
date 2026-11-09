using System.Data;
using MySqlConnector;
using SkillBridge.DataAccess.Entities;

namespace SkillBridge.DataAccess.Repositories;

public class RoadmapRepository
{
    private readonly DBHelper _db;

    public RoadmapRepository(DBHelper db)
    {
        _db = db;
    }

    public async Task<List<RoadmapLevel>> GetRoadmapForStudentAsync(int studentId, int careerGoalId)
    {
        string sql = @"SELECT rl.*, s.Name as SkillName,
                              IF(ss.Id IS NOT NULL, TRUE, FALSE) as IsCompleted
                       FROM RoadmapLevels rl
                       JOIN Skills s ON rl.SkillId = s.Id
                       LEFT JOIN StudentSkills ss ON ss.StudentId = @StudentId AND ss.SkillId = rl.SkillId
                       WHERE rl.CareerGoalId = @CareerGoalId
                       ORDER BY rl.LevelNumber, rl.Id";
        var dt = await _db.ExecuteQueryAsync(sql,
            new MySqlParameter("@StudentId", studentId),
            new MySqlParameter("@CareerGoalId", careerGoalId));

        var list = new List<RoadmapLevel>();
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new RoadmapLevel
            {
                Id = Convert.ToInt32(r["Id"]),
                CareerGoalId = Convert.ToInt32(r["CareerGoalId"]),
                LevelNumber = Convert.ToInt32(r["LevelNumber"]),
                LevelTitle = r["LevelTitle"].ToString()!,
                SkillId = Convert.ToInt32(r["SkillId"]),
                Description = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
                SkillName = r["SkillName"].ToString()!,
                IsCompleted = Convert.ToBoolean(r["IsCompleted"])
            });
        }
        return list;
    }
}
