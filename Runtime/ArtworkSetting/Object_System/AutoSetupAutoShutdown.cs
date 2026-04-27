using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 在程式啟動時，於 exe 旁邊產出一支懶人工具 bat：
///   SetupAutoShutdown.bat - 雙擊後輸入每日關機時間 (HH:mm, 24h)，
///                           即會在 Windows 工作排程器 (schtasks) 註冊
///                           每日定時觸發 "shutdown.exe /s /f /t 180" 的任務。
///
/// 觸發時 shutdown 會啟動 180 秒倒數，期間使用者仍可執行 "shutdown /a" 取消。
///
/// 權限注意事項：
///   schtasks /Create 在「目前使用者」名下建立任務時不需要管理員權限；
///   shutdown /s /f 對一般使用者帳號預設已具備 SeShutdownPrivilege。
///   因此這支 bat 不需要、也不應以系統管理員身分執行
///   ( /RU SYSTEM 會讓任務在使用者未登入時也觸發，但需提權，本工具不採用 )。
/// </summary>
public class AutoSetupAutoShutdown : MonoBehaviour
{
    [Tooltip("是否在程式啟動時自動產生工具 bat (Editor 中會略過)")]
    public bool createOnStart = true;

    [Tooltip("『設定每日自動關機』工具 bat 名稱 (不含副檔名)")]
    public string setupBatName = "SetupAutoShutdown";

    [Tooltip("schtasks 任務名稱前綴 (後接 Application.productName，避免多專案衝突)")]
    public string taskNamePrefix = "AutoShutdown_";

    [Tooltip("shutdown /t 倒數秒數 (預設 180 秒，在排程觸發後給使用者取消的緩衝)")]
    public int shutdownDelaySeconds = 180;

    void Start()
    {
#if !UNITY_EDITOR
        if (createOnStart)
        {
            CreateSetupBat();
        }
#else
        Debug.Log("[AutoSetupAutoShutdown] 在 Unity Editor 中執行，跳過產生工具 bat");
#endif
    }

    /// <summary>
    /// 產生 SetupAutoShutdown.bat。
    /// 內容：詢問每日關機時間，並用 schtasks 註冊每日觸發 shutdown.exe 的任務。
    /// </summary>
    public string CreateSetupBat()
    {
        try
        {
            string exePath = Path.GetDirectoryName(Application.dataPath);
            string setupBatPath = Path.Combine(exePath, StripBatExt(setupBatName) + ".bat");

            string taskName = (taskNamePrefix ?? string.Empty) + Application.productName;
            int delay = shutdownDelaySeconds > 0 ? shutdownDelaySeconds : 180;

            using (var file = File.Open(setupBatPath, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(file, new UTF8Encoding(false)))
            {
                writer.NewLine = "\r\n";
                writer.WriteLine("@echo off");
                writer.WriteLine("setlocal");
                writer.WriteLine("title Setup " + Application.productName + " AutoShutdown");
                writer.WriteLine();
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine("REM  " + Application.productName + " - Register Daily AutoShutdown");
                writer.WriteLine("REM  Run as the CURRENT USER. Do NOT 'Run as administrator'.");
                writer.WriteLine("REM  schtasks creates a task under the current user, and");
                writer.WriteLine("REM  shutdown.exe needs SeShutdownPrivilege which normal users");
                writer.WriteLine("REM  already have by default.");
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine();
                writer.WriteLine("set \"TASK_NAME=" + taskName + "\"");
                writer.WriteLine("set \"SHUTDOWN_CMD=shutdown.exe /s /f /t " + delay + "\"");
                writer.WriteLine();
                writer.WriteLine("echo ==========================================");
                writer.WriteLine("echo  Setup Daily AutoShutdown");
                writer.WriteLine("echo  Product : " + Application.productName);
                writer.WriteLine("echo  Task    : %TASK_NAME%");
                writer.WriteLine("echo ==========================================");
                writer.WriteLine("echo This will register a Windows scheduled task that");
                writer.WriteLine("echo runs every day at the time you specify, then triggers:");
                writer.WriteLine("echo   %SHUTDOWN_CMD%");
                writer.WriteLine("echo (" + delay + "-second countdown; cancel via \"shutdown /a\").");
                writer.WriteLine("echo.");
                writer.WriteLine();
                writer.WriteLine(":ASK_TIME");
                writer.WriteLine("set \"USER_TIME=\"");
                writer.WriteLine("set /p \"USER_TIME=Enter daily shutdown time (HH:mm, 24h, e.g. 23:00): \"");
                writer.WriteLine("if \"%USER_TIME%\"==\"\" goto ASK_TIME");
                writer.WriteLine();
                writer.WriteLine("echo %USER_TIME%| findstr /R \"^[0-2][0-9]:[0-5][0-9]$\" >nul");
                writer.WriteLine("if errorlevel 1 (");
                writer.WriteLine("    echo [ERROR] Invalid format. Please use HH:mm, e.g. 09:30 or 23:00.");
                writer.WriteLine("    echo.");
                writer.WriteLine("    goto ASK_TIME");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("set \"HH=%USER_TIME:~0,2%\"");
                writer.WriteLine("if 1%HH% GTR 123 (");
                writer.WriteLine("    echo [ERROR] Hour must be 00-23.");
                writer.WriteLine("    echo.");
                writer.WriteLine("    goto ASK_TIME");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("echo.");
                writer.WriteLine("echo Time accepted : %USER_TIME%");
                writer.WriteLine("echo Task name     : %TASK_NAME%");
                writer.WriteLine("echo.");
                writer.WriteLine();
                writer.WriteLine("schtasks /Query /TN \"%TASK_NAME%\" >nul 2>&1");
                writer.WriteLine("if not errorlevel 1 (");
                writer.WriteLine("    echo Existing task found, replacing...");
                writer.WriteLine("    schtasks /Delete /TN \"%TASK_NAME%\" /F >nul");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("schtasks /Create /SC DAILY /TN \"%TASK_NAME%\" /TR \"%SHUTDOWN_CMD%\" /ST %USER_TIME% /F");
                writer.WriteLine("if errorlevel 1 (");
                writer.WriteLine("    echo [ERROR] Failed to register the scheduled task.");
                writer.WriteLine("    echo - Possibly blocked by Group Policy or anti-virus.");
                writer.WriteLine("    echo - Make sure you are NOT running this as administrator.");
                writer.WriteLine("    pause");
                writer.WriteLine("    exit /b 1");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("echo.");
                writer.WriteLine("echo [OK] Daily auto-shutdown scheduled at %USER_TIME%.");
                writer.WriteLine("echo To remove later, run this command in cmd:");
                writer.WriteLine("echo   schtasks /Delete /TN \"%TASK_NAME%\" /F");
                writer.WriteLine("echo.");
                writer.WriteLine("pause");
                writer.WriteLine("endlocal");
                writer.WriteLine("exit /b 0");
            }

            Debug.Log($"[AutoSetupAutoShutdown] 已產生工具 bat: {setupBatPath}");
            return setupBatPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AutoSetupAutoShutdown] 產生 Setup bat 失敗: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    static string StripBatExt(string name)
    {
        if (string.IsNullOrEmpty(name)) return "SetupAutoShutdown";
        if (name.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 4);
        return name;
    }
}
