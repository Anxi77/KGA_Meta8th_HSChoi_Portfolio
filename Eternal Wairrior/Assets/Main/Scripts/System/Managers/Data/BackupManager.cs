using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class BackupManager
{
    private const string BACKUP_PATH = "Backups";
    private const int MAX_BACKUPS = 5;

    // 백업이 필요한 파일 확장자 지정
    private readonly string[] BACKUP_EXTENSIONS = new string[]
    {
        ".json",    // 게임 세이브 데이터
        ".csv",     // 사용자 정의 데이터
        // 필요한 다른 확장자 추가
    };

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
            // 지정된 확장자의 파일만 백업
            if (BACKUP_EXTENSIONS.Any(ext => filePath.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase)))
            {
                string newPath = filePath.Replace(source, target);
                File.Copy(filePath, newPath, true);

                // meta 파일도 함께 복사
                string metaPath = filePath + ".meta";
                if (File.Exists(metaPath))
                {
                    string newMetaPath = newPath + ".meta";
                    File.Copy(metaPath, newMetaPath, true);
                    UpdateMetaFileGuid(newMetaPath);
                }
            }
        }
    }

    private void UpdateMetaFileGuid(string metaFilePath)
    {
        try
        {
            string content = File.ReadAllText(metaFilePath);

            string pattern = @"guid: \w+";
            string newGuid = System.Guid.NewGuid().ToString("N");
            content = Regex.Replace(content, pattern, $"guid: {newGuid}");

            File.WriteAllText(metaFilePath, content);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"메타 파일 GUID 업데이트 실패: {metaFilePath}, 오류: {e.Message}");
        }
    }

    private void CleanupOldBackups()
    {
        string backupRoot = Path.Combine(Application.dataPath, BACKUP_PATH);

        // 백업 디렉토리들을 날짜순으로 정렬하고 MAX_BACKUPS 이후의 것들을 가져옵니다
        var backups = Directory.GetDirectories(backupRoot)
            .OrderByDescending(d => d)
            .Skip(MAX_BACKUPS);

        foreach (var oldBackup in backups)
        {
            try
            {
                // 디렉토리와 그 안의 모든 파일(.meta 포함) 삭제
                Directory.Delete(oldBackup, true);

                // .meta 파일이 있다면 삭제
                string metaFilePath = oldBackup + ".meta";
                if (File.Exists(metaFilePath))
                {
                    File.Delete(metaFilePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"백업 정리 중 오류 발생: {oldBackup}, 오류: {e.Message}");
            }
        }

        // 변경사항을 Unity에 반영
        AssetDatabase.Refresh();
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