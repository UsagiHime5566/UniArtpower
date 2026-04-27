using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HimeLib;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

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
    
    [Header("Debug Params 僅動態顯示 非參數")]
    [SerializeField] int currentClickCount = 0;
    [SerializeField] float lastClickTime;
    [SerializeField] float lastOperateTime;
    float clickResetTime = 2f; // 2秒內需要再次點擊才做計算
    Coroutine autoHideCoroutine;
    RectTransform systemLayoutRect;
    Canvas parentCanvas;
    
    async void Start()
    {
        systemLayoutRect = transform as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();

        BTN_Option_Open.onClick.AddListener(delegate {
            HandleOpenClick();
        });

        BTN_Option_Close.onClick.AddListener(delegate {
            ShowOption(false);
            SystemConfig.Instance.SaveValues();
        });

        await Task.Delay((int)(autoHideTimeAfterStart * 1000));

        if(this == null)
            return;

        ShowOption(false);
    }

    void HandleOpenClick()
    {
        float currentTime = Time.time;
        
        // 如果超過重置時間，重置點擊計數
        if (currentTime - lastClickTime > clickResetTime)
        {
            currentClickCount = 0;
        }
        
        currentClickCount++;
        lastClickTime = currentTime;
        
        // 如果達到指定點擊次數，顯示選單
        if (currentClickCount >= openCount)
        {
            ShowOption(true);
            BTN_Option_Close.interactable = false;
            StartCoroutine(DelayedEnableButton());
            currentClickCount = 0; // 重置計數
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8)){
            ShowOption(!isActive);
        }

        if (isActive && IsOperatingOnSystemLayout())
        {
            ResetAutoHideTimer();
        }
    }

    void ShowOption(bool val){
        ContentCanvas.blocksRaycasts = val;
        ContentCanvas.alpha = val ? 1 : 0;
        foreach (var item in needHides)
        {
            item.SetActive(val);
        }

        // 處理自動隱藏
        if (val)
        {
            ResetAutoHideTimer();
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
        while (Time.time - lastOperateTime < autoHideTime)
        {
            yield return null;
        }

        ShowOption(false);
    }

    void ResetAutoHideTimer()
    {
        lastOperateTime = Time.time;
    }

    bool IsOperatingOnSystemLayout()
    {
        if (systemLayoutRect == null || autoHideTime <= 0f)
        {
            return false;
        }

        if (IsSelectedObjectInSystemLayout() && Input.anyKeyDown)
        {
            return true;
        }

        return IsPointerOperatingOnSystemLayout();
    }

    bool IsSelectedObjectInSystemLayout()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            return false;
        }

        return EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform);
    }

    bool IsPointerOperatingOnSystemLayout()
    {
        if (IsPointerInSystemLayout(Input.mousePosition) && 
            (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)))
        {
            return true;
        }

        if (IsPointerInSystemLayout(Input.mousePosition) && Input.mouseScrollDelta.sqrMagnitude > 0f)
        {
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled && IsPointerInSystemLayout(touch.position))
            {
                return true;
            }
        }

        return false;
    }

    bool IsPointerInSystemLayout(Vector2 screenPosition)
    {
        Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(systemLayoutRect, screenPosition, uiCamera);
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
