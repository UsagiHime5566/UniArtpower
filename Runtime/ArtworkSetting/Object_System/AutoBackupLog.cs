using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AutoBackupLog : MonoBehaviour
{
    void Start()
    {
        BackupLog();
    }

    void BackupLog()
    {
        // 檢查上次執行是否有 player.log 文件存在
        string playerLogPath = Application.persistentDataPath + "/Player-prev.log";
        if (File.Exists(playerLogPath))
        {
            // 備份 player.log 文件
            string backupFolderPath = Application.persistentDataPath + "/Backup";
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }
            string backupFilePath = backupFolderPath + "/player_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
            File.Copy(playerLogPath, backupFilePath, true);

            Debug.Log("Player.log file backed up to: " + backupFilePath);
        }
        else
        {
            Debug.Log("No player.log file found from previous run.");
        }
    }
}
