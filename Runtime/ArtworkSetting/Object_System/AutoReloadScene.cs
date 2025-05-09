using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoReloadScene : MonoBehaviour
{
    [Header("Monitor 可以為 null")]
    public MemoryMonitor memoryMonitor;
    public FpsMonitor fpsMonitor;

    [Header("Set Reload Condition Action")]
    public long ReloadWhenMB = 1024;
    public double ReloadWhenFPS = 40;
    public ReloadOption reloadOption;
    public float AffectAfterSecond = 10;

    [Header("Set Restart Time")]
    public bool RestartAppEveryday = false;
    public int restartHour = 0; // 設置重啟小時
    public int restartMinute = 0; // 設置重啟分鐘
    public int restartSecond = 0; // 設置重啟秒鐘

    [Header("Set Force Restart in Hangs Time")]
    public float HangsMaxTime = 5;
    public Thread HangsMonitor;
    CancellationTokenSource cts = new CancellationTokenSource();

    [Header("Debug Params 僅動態顯示 非參數")]
    [SerializeField] bool isReloading = false;
    [SerializeField] float lastLiveTime = 7;
    [SerializeField] float timeSinceStartup = 0;

    void Start(){
        HangsMonitor = new Thread(() => ThreadLoop(cts.Token));
        HangsMonitor.Start();

        StartCoroutine(SlowLoop());
    }

    void ThreadLoop(CancellationToken token){
        while (true)
        {
            if(lastLiveTime < HangsMaxTime - 1){
                if(memoryMonitor)
                    Debug.Log("Watchdog Loop / " + System.DateTime.Now + " / " + memoryMonitor.GetMemoryUsageString() + $" / tick:{lastLiveTime}");
                else 
                    Debug.Log("Watchdog Loop / " + System.DateTime.Now + " / " + "No Memory Infomation" + $" / tick:{lastLiveTime}");
            }
            Thread.Sleep(100);
            lastLiveTime -= 0.1f;

            if(lastLiveTime < 0){
                Debug.Log("Watchdog Loop / " + System.DateTime.Now + " / Restart APP !!!");
                RestartAppDotNet();
                break;
            }

            if (token.IsCancellationRequested)
                break;
        }
    }

    void OnDestroy() {
        //memMonitor.Abort();
        cts?.Cancel();
        cts?.Dispose();
    }

    public void ReloadScene()
    {
        Debug.Log("Critical / " + System.DateTime.Now + " / Reload Scene !!!");
        // 獲取目前的場景名稱
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 使用場景名稱重新載入場景
        SceneManager.LoadScene(currentSceneName);
    }

    public async void RestartApp()
    {
        Debug.Log("Critical / " + System.DateTime.Now + " / Restart APP !!!");
        GenerateRestartSequence();
        await Task.Delay(1000);
        Application.Quit();
    }

    public async void RestartAppDotNet(){
        GenerateRestartSequence();
        await Task.Delay(1000);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    void Update(){
        //凍結方案
        lastLiveTime = HangsMaxTime;
        timeSinceStartup += Time.deltaTime;

        //記憶體釋放方案
    #if !UNITY_EDITOR
        if(memoryMonitor && memoryMonitor.GetMemoryUsageMB() > ReloadWhenMB && timeSinceStartup > AffectAfterSecond && !isReloading){
            isReloading = true;

            if(reloadOption == ReloadOption.ReloadScene){
                ReloadScene();
            }
            if(reloadOption == ReloadOption.RestartApp){
                RestartApp();
            }
        }
    #endif

        //貞數過慢時重新, 該處數字越大表示越卡頓, 數值為每幀的微秒數
        if(fpsMonitor != null){
            if(fpsMonitor.GetFPS() > ReloadWhenFPS && timeSinceStartup > AffectAfterSecond && !isReloading){
                isReloading = true;

                if(reloadOption == ReloadOption.ReloadScene){
                    ReloadScene();
                }
                if(reloadOption == ReloadOption.RestartApp){
                    RestartApp();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Home) && Input.GetKey(KeyCode.LeftAlt)){
            ReloadScene();
        }

        if(Input.GetKeyDown(KeyCode.End) && Input.GetKey(KeyCode.LeftAlt)){
            RestartApp();
        }
    }

    IEnumerator SlowLoop(){
        WaitForSeconds wait = new WaitForSeconds(1);
        while(RestartAppEveryday){
            yield return wait;

            System.DateTime now = System.DateTime.Now;

            // 如果當前時間達到設置的重啟時間，且重啟操作尚未進行
            if (now.Hour == restartHour && now.Minute == restartMinute && now.Second == restartSecond && !isReloading)
            {
                isReloading = true;
                Debug.Log("SlowLoop / " + System.DateTime.Now + " / Schedule Restart APP !!!");
                RestartApp();
            }
        }
    }

    void GenerateRestartSequence()
    {
#if !UNITY_EDITOR
            string exePath = System.IO.Path.GetDirectoryName(Application.dataPath);
            string batName = exePath + "/" + "temp.bat";
            var file = System.IO.File.Open(batName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            var writer = new System.IO.StreamWriter(file);
            writer.WriteLine("@echo off");
            writer.WriteLine("echo !!!");
            writer.WriteLine("echo Wait for system prepare...");
            writer.WriteLine("ping 127.0.0.1 -n 10 -w 1000");
            writer.WriteLine("cd /D " + exePath);
            //writer.WriteLine("shutdown -r -t 1");
            //用start 無法覆蓋最上層畫面
            writer.WriteLine(Application.productName + ".exe");
            writer.Flush();
            file.Close();
            System.Diagnostics.Process.Start("temp.bat");
#endif
    }

    public enum ReloadOption
    {
        ReloadScene = 0,
        RestartApp = 1,
    }
}
