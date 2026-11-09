namespace SkillBridge.Common;

public static class MatchPercentageCalculator
{
    /// <summary>
    /// Computes skill match percentage between required skills and student held skills.
    /// </summary>
    public static int Calculate(IEnumerable<int> requiredSkillIds, IEnumerable<int> studentSkillIds)
    {
        var requiredList = requiredSkillIds.Distinct().ToList();
        if (requiredList.Count == 0) return 100;

        var studentSet = new HashSet<int>(studentSkillIds);
        int matchedCount = requiredList.Count(reqId => studentSet.Contains(reqId));

        double percentage = ((double)matchedCount / requiredList.Count) * 100.0;
        return (int)Math.Round(percentage);
    }
}
