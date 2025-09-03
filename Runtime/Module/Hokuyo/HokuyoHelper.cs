using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Urg;

//使用前先安裝 git url: https://github.com/curiosity-inc/urg-unity.git?path=Packages/jp.curiosity-inc.urg-unity
//依照url指示設定環境

namespace HimeLib
{
    public class HokuyoHelper : MonoBehaviour
    {
        [Header("Hokuyo UST Prefab")]
        public UrgSensor urg;

        [Header("元件 Component")]
        public ReadEmuSignal readEmuSignal;

        [Header("設定")]
        public bool useEmuSignal = false;
        public bool isFlip = false;

        [Header("感應器參數 單位: 公尺")]
        public int temporalMedianFilter = 3;
        public int spatialMedianFilter = 3;
        public float distanceFilter = 2.25f;    //Hokuyo's vision radius
        public float euclidianClusterExtraction = 0.1f;
        public float euclidianCluster = 0.2f;

        [Header("地圖參數: sensor 單位公尺， world 單位 pixels")]
        public MinMaxFloat rawLocationFilter_X = new MinMaxFloat(-2, 2);
        public MinMaxFloat rawLocationFilter_Y = new MinMaxFloat(-2, 2);

        [Tooltip("感測器取用內容的角落座標，按照順序：右上、右下、左下、左上")]
        public Vector2 [] sensorCorners = new Vector2[4]{new Vector2(3, 3), new Vector2(3, -3), new Vector2(-3, -3), new Vector2(-3, 3)};

        [Tooltip("世界座標角落，根據該角落是你畫面的哪個點來填數值，比如說感測器右上角是你畫面的右下角，那就填(1920, 0)")]
        public Vector2 [] worldCorners = new Vector2[4]{new Vector2(1920, 1200), new Vector2(1920, 0), new Vector2(0, 0), new Vector2(0, 1200)};
        public MinMaxFloat worldCornerFilter_X = new MinMaxFloat(0, 1920);
        public MinMaxFloat worldCornerFilter_Y = new MinMaxFloat(0, 1200);


        public System.Action<List<DetectedLocation>> OnRefineLocationCome;
        public System.Action<List<Vector2>> OnClusterLocationCome;
        public System.Action<List<Vector2>> OnConvertLocationCome;
        public System.Action<Vector2> OnScreenPosCome;
        public UnityEvent<Vector2> OnScreenPosComeUnityEvent;


        private float[] rawDistances;
        private List<DetectedLocation> locations = new List<DetectedLocation>();
        //private List<List<int>> clusterIndices; //URG給的點群索引，不使用
        private EuclidianClusterExtraction cluster;
        private LocationConverter locationConverter;

#if UNITY_EDITOR
        void OnValidate(){
            SetupUrgFilters();
            SetupAffineConverter();
        }
#endif

        void Urg_OnDistanceReceived(DistanceRecord data)
        {
            if(useEmuSignal)
                return;

            rawDistances = data.RawDistances;
            locations = data.FilteredResults;
            //clusterIndices = data.ClusteredIndices;
        }

        void OnReadEmuUstSignal(List<Urg.DetectedLocation> emuLocations)
        {
            if(!useEmuSignal)
                return;

            locations = new List<Urg.DetectedLocation>();
            foreach(var location in emuLocations){
                var newLocation = new Urg.DetectedLocation(location.rawIndex, location.angle + (135 + urg.offsetDegrees), location.distance);
                locations.Add(newLocation);
            }
        }

        void Awake()
        {
            //使用真實訊號或模擬訊號
            readEmuSignal.OnReadEmuSignal += OnReadEmuUstSignal;
            urg.OnDistanceReceived += Urg_OnDistanceReceived;
            
            SetupUrgFilters();
            SetupAffineConverter();
        }

        void Update()
        {
            if(urg == null && !useEmuSignal)
                return;
            
            //在Unity Editor中繪製Urg雷射光，以Top視角觀看結果
            DrawUrgRays(urg);

            //等待感測器或虛擬訊號進入
            if (locations == null)
                return;

            //過濾雜訊，刪除不要的點
            var refinedLocations = LocationFilter(locations, rawLocationFilter_X, rawLocationFilter_Y);

            var clusterIndices = cluster.ExtractClusters(refinedLocations);
            var predictedLocations = new List<Vector2>();
            var inRegionUGUIPoints = new List<Vector2>();

            for (var i = 0; i < clusterIndices.Count; i++)
            {
                if (clusterIndices[i].Count < 2)
                    continue;
                
                Vector2 center = Vector2.zero;
                foreach (var j in clusterIndices[i])
                {
                    if (j < refinedLocations.Count)
                        center += refinedLocations[j].ToPosition2D();
                }
                try
                {
                    center /= (float)clusterIndices[i].Count;
                }
                catch { return; }

                predictedLocations.Add(center);

                Vector3 worldPos = new Vector3(0, 0, 0);
                var inRegion = locationConverter.Sensor2WorldPosition(center, out worldPos);

                if (inRegion)
                {
                    if (worldCornerFilter_X.IsInRange(worldPos.x) && worldCornerFilter_Y.IsInRange(worldPos.z))
                    {
                        Vector2 outputWorldPos = isFlip ?
                            new Vector2(worldCorners[1].x - worldPos.x, worldCorners[1].y - worldPos.z) :
                            new Vector2(worldPos.x, worldPos.z);

                        inRegionUGUIPoints.Add(outputWorldPos);
                        OnScreenPosCome?.Invoke(outputWorldPos);
                        OnScreenPosComeUnityEvent?.Invoke(outputWorldPos);
                    }
                }
            }

            OnRefineLocationCome?.Invoke(refinedLocations);
            OnClusterLocationCome?.Invoke(predictedLocations);
            OnConvertLocationCome?.Invoke(inRegionUGUIPoints);
        }

        public void SetupUrgFilters(){
            if(urg == null)
                return;
                
            //使用過濾器和聚類算法
            urg.RemoveAllFilters();
            urg.AddFilter(new TemporalMedianFilter(temporalMedianFilter));
            urg.AddFilter(new SpatialMedianFilter(spatialMedianFilter));
            urg.AddFilter(new DistanceFilter(distanceFilter));

            urg.ClearClusterExtraction();
            urg.SetClusterExtraction(new EuclidianClusterExtraction(euclidianClusterExtraction));

            cluster = new EuclidianClusterExtraction(euclidianCluster);
        }

        public void SetupAffineConverter(){
            var worldCorners3D = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                worldCorners3D[i] = new Vector3(worldCorners[i].x, 0, worldCorners[i].y);
            }
            locationConverter = new LocationConverter(sensorCorners, worldCorners3D);
        }

        void DrawUrgRays(UrgSensor urg){
            if(urg == null)
                return;

            if (rawDistances != null && rawDistances.Length > 0)
            {
                for (int i = 0; i < rawDistances.Length; i++)
                {
                    float distance = rawDistances[i];
                    float angle = urg.StepAngleRadians * i + urg.OffsetRadians;
                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);
                    var dir = new Vector3(cos, 0, sin);
                    var pos = distance * dir;

                    Debug.DrawRay(urg.transform.position, pos, Color.blue);
                }
            }
        }

        List<DetectedLocation> LocationFilter(List<DetectedLocation> locations, MinMaxFloat minmaxX, MinMaxFloat minmaxY)
        {
            for (int i = locations.Count - 1; i >= 0; i--)
            {
                var p = locations[i].ToPosition2D();
                if (!minmaxX.IsInRange(p.x) || !minmaxY.IsInRange(p.y))
                {
                    locations.RemoveAt(i);
                }
            }
            return new List<DetectedLocation>(locations);
        }
    }

    [System.Serializable]
    public class MinMaxFloat
    {
        public float min;
        public float max;
        
        public MinMaxFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        
        public float GetRandomValue()
        {
            return UnityEngine.Random.Range(min, max);
        }
        
        public bool IsInRange(float value)
        {
            return value >= min && value <= max;
        }
    }
}