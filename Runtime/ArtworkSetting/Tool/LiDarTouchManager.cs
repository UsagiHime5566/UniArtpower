using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HimeLib;
using System.Collections.Generic;

namespace HimeLib
{
    public class LiDarTouchManager : SingletonMono<LiDarTouchManager>
    {
        [HelpBox] public string helpBoxText = "用法:觸發事件UGUI_PosCome來讓座標可以轉換成按鈕點擊";

        [Header("使用滑鼠座標代替感測器座標？")]
        public bool useMousePos;

        [Header("觸碰冷卻時間")]
        public float cooldownTime = 0.5f;

        [Header("至少要觸碰幾次才會觸發")]
        public int affectClickTime = 2;

        [Header("顯示觸發座標，如果使用的話需設置座標要顯示在哪個Canvas上，Image的Parent要為Canvas，快捷鍵Tab切換")]
        public bool showTouchPos = false;
        public Canvas touchPosCanvas;
        public Image touchPosImage;

        [Header("Runtime")]
        [SerializeField] Vector2 screenCoordinate;
        [SerializeField] bool isInCooldown = false;
        [SerializeField] float cooldownRemain = 0;
        [SerializeField] GameObject lastTouchObject;
        [SerializeField] int clickTimes = 0;
        
        // Unity 生命週期方法
        void Start()
        {
            //Hokuyo 安裝後再取消註解
            //HokuyoUGUI.instance.OnUGUIPosCome += UGUI_PosCome;
        }

        void Update()
        {
            if (useMousePos)
            {
                screenCoordinate = Input.mousePosition;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isInCooldown == false)
                {
                    TrigPosEvent();
                }
            }

            if(Input.GetKeyDown(KeyCode.Tab))
            {
                showTouchPos = !showTouchPos;
                if(touchPosImage != null) touchPosImage.enabled = showTouchPos;
            }

            if(showTouchPos && touchPosCanvas && touchPosImage){
                touchPosImage.raycastTarget = false;
                touchPosImage.rectTransform.localPosition = ScreenPosToCanvasPos(touchPosCanvas, screenCoordinate);
            }

            cooldownRemain -= Time.deltaTime;
            if (cooldownRemain < 0)
            {
                isInCooldown = false;
            }
        }

        // 公共方法
        public void TrigPosEvent()
        {
            // 檢測特定螢幕座標上是否有UI元素
            GameObject clickedObject = GetObjectAtScreenCoordinate(screenCoordinate);

            if (clickedObject != null)
            {
                // 先檢查物件本身是否有 Button 組件
                Button button = clickedObject.GetComponent<Button>();
                
                // 如果沒有，則向上搜尋父物件
                if (button == null)
                {
                    button = clickedObject.GetComponentInParent<Button>();
                    if (button != null)
                    {
                        // 更新 clickedObject 為 Button 物件
                        clickedObject = button.gameObject;
                    }
                }
                
                if (button != null && button.interactable)
                {
                    if (lastTouchObject == clickedObject)
                    {
                        clickTimes++;
                    } 
                    else 
                    {
                        lastTouchObject = clickedObject;
                        clickTimes = 1;
                    }

                    if (clickTimes >= affectClickTime)
                    {
                        button.onClick.Invoke();
                        clickTimes = 0;
                        EnterTrigCD();
                    }
                }
            }
        }

        public void UGUI_PosCome(Vector2 pos)
        {
            //Debug.Log($"come : {pos}");
            screenCoordinate = pos;
            if (isInCooldown == false)
            {
                TrigPosEvent();
            }
        }

        void EnterTrigCD()
        {
            isInCooldown = true;
            cooldownRemain = cooldownTime;
        }

        // 獲取特定螢幕座標上的 UI 元素，參數為螢幕座標
        GameObject GetObjectAtScreenCoordinate(Vector2 coordinate)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = coordinate;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            if (results.Count == 0)
                return null;
                
            // 返回第一個物件，讓 TrigPosEvent 來處理 Button 檢測
            return results[0].gameObject;
        }

        Vector2 ScreenPosToCanvasPos(Canvas canvas, Vector3 mousePos)
        {
            Vector3 mousePosition = mousePos;

            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, mousePosition, cam, out Vector2 localPoint);

            return localPoint;
        }
    }
}