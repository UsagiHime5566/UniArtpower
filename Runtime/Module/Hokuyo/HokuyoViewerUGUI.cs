using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Urg;

namespace HimeLib
{
    public class HokuyoViewerUGUI : MonoBehaviour
    {
        public HokuyoHelper hokuyoHelper;

        [Header("UI")]
        public Text normalInfo;
        public Text clusterInfo;
        public Text convertInfo;

        [Header("Canvas & Prefab")]
        public Canvas CanvasPointsPreview;
        public Canvas CanvasParamUI;
        public RectTransform PanelConvertedOutput;
        public Image Prefab_Point;

        [Header("Filter Parameters")]
        public List<Image> FilterXImages;
        public List<Image> FilterYImages;
        public float rawPointScale = 100;

        [Header("Config")]
        public bool previewDetectInfo = true;
        

        //邊界pool
        List<Image> affineConverterPool = new List<Image>();

        //點池pool
        List<Image> normalPool = new List<Image>();
        List<Image> clusterPool = new List<Image>();
        List<Image> convertPool = new List<Image>();

        void Start()
        {
            hokuyoHelper.OnRefineLocationCome += OnRefineLocationCome;
            hokuyoHelper.OnClusterLocationCome += OnClusterLocationCome;
            hokuyoHelper.OnConvertLocationCome += OnConvertLocationCome;
            StartCoroutine(DelayHidePreview());

            for (int i = 0; i < 100; i++)
            {
                Image newPoint = Instantiate(Prefab_Point, CanvasPointsPreview.transform);
                newPoint.name = "affineConverterPoint_" + i;
                newPoint.transform.localScale = Vector3.one;
                newPoint.color = Color.yellow;
                newPoint.enabled = true;
                affineConverterPool.Add(newPoint);
            }

        }

        IEnumerator DelayHidePreview(){
            yield return new WaitForSeconds(5);
            previewDetectInfo = false;
            CanvasParamUI.enabled = false;
            CanvasPointsPreview.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1)){
                previewDetectInfo = !previewDetectInfo;
                CanvasParamUI.enabled = previewDetectInfo;
                CanvasPointsPreview.enabled = previewDetectInfo;
            }
            if(Input.GetKeyDown(KeyCode.F2)){
                CanvasParamUI.GetComponent<GraphicRaycaster>().enabled = !CanvasParamUI.GetComponent<GraphicRaycaster>().enabled;
            }
            
            if(!previewDetectInfo)
                return;

            UpdateFilter();
            UpdateInfo();
            UpdateAffineConverter();
        }

        void UpdateInfo()
        {
            normalInfo.text = "Normal: " + normalPool.Count;
            clusterInfo.text = "Cluster: " + clusterPool.Count;
            convertInfo.text = "Convert: " + convertPool.Count;
        }

        void UpdateFilter()
        {
            FilterXImages[0].rectTransform.anchoredPosition = new Vector2(hokuyoHelper.rawLocationFilter_X.min, 0) * rawPointScale;
            FilterXImages[1].rectTransform.anchoredPosition = new Vector2(hokuyoHelper.rawLocationFilter_X.max, 0) * rawPointScale;
            FilterYImages[0].rectTransform.anchoredPosition = new Vector2(0, hokuyoHelper.rawLocationFilter_Y.min) * rawPointScale;
            FilterYImages[1].rectTransform.anchoredPosition = new Vector2(0, hokuyoHelper.rawLocationFilter_Y.max) * rawPointScale;
        }

        void UpdateAffineConverter()
        {
            // 計算矩形的四條邊
            Vector3[] edges = new Vector3[4];
            edges[0] = hokuyoHelper.sensorCorners[1] - hokuyoHelper.sensorCorners[0]; // 右邊
            edges[1] = hokuyoHelper.sensorCorners[2] - hokuyoHelper.sensorCorners[1]; // 下邊
            edges[2] = hokuyoHelper.sensorCorners[3] - hokuyoHelper.sensorCorners[2]; // 左邊
            edges[3] = hokuyoHelper.sensorCorners[0] - hokuyoHelper.sensorCorners[3]; // 上邊

            // 每條邊分佈25個點
            for (int i = 0; i < 100; i++)
            {
                int edgeIndex = i / 25; // 決定在哪條邊上
                int pointIndex = i % 25; // 在該邊上的位置
                
                // 計算點的位置：起點 + 沿邊方向的偏移
                Vector3 startPoint = hokuyoHelper.sensorCorners[edgeIndex];
                Vector3 direction = edges[edgeIndex];
                float t = pointIndex / 24.0f; // 0到1的參數，24是因為25個點需要24個間隔
                
                affineConverterPool[i].rectTransform.anchoredPosition = (startPoint + direction * t) * rawPointScale;
            }
        }

        private void ShowPointsInCanvas<T>(List<T> workLocations, List<Image> pointsPool, string namePrefix, Color pointColor, Vector3 scale, Transform parent, System.Func<T, Vector2> positionConverter)
        {
            int lastPoolIndex = 0;
            
            // 啟用並設置需要的點
            for (int i = 0; i < workLocations.Count; i++)
            {
                Image workPoint = GetOrCreatePoint(pointsPool, i, namePrefix, pointColor, scale, parent);
                workPoint.rectTransform.anchoredPosition = positionConverter(workLocations[i]);
                workPoint.enabled = true;
                lastPoolIndex = i;
            }
            
            // 禁用不需要的點
            for (int i = lastPoolIndex + 1; i < pointsPool.Count; i++)
            {
                pointsPool[i].enabled = false;
            }
            
            // 如果沒有點但池中有點，禁用第一個
            if (workLocations.Count == 0 && pointsPool.Count > 0 && pointsPool[0] != null)
            {
                pointsPool[0].enabled = false;
            }
        }

        private Image GetOrCreatePoint(List<Image> pointsPool, int index, string namePrefix, Color pointColor, Vector3 scale, Transform parent)
        {
            if (index >= pointsPool.Count)
            {
                Image newPoint = Instantiate(Prefab_Point, parent);
                newPoint.name = namePrefix + index;
                newPoint.color = pointColor;
                newPoint.rectTransform.localScale = scale;
                if(parent == PanelConvertedOutput){
                    newPoint.rectTransform.anchorMax = Vector2.zero;
                    newPoint.rectTransform.anchorMin = Vector2.zero;
                }
                pointsPool.Add(newPoint);
                return newPoint;
            }
            return pointsPool[index];
        }

        void OnRefineLocationCome(List<DetectedLocation> locations)
        {
            if(!previewDetectInfo)
                return;

            //將點位置配給anchoredPosition
            ShowPointsInCanvas<DetectedLocation>(locations, normalPool, "point_", Color.white, Vector3.one, CanvasPointsPreview.transform,
                (location) => location.ToPosition2D() * rawPointScale);
        }
        void OnClusterLocationCome(List<Vector2> predictedLocations)
        {
            if(!previewDetectInfo)
                return;

            //將點位置配給anchoredPosition
            ShowPointsInCanvas<Vector2>(predictedLocations, clusterPool, "predicted_point_", Color.red, Vector3.one, CanvasPointsPreview.transform,
                (location) => location * rawPointScale);
        }
        void OnConvertLocationCome(List<Vector2> inRegionUGUIPoints)
        {
            if(!previewDetectInfo)
                return;

            //將點位置配給anchoredPosition, 但這個點要用preview不一樣
            ShowPointsInCanvas<Vector2>(inRegionUGUIPoints, convertPool, "inRegionUGUIPoint_", Color.green, Vector3.one * 4f, PanelConvertedOutput,
                (location) => location); //Screen Position
        }
    }
}