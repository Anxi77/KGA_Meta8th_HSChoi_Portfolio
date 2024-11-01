using UnityEngine;

public class SkillDataManager : DataManager
{
    private const int CURRENT_DATA_VERSION = 2;
    private const string VERSION_KEY = "SkillDataVersion";

    private void MigrateDataIfNeeded()
    {
        int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 1);
        if (savedVersion < CURRENT_DATA_VERSION)
        {
            try
            {
                MigrateData(savedVersion);
                PlayerPrefs.SetInt(VERSION_KEY, CURRENT_DATA_VERSION);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Data migration failed: {e.Message}");
                BackupAndResetData();
            }
        }
    }

    private void MigrateData(int fromVersion)
    {
        switch (fromVersion)
        {
            case 1:
                MigrateFromVersion1To2();
                break;
                // 추가 버전 마이그레이션...
        }
    }

    private void MigrateFromVersion1To2()
    {
        foreach (var skillData in skillDatas)
        {
            if (skillData.GetStatsForLevel(1) is ProjectileSkillStat projectileStats)
            {
                projectileStats.persistenceData = new ProjectilePersistenceData
                {
                    isPersistent = false,
                    duration = 0f,
                    effectInterval = 0.5f
                };
            }
            else if (skillData.GetStatsForLevel(1) is AreaSkillStat areaStats)
            {
                areaStats.persistenceData = new AreaPersistenceData
                {
                    isPersistent = true,
                    duration = 0f,
                    effectInterval = 0f
                };
            }
        }
    }

    private void BackupAndResetData()
    {
        BackupCSVFiles();
        ClearAllData();
        LoadAllData();
    }
}