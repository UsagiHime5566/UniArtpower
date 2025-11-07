#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace HimeLib
{
    class AutoIncreaseBundle : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        
        public void OnPreprocessBuild(BuildReport report)
        {
            BuildTarget buildTarget = report.summary.platform;
            
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    IncreaseAndroidVersion();
                    break;
                    
                case BuildTarget.iOS:
                    IncreaseIOSVersion();
                    break;
                    
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    IncreaseWindowsVersion();
                    break;
                    
                default:
                    Debug.Log($"AutoIncreaseBundle: 不支援的平台 {buildTarget}，跳過版本號增加");
                    break;
            }
        }
        
        void IncreaseAndroidVersion()
        {
            int currentVersion = PlayerSettings.Android.bundleVersionCode;
            int newVersion = currentVersion + 1;
            PlayerSettings.Android.bundleVersionCode = newVersion;
            Debug.Log($"[AutoIncreaseBundle] Android bundleVersionCode: {currentVersion} -> {newVersion}");
        }
        
        void IncreaseIOSVersion()
        {
            string currentBuildNumber = PlayerSettings.iOS.buildNumber;
            int currentVersion = string.IsNullOrEmpty(currentBuildNumber) ? 0 : System.Convert.ToInt32(currentBuildNumber);
            int newVersion = currentVersion + 1;
            PlayerSettings.iOS.buildNumber = newVersion.ToString();
            Debug.Log($"[AutoIncreaseBundle] iOS buildNumber: {currentVersion} -> {newVersion}");
        }
        
        void IncreaseWindowsVersion()
        {
            // Windows 使用 bundleVersion (格式: major.minor.build)
            string currentVersion = PlayerSettings.bundleVersion;
            string[] versionParts = currentVersion.Split('.');
            
            int major = versionParts.Length > 0 ? int.Parse(versionParts[0]) : 0;
            int minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
            int build = versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0;
            
            // 增加 build 版本號
            build++;
            
            string newVersion = $"{major}.{minor}.{build}";
            PlayerSettings.bundleVersion = newVersion;
            Debug.Log($"[AutoIncreaseBundle] Windows bundleVersion: {currentVersion} -> {newVersion}");
        }
    }
}
#endif
