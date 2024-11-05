using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class BackupManager
{
    private const string BACKUP_PATH = "Backups";
    private const int MAX_BACKUPS = 5;

    public void CreateBackup(string sourcePath)
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(Application.dataPath, BACKUP_PATH, timestamp);

            // 백업 디렉토리 생성
            Directory.CreateDirectory(backupPath);

            // 모든 데이터 파일 복사
            CopyDirectory(sourcePath, backupPath);

            // 오래된 백업 정리
            CleanupOldBackups();

            Debug.Log($"Backup created successfully: {backupPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Backup failed: {e.Message}");
        }
    }

    private void CopyDirectory(string source, string target)
    {
        foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(source, target));
        }

        foreach (string filePath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(filePath, filePath.Replace(source, target), true);
        }
    }

    private void CleanupOldBackups()
    {
        string backupRoot = Path.Combine(Application.dataPath, BACKUP_PATH);
        var backups = Directory.GetDirectories(backupRoot)
            .OrderByDescending(d => d)
            .Skip(MAX_BACKUPS);

        foreach (var oldBackup in backups)
        {
            Directory.Delete(oldBackup, true);
        }
    }

    public bool RestoreFromBackup(string backupTimestamp)
    {
        try
        {
            string backupPath = Path.Combine(Application.dataPath, BACKUP_PATH, backupTimestamp);
            string resourcePath = Path.Combine(Application.dataPath, "Resources");

            if (!Directory.Exists(backupPath))
            {
                Debug.LogError($"Backup not found: {backupPath}");
                return false;
            }

            // 현재 데이터 백업
            CreateBackup(resourcePath);

            // 백업에서 복원
            CopyDirectory(backupPath, resourcePath);
            AssetDatabase.Refresh();

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Restore failed: {e.Message}");
            return false;
        }
    }
}