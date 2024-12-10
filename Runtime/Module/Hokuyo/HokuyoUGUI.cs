using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Urg;
//from git url: https://github.com/curiosity-inc/urg-unity.git?path=Packages/jp.curiosity-inc.urg-unity
//再把此檔案複製到自己的script資料夾裡

namespace HimeLib
{
    public class HokuyoUGUI : SingletonMono<HokuyoUGUI>
    {
        public UrgSensor urg;
        public System.Action<Vector2> OnUGUIPosCome;

        private float[] rawDistances;
        private List<DetectedLocation> locations = new List<DetectedLocation>();
        private List<List<int>> clusterIndices;
        private AffineConverter affineConverter;
        private List<GameObject> debugObjects;
        private Object syncLock = new Object();
        private System.Diagnostics.Stopwatch stopwatch;
        EuclidianClusterExtraction cluster;

        [Header("Debug")]
        public bool useDebug;

        [Header("感應器參數")]
        public int temporalMedianFilter = 3;
        public int spatialMedianFilter = 3;
        public float distanceFilter = 2.25f;
        public float euclidianClusterExtraction = 0.1f;
        public float euclidianCluster = 0.5f;

        [Header("校正資料, 共四個點位, 感應器感之內的可遊玩區域")]
        public Vector2 [] sensorCorners = new Vector2[4];

        [Header("校正資料, 共四個點位, 要輸出的UGUI座標資料, 依序是 左上 右上 右下 左下")]
        public Vector3 [] worldCorners = new Vector3[4];

        [Header("校正資料(XY), 只允許XY值以內的資料輸出")]
        public float [] filterXX = new float[2];
        public float [] filterYY = new float[2];

        [Header("是否輸出相反資料 (由天花板照地板時為相反)")]
        public bool isFlip = false;
        void OnValidate() {
            
        }

        protected internal override void OnSingletonAwake()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // delegate method to be triggered when the new data is received from sensor.
            urg.OnDistanceReceived += Urg_OnDistanceReceived;

            // uncomment if you need some filters before clustering
            urg.AddFilter(new TemporalMedianFilter(temporalMedianFilter));
            urg.AddFilter(new SpatialMedianFilter(spatialMedianFilter));
            urg.AddFilter(new DistanceFilter(distanceFilter));
            urg.SetClusterExtraction(new EuclidianClusterExtraction(euclidianClusterExtraction));
            cluster = new EuclidianClusterExtraction(euclidianCluster);

            // var worldCorners = new Vector3[4];
            // worldCorners[0] = Screen2WorldPosition(new Vector2(0, Screen.height), cam, plane);
            // worldCorners[1] = Screen2WorldPosition(new Vector2(Screen.width, Screen.height), cam, plane);
            // worldCorners[2] = Screen2WorldPosition(new Vector2(Screen.width, 0), cam, plane);
            // worldCorners[3] = Screen2WorldPosition(new Vector2(0, 0), cam, plane);
            // worldCorners[0] = new Vector3(-685, 0, 600);
            // worldCorners[1] = new Vector3(685, 0, 600);
            // worldCorners[2] = new Vector3(685, 0, -600);
            // worldCorners[3] = new Vector3(-685, 0, -600);

            affineConverter = new AffineConverter(sensorCorners, worldCorners);

            if(useDebug)
            {
                debugObjects = new List<GameObject>();
                for (var i = 0; i < 100; i++)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.parent = transform;
                    obj.transform.localScale = 0.3f * Vector3.one;
                    debugObjects.Add(obj);
                }
            }
        }

        void Update()
        {
            if (urg == null)
            {
                return;
            }

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

            if (locations == null)
            {
                return;
            }

            clusterIndices = cluster.ExtractClusters(locations);

            int index = 0;
            for (var i = 0; i < clusterIndices.Count; i++)
            {
                if (clusterIndices[i].Count < 2)
                {
                    continue;
                }
                //Debug.Log(clusterIndices[i].Count);

                Vector2 center = Vector2.zero;
                foreach (var j in clusterIndices[i])
                {
                    if(j < locations.Count)
                        center += locations[j].ToPosition2D();
                }
                center /= (float)clusterIndices[i].Count;

                Vector3 worldPos = new Vector3(0, 0, 0);
                var inRegion = affineConverter.Sensor2WorldPosition(center, out worldPos);

                if (inRegion){

                    //Gizmos.DrawCube(worldPos, new Vector3(0.1f, 0.1f, 0.1f));
                    if(Mathf.Abs(worldPos.x) > filterXX[0] && Mathf.Abs(worldPos.x) < filterXX[1] &&
                        Mathf.Abs(worldPos.z) > filterYY[0] && Mathf.Abs(worldPos.z) < filterYY[1]){
                       // Debug.Log(worldPos);
                        if(isFlip){
                            OnUGUIPosCome?.Invoke(new Vector2(worldCorners[1].x - worldPos.x, worldCorners[1].z - worldPos.z));
                        } else {
                            OnUGUIPosCome?.Invoke(new Vector2(worldPos.x, worldPos.z));
                        }
                    }

                    if(useDebug && index < debugObjects.Count){
                        debugObjects[index].transform.position = worldPos;
                        index++;
                    }
                }
            }

            if(useDebug)
            {
                for (var i = index; i < debugObjects.Count; i++)
                {
                    debugObjects[i].transform.position = new Vector3(100, 100, 100);
                }
            }
        }

        void Urg_OnDistanceReceived(DistanceRecord data)
        {
            //Debug.LogFormat("distance received: SCIP timestamp={0} unity timer={1}", data.Timestamp, stopwatch.ElapsedMilliseconds);
            //Debug.LogFormat("cluster count: {0}", data.ClusteredIndices.Count);
            this.rawDistances = data.RawDistances;
            this.locations = data.FilteredResults;
            this.clusterIndices = data.ClusteredIndices;
        }

        private static Vector3 Screen2WorldPosition(Vector2 screenPosition, Camera camera, Plane basePlane)
        {
            var ray = camera.ScreenPointToRay(screenPosition);
            var distance = 0f;

            if (basePlane.Raycast(ray, out distance))
            {
                var p = ray.GetPoint(distance);
                return p;
            }
            return Vector3.negativeInfinity;
        }

        static UnityEngine.Vector2 ScreenPosToUGUI(UnityEngine.Vector2 screenPosition, Camera camera, RectTransform basePlane){
            // 将屏幕坐标转换为 Canvas 上的局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(basePlane, screenPosition, camera, out UnityEngine.Vector2 localPoint);

            // 设置图像的位置为鼠标当前位置
            return localPoint;
        }
    }
}