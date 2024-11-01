using UnityEditor;
using UnityEngine;

public class AutoSaveManager
{
    private const float AUTO_SAVE_INTERVAL = 300f; // 5분
    private static float lastSaveTime;

    public static void Initialize()
    {
        EditorApplication.update += CheckAutoSave;
        lastSaveTime = (float)EditorApplication.timeSinceStartup;
    }

    private static void CheckAutoSave()
    {
        if (EditorApplication.timeSinceStartup - lastSaveTime >= AUTO_SAVE_INTERVAL)
        {
            AutoSave();
            lastSaveTime = (float)EditorApplication.timeSinceStartup;
        }
    }

    public static void AutoSave()
    {
        try
        {
            var skillDataManager = Object.FindObjectOfType<SkillDataManager>();
            if (skillDataManager != null)
            {
                skillDataManager.SaveAllSkillData();
                Debug.Log($"Auto-saved skill data at {System.DateTime.Now}");
            }

            // 백업 생성
            AutoBackupManager.CreateBackup();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Auto-save failed: {e.Message}");
        }
    }
}