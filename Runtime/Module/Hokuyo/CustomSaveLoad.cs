using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace HimeLib
{
    public class CustomSaveLoad
    {
        static CustomSaveLoad instance;
        public static CustomSaveLoad Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CustomSaveLoad();
                }
                return instance;
            }
        }

        public string identifier = "LidarConfig.ini";
        public string SavePath => Application.dataPath + "/../" + identifier;
        
        private Dictionary<string, object> dataCache = new Dictionary<string, object>();
        private bool isLoaded = false;

        private CustomSaveLoad()
        {
            LoadData();
        }

        /// <summary>
        /// 儲存資料到檔案
        /// </summary>
        /// <param name="key">資料的鍵值</param>
        /// <param name="value">要儲存的資料</param>
        /// <returns>成功儲存回傳1，失敗回傳0</returns>
        public int SaveData(string key, object value)
        {
            try
            {
                // 更新快取
                dataCache[key] = value;
                
                // 寫入檔案
                WriteToFile();
                return 1;
            }
            catch (Exception e)
            {
                Debug.LogError($"儲存資料失敗: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 從檔案讀取資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="key">資料的鍵值</param>
        /// <param name="defaultValue">預設值</param>
        /// <returns>讀取到的資料或預設值</returns>
        public T GetData<T>(string key, T defaultValue)
        {
            try
            {
                if (!isLoaded)
                {
                    LoadData();
                }

                if (dataCache.ContainsKey(key))
                {
                    object value = dataCache[key];
                    if (value is T)
                    {
                        return (T)value;
                    }
                    else
                    {
                        // 嘗試轉換型別
                        try
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch
                        {
                            Debug.LogWarning($"無法將 {value} 轉換為 {typeof(T)} 型別");
                            return defaultValue;
                        }
                    }
                }
                return defaultValue;
            }
            catch (Exception e)
            {
                Debug.LogError($"讀取資料失敗: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 從檔案載入所有資料
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string[] lines = File.ReadAllLines(SavePath);
                    dataCache.Clear();

                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            continue;

                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            
                            // 嘗試解析為不同型別
                            object parsedValue = ParseValue(value);
                            dataCache[key] = parsedValue;
                        }
                    }
                }
                isLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"載入資料失敗: {e.Message}");
                isLoaded = true; // 避免重複載入
            }
        }

        /// <summary>
        /// 將資料寫入檔案
        /// </summary>
        private void WriteToFile()
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add("# CustomSaveLoad 設定檔");
                lines.Add($"# 生成時間: {DateTime.Now}");
                lines.Add("");

                foreach (var kvp in dataCache)
                {
                    lines.Add($"{kvp.Key}={kvp.Value}");
                }

                File.WriteAllLines(SavePath, lines);
            }
            catch (Exception e)
            {
                Debug.LogError($"寫入檔案失敗: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 解析字串值為適當的型別
        /// </summary>
        /// <param name="value">字串值</param>
        /// <returns>解析後的物件</returns>
        private object ParseValue(string value)
        {
            // 嘗試解析為整數
            if (int.TryParse(value, out int intValue))
                return intValue;

            // 嘗試解析為浮點數
            if (float.TryParse(value, out float floatValue))
                return floatValue;

            // 嘗試解析為布林值
            if (bool.TryParse(value, out bool boolValue))
                return boolValue;

            // 預設回傳字串
            return value;
        }

        /// <summary>
        /// 清除所有資料
        /// </summary>
        public void ClearData()
        {
            dataCache.Clear();
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }

        /// <summary>
        /// 檢查是否包含指定的鍵值
        /// </summary>
        /// <param name="key">要檢查的鍵值</param>
        /// <returns>如果包含則回傳true</returns>
        public bool HasKey(string key)
        {
            if (!isLoaded)
            {
                LoadData();
            }
            return dataCache.ContainsKey(key);
        }
    }
}
