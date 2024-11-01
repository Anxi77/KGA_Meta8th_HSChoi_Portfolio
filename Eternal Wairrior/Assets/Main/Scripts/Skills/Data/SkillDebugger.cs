using UnityEngine;
using System.Collections.Generic;

public static class SkillDebugger
{
    private static readonly Dictionary<string, System.Diagnostics.Stopwatch> performanceTrackers
        = new Dictionary<string, System.Diagnostics.Stopwatch>();

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogSkillAction(string skillName, string action)
    {
        Debug.Log($"[Skill] {skillName}: {action}");
    }

    public static void StartTrackingPerformance(string key)
    {
        if (!performanceTrackers.TryGetValue(key, out var tracker))
        {
            tracker = new System.Diagnostics.Stopwatch();
            performanceTrackers[key] = tracker;
        }
        tracker.Start();
    }

    public static void StopTrackingPerformance(string key)
    {
        if (performanceTrackers.TryGetValue(key, out var tracker))
        {
            tracker.Stop();
            Debug.Log($"[Performance] {key}: {tracker.ElapsedMilliseconds}ms");
            tracker.Reset();
        }
    }
}