using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 在程式啟動時，於 exe 旁邊產出兩支工具 bat：
///   1. SetupAutoStart.bat   - 將 AutoGenerateBat 產出的 {ProductName}.bat 複製到目前使用者的 Startup 資料夾 (shell:startup)
///   2. UnsetupAutoStart.bat - 從 Startup 資料夾移除上述 bat
///
/// 權限注意事項：
///   shell:startup 對應的路徑為 "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"，
///   屬於『目前使用者』的個人資料夾，以一般使用者身分即可寫入，
///   反而『不應該』使用系統管理員身分執行 — 否則 %APPDATA% 會解析到 Administrator 帳號，
///   bat 會被放到管理員的 Startup，原使用者登入時不會被觸發。
/// </summary>
public class AutoSetupAutoStart : MonoBehaviour
{
    [Tooltip("是否在程式啟動時自動產生工具 bat (Editor 中會略過)")]
    public bool createOnStart = true;

    [Tooltip("『加入開機啟動』工具 bat 名稱 (不含副檔名)")]
    public string setupBatName = "SetupAutoStart";

    [Tooltip("是否同時產出『解除開機啟動』工具 bat")]
    public bool createUnsetupBat = true;

    [Tooltip("『解除開機啟動』工具 bat 名稱 (不含副檔名)")]
    public string unsetupBatName = "UnsetupAutoStart";

    void Start()
    {
#if !UNITY_EDITOR
        if (createOnStart)
        {
            CreateSetupBat();
            if (createUnsetupBat)
            {
                CreateUnsetupBat();
            }
        }
#else
        Debug.Log("[AutoSetupAutoStart] 在 Unity Editor 中執行，跳過產生工具 bat");
#endif
    }

    /// <summary>
    /// 產生 SetupAutoStart.bat。
    /// 內容：把與 AutoGenerateBat 對應的 {ProductName}.bat 複製到目前使用者的 Startup 資料夾。
    /// </summary>
    public string CreateSetupBat()
    {
        try
        {
            string exePath = Path.GetDirectoryName(Application.dataPath);
            string sourceBatName = Application.productName + ".bat";
            string setupBatPath = Path.Combine(exePath, StripBatExt(setupBatName) + ".bat");

            using (var file = File.Open(setupBatPath, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(file, new UTF8Encoding(false)))
            {
                writer.NewLine = "\r\n";
                writer.WriteLine("@echo off");
                writer.WriteLine("setlocal");
                writer.WriteLine("title Setup " + Application.productName + " AutoStart");
                writer.WriteLine();
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine("REM  " + Application.productName + " - Register on Windows Startup");
                writer.WriteLine("REM  Run as the CURRENT USER. Do NOT 'Run as administrator',");
                writer.WriteLine("REM  otherwise %APPDATA% resolves to the admin account and");
                writer.WriteLine("REM  the bat will land in the wrong Startup folder.");
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine();
                writer.WriteLine("set \"SRC_DIR=%~dp0\"");
                writer.WriteLine("set \"SRC_BAT=%SRC_DIR%" + sourceBatName + "\"");
                writer.WriteLine("set \"DEST_DIR=%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"");
                writer.WriteLine("set \"DEST_BAT=%DEST_DIR%\\" + sourceBatName + "\"");
                writer.WriteLine();
                writer.WriteLine("echo ===========================================");
                writer.WriteLine("echo  Setup " + Application.productName + " AutoStart");
                writer.WriteLine("echo ===========================================");
                writer.WriteLine("echo Source : %SRC_BAT%");
                writer.WriteLine("echo Target : %DEST_BAT%");
                writer.WriteLine("echo.");
                writer.WriteLine();
                writer.WriteLine("if not exist \"%SRC_BAT%\" (");
                writer.WriteLine("    echo [ERROR] Source bat not found.");
                writer.WriteLine("    echo Please launch " + Application.productName + ".exe at least once so that");
                writer.WriteLine("    echo AutoGenerateBat can produce \"" + sourceBatName + "\" first.");
                writer.WriteLine("    pause");
                writer.WriteLine("    exit /b 1");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("if not exist \"%DEST_DIR%\" (");
                writer.WriteLine("    echo Startup folder missing, creating...");
                writer.WriteLine("    mkdir \"%DEST_DIR%\" 2>nul");
                writer.WriteLine("    if errorlevel 1 (");
                writer.WriteLine("        echo [ERROR] Failed to create Startup folder.");
                writer.WriteLine("        echo Possibly blocked by Group Policy or anti-virus.");
                writer.WriteLine("        pause");
                writer.WriteLine("        exit /b 1");
                writer.WriteLine("    )");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("copy /Y \"%SRC_BAT%\" \"%DEST_BAT%\" >nul");
                writer.WriteLine("if errorlevel 1 (");
                writer.WriteLine("    echo [ERROR] Copy failed.");
                writer.WriteLine("    echo - The destination may be protected by your IT policy or anti-virus.");
                writer.WriteLine("    echo - The bat in the Startup folder may be in use.");
                writer.WriteLine("    echo - Make sure you are NOT running this as administrator.");
                writer.WriteLine("    pause");
                writer.WriteLine("    exit /b 1");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("echo [OK] " + Application.productName + " will now launch on Windows startup.");
                writer.WriteLine("echo To remove it, run \"" + StripBatExt(unsetupBatName) + ".bat\" next to this file,");
                writer.WriteLine("echo or simply delete the bat in:");
                writer.WriteLine("echo   %DEST_DIR%");
                writer.WriteLine("pause");
                writer.WriteLine("endlocal");
                writer.WriteLine("exit /b 0");
            }

            Debug.Log($"[AutoSetupAutoStart] 已產生安裝工具 bat: {setupBatPath}");
            return setupBatPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AutoSetupAutoStart] 產生 Setup bat 失敗: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 產生 UnsetupAutoStart.bat。
    /// 內容：移除 Startup 資料夾中對應的 {ProductName}.bat。
    /// </summary>
    public string CreateUnsetupBat()
    {
        try
        {
            string exePath = Path.GetDirectoryName(Application.dataPath);
            string sourceBatName = Application.productName + ".bat";
            string unsetupBatPath = Path.Combine(exePath, StripBatExt(unsetupBatName) + ".bat");

            using (var file = File.Open(unsetupBatPath, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(file, new UTF8Encoding(false)))
            {
                writer.NewLine = "\r\n";
                writer.WriteLine("@echo off");
                writer.WriteLine("setlocal");
                writer.WriteLine("title Unsetup " + Application.productName + " AutoStart");
                writer.WriteLine();
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine("REM  " + Application.productName + " - Remove from Windows Startup");
                writer.WriteLine("REM  Run as the CURRENT USER (not administrator).");
                writer.WriteLine("REM ===========================================================");
                writer.WriteLine();
                writer.WriteLine("set \"DEST_DIR=%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"");
                writer.WriteLine("set \"DEST_BAT=%DEST_DIR%\\" + sourceBatName + "\"");
                writer.WriteLine();
                writer.WriteLine("echo Target : %DEST_BAT%");
                writer.WriteLine("echo.");
                writer.WriteLine();
                writer.WriteLine("if not exist \"%DEST_BAT%\" (");
                writer.WriteLine("    echo Nothing to remove. " + Application.productName + " is not registered.");
                writer.WriteLine("    pause");
                writer.WriteLine("    exit /b 0");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("del /F /Q \"%DEST_BAT%\"");
                writer.WriteLine("if errorlevel 1 (");
                writer.WriteLine("    echo [ERROR] Failed to delete the registered bat.");
                writer.WriteLine("    echo - The file may be in use.");
                writer.WriteLine("    echo - It may belong to another user account if you ran setup as admin.");
                writer.WriteLine("    pause");
                writer.WriteLine("    exit /b 1");
                writer.WriteLine(")");
                writer.WriteLine();
                writer.WriteLine("echo [OK] " + Application.productName + " AutoStart removed.");
                writer.WriteLine("pause");
                writer.WriteLine("endlocal");
                writer.WriteLine("exit /b 0");
            }

            Debug.Log($"[AutoSetupAutoStart] 已產生解除工具 bat: {unsetupBatPath}");
            return unsetupBatPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AutoSetupAutoStart] 產生 Unsetup bat 失敗: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    static string StripBatExt(string name)
    {
        if (string.IsNullOrEmpty(name)) return "SetupAutoStart";
        if (name.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 4);
        return name;
    }
}
