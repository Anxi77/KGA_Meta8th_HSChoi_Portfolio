using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class AutoBackupManager
{
    private const string BACKUP_PATH = "Backup";
    private const int MAX_BACKUPS = 5;
    private const float BACKUP_INTERVAL = 300f; // 5분

    private static string BackupDirectory =>
        Path.Combine(Application.dataPath, "Resources", SkillDataManager.RESOURCE_PATH, BACKUP_PATH);

    public static void Initialize()
    {
        Directory.CreateDirectory(BackupDirectory);
        EditorApplication.update += CheckAutoBackup;
    }

    private static float lastBackupTime;

    private static void CheckAutoBackup()
    {
        if (EditorApplication.timeSinceStartup - lastBackupTime >= BACKUP_INTERVAL)
        {
            CreateBackup();
            lastBackupTime = (float)EditorApplication.timeSinceStartup;
            CleanOldBackups();
        }
    }

    public static void CreateBackup()
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFolder = Path.Combine(BackupDirectory, timestamp);
            Directory.CreateDirectory(backupFolder);

            // CSV 파일 백업
            foreach (var file in Directory.GetFiles(Path.Combine(Application.dataPath, "Resources", SkillDataManager.RESOURCE_PATH), "*.csv"))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(backupFolder, fileName));
            }

            // JSON 파일 백업
            string jsonPath = Path.Combine(Application.dataPath, "Resources", SkillDataManager.RESOURCE_PATH, "SkillData.json");
            if (File.Exists(jsonPath))
            {
                File.Copy(jsonPath, Path.Combine(backupFolder, "SkillData.json"));
            }

            Debug.Log($"Backup created at: {backupFolder}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Backup failed: {e.Message}");
        }
    }

    private static void CleanOldBackups()
    {
        var backupFolders = Directory.GetDirectories(BackupDirectory)
            .OrderByDescending(d => d)
            .Skip(MAX_BACKUPS);

        foreach (var folder in backupFolders)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete old backup {folder}: {e.Message}");
            }
        }
    }
}