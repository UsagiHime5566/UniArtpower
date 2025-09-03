using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;

namespace HimeLib
{
    public class ReadEmuSignal : MonoBehaviour
    {
        [Header("Config")]
        public string SignalFileName = "Signal.txt";
        [Range(0.1f, 1)] public float signalDelay = 0.1f;

        [Header("Runtime")]
        public List<Vector2> listPos;
        public List<List<Vector2>> listVector2;
        public List<List<Urg.DetectedLocation>> listDetectedLocation;
        public string filePath;
        public bool isReady = false;
        public int currentIndex = 0;

        public System.Action OnReadEmuSignalReady;
        public System.Action<List<Urg.DetectedLocation>> OnReadEmuSignal;
        public System.Action<List<Vector2>> OnReadVector2Signal;
        
        void Start()
        {
            listPos = new List<Vector2>();
            listDetectedLocation = new List<List<Urg.DetectedLocation>>();
            listVector2 = new List<List<Vector2>>();
            
            filePath = Application.dataPath + "/../" + SignalFileName;
            Task.Run(() => {
                string data = File.ReadAllText(filePath);
                string[] lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                
                List<Urg.DetectedLocation> currentGroup = new List<Urg.DetectedLocation>();
                List<Vector2> currentGroupVector2 = new List<Vector2>();
                
                foreach (string line in lines)
                {
                    // 檢查是否為分隔線
                    if (line.Trim() == "===============")
                    {
                        // 如果當前組有資料，則加入 listPosList
                        if (currentGroup.Count > 0)
                        {
                            listDetectedLocation.Add(new List<Urg.DetectedLocation>(currentGroup));
                            currentGroup.Clear();
                        }
                        if (currentGroupVector2.Count > 0)
                        {
                            listVector2.Add(new List<Vector2>(currentGroupVector2));
                            currentGroupVector2.Clear();
                        }
                        continue;
                    }
                    
                    // 檢查是否為座標行 (格式: (x, y))
                    if (line.Trim().StartsWith("(") && line.Trim().EndsWith(")"))
                    {
                        try
                        {
                            // 移除括號並分割座標
                            string coordStr = line.Trim().Substring(1, line.Trim().Length - 2);
                            string[] coords = coordStr.Split(',');
                            
                            if (coords.Length == 2)
                            {
                                float x = float.Parse(coords[0].Trim());
                                float y = float.Parse(coords[1].Trim());
                                
                                Vector2 pos = new Vector2(x, y);
                                listPos.Add(pos); // 加入單一列表

                                currentGroupVector2.Add(pos);
                                
                                // 將 Vector2 轉換為 DetectedLocation
                                Urg.DetectedLocation detectedLocation = Vector2ToDetectedLocation(pos);
                                currentGroup.Add(detectedLocation);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"無法解析座標行: {line}, 錯誤: {e.Message}");
                        }
                    }
                }
                
                // 不要處理最後一組資料
                
                Debug.Log($"成功載入 {listDetectedLocation.Count} 組座標資料，總共 {listPos.Count} 個座標點");
                isReady = true;
            });

            StartCoroutine(ReadEmuSignalCoroutine());
        }

        /// <summary>
        /// 將 Vector2 座標轉換為 DetectedLocation
        /// </summary>
        /// <param name="position">Vector2 座標</param>
        /// <returns>DetectedLocation 物件</returns>
        private Urg.DetectedLocation Vector2ToDetectedLocation(Vector2 position)
        {
            // 計算距離
            float distance = Mathf.Sqrt(position.x * position.x + position.y * position.y);
            
            // 計算角度（弧度）
            float angleInRadians = Mathf.Atan2(position.y, position.x);
            
            // 將弧度轉換為角度，因為 DetectedLocation.angle 欄位存儲的是角度
            float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
            
            // 創建 DetectedLocation（angle 欄位存儲角度）
            return new Urg.DetectedLocation(0, angleInDegrees, distance);
        }

        IEnumerator ReadEmuSignalCoroutine()
        {
            yield return new WaitUntil(() => isReady);
            OnReadEmuSignalReady?.Invoke();
            while (true){
                yield return new WaitForSeconds(signalDelay);

                var outputDL = new List<Urg.DetectedLocation>(listDetectedLocation[currentIndex]);
                var outputV2 = new List<Vector2>(listVector2[currentIndex]);

                OnReadEmuSignal?.Invoke(outputDL);
                OnReadVector2Signal?.Invoke(outputV2);
                
                currentIndex++;
                currentIndex %= listDetectedLocation.Count;
            }
        }
    }
}