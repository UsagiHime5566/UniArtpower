#if UNITY_EDITOR && !UNITY_STANDALONE_WIN
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
class AutoIncreaseBundle : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Auto Increase android bundleVersionCode...");
        int androidBV = PlayerSettings.Android.bundleVersionCode;
        int iosBV = System.Convert.ToInt32(PlayerSettings.iOS.buildNumber);

        int BV = Mathf.Max(androidBV, iosBV) + 1;
        PlayerSettings.Android.bundleVersionCode = BV;
        PlayerSettings.iOS.buildNumber = BV.ToString();
    }
}
#endif
