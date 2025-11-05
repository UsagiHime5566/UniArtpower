using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;

public class AutoBuildAppDataLnk : MonoBehaviour
{
    [Tooltip("是否在啟動時自動創建捷徑")]
    public bool createOnStart = true;

    [Tooltip("捷徑名稱（不含副檔名）")]
    public string shortcutName = "遊戲存檔資料夾";

    void Start()
    {
#if !UNITY_EDITOR
        if (createOnStart)
        {
            CreateShortcutToAppData();
        }
#else
        Debug.Log("在 Unity Editor 中執行，跳過創建捷徑");
#endif
    }

    /// <summary>
    /// 創建指向 AppData 資料夾的捷徑
    /// </summary>
    public void CreateShortcutToAppData()
    {
#if UNITY_EDITOR
        Debug.LogWarning("無法在 Unity Editor 中創建捷徑，請在打包後的程式中執行");
        return;
#endif
        try
        {
            // 獲取目標路徑：AppData\LocalLow\CompanyName\ProductName
            string userName = Environment.UserName;
            string companyName = Application.companyName;
            string productName = Application.productName;
            
            string targetPath = $@"C:\Users\{userName}\AppData\LocalLow\{companyName}\{productName}";
            
            // 確保目標資料夾存在
            if (!Directory.Exists(targetPath))
            {
                Debug.LogWarning($"目標資料夾不存在，將創建: {targetPath}");
                Directory.CreateDirectory(targetPath);
            }

            // 捷徑放置位置：應用程式目錄
            string appDirectory = Path.GetDirectoryName(Application.dataPath + "/../");
            string shortcutPath = Path.Combine(appDirectory, $"{shortcutName}.lnk");

            // 如果捷徑已存在，先刪除
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
                Debug.Log($"已刪除舊的捷徑: {shortcutPath}");
            }

            // 創建捷徑
            CreateShortcut(shortcutPath, targetPath);
            
            Debug.Log($"成功創建捷徑！\n捷徑位置: {shortcutPath}\n目標位置: {targetPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"創建捷徑時發生錯誤: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 使用 IShellLink 創建 Windows 捷徑
    /// </summary>
    private void CreateShortcut(string shortcutPath, string targetPath)
    {
        Type shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"));
        IShellLink shellLink = (IShellLink)Activator.CreateInstance(shellLinkType);

        shellLink.SetPath(targetPath);
        shellLink.SetDescription($"快速開啟 {Application.productName} 的存檔資料夾");
        
        IPersistFile persistFile = (IPersistFile)shellLink;
        persistFile.Save(shortcutPath, false);

        Marshal.ReleaseComObject(persistFile);
        Marshal.ReleaseComObject(shellLink);
    }

    // COM 介面定義
    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}
