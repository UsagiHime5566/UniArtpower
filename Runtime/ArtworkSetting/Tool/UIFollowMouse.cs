using UnityEngine;
using UnityEngine.UI;

namespace HimeLib
{
    [RequireComponent(typeof(Image))]
    public class UIFollowMouse : MonoBehaviour
    {
        public bool isShowDebugLog = false;
        public Canvas inWhichCanvas;
        Image image; // 要顯示的圖像

        void Awake()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
        }

        void Update()
        {
            // 獲取滑鼠當前的螢幕座標
            Vector3 mousePosition = Input.mousePosition;
            if(isShowDebugLog) Debug.Log($"mousePosition : {mousePosition}");

            Camera cam = inWhichCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : inWhichCanvas.worldCamera;

            // 將螢幕座標轉換為 Canvas 上的局部座標
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)inWhichCanvas.transform, mousePosition, cam, out Vector2 localPoint);

            // 設置圖像的位置為滑鼠當前位置
            image.rectTransform.localPosition = localPoint;
        }
    }
}