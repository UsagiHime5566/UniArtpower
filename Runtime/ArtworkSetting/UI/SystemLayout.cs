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
    public bool isActive => ContentCanvas.blocksRaycasts;
    
    async void Start()
    {
        BTN_Option_Open.onClick.AddListener(delegate {
            ShowOption(true);

        });

        BTN_Option_Close.onClick.AddListener(delegate {
            ShowOption(false);
        });

        await Task.Delay(10000);

        if(this == null)
            return;

        ShowOption(false);
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
    }
}
