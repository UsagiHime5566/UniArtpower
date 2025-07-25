using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HimeLib;
using System.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class SystemLayout : SingletonMono<SystemLayout>
{
    [Header("透明按鈕 (用於開啟選單)")] public Button BTN_Option_Open;
    [Header("關閉選單")] public Button BTN_Option_Close;
    [Header("選單內容放置容器")] public CanvasGroup ContentCanvas;
    [Header("需一起隱藏物件")] public List<GameObject> needHides;
    [Header("選單點幾次開啟")] public int openCount = 1;
    [Header("若未隱藏，幾秒後自動隱藏")] public float autoHideTime = 30f;
    [Header("程式執行後幾秒隱藏")] public float autoHideTimeAfterStart = 10f;
    public bool isActive => ContentCanvas.blocksRaycasts;
    
    int currentClickCount = 0;
    float lastClickTime;
    float clickResetTime = 2f; // 2秒内需要完成所有点击
    Coroutine autoHideCoroutine;
    
    async void Start()
    {
        BTN_Option_Open.onClick.AddListener(delegate {
            HandleOpenClick();
        });

        BTN_Option_Close.onClick.AddListener(delegate {
            ShowOption(false);
        });

        await Task.Delay((int)(autoHideTimeAfterStart * 1000));

        if(this == null)
            return;

        ShowOption(false);
    }

    void HandleOpenClick()
    {
        float currentTime = Time.time;
        
        // 如果超过重置时间，重置点击计数
        if (currentTime - lastClickTime > clickResetTime)
        {
            currentClickCount = 0;
        }
        
        currentClickCount++;
        lastClickTime = currentTime;
        
        // 如果达到指定点击次数，显示菜单
        if (currentClickCount >= openCount)
        {
            ShowOption(true);
            BTN_Option_Close.interactable = false;
            StartCoroutine(DelayedEnableButton());
            currentClickCount = 0; // 重置计数
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8)){
            ShowOption(!isActive);
        }
    }

    void ShowOption(bool val){
        ContentCanvas.blocksRaycasts = val;
        ContentCanvas.alpha = val ? 1 : 0;
        foreach (var item in needHides)
        {
            item.SetActive(val);
        }

        // 处理自动隐藏
        if (val)
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }
            autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
        }
        else
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
        }
    }

    IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(autoHideTime);
        ShowOption(false);
    }

    IEnumerator DelayedEnableButton()
    {
        yield return new WaitForSeconds(1f); // 延遲 1 秒
        BTN_Option_Close.interactable = true;
    }

    public void QuitApp(){
        Application.Quit();
    }
}
