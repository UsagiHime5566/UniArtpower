using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HimeLib;
using System.IO;
using System.Linq;

namespace HimeLib {
    public class TimeArrangeFile : SingletonMono<TimeArrangeFile>
    {
        [System.Serializable]
        public enum PlayMode
        {
            No = 0,
            OnlyAd = 1,
            OnlyGame = 2,
            MoreAd = 3,
            MoreGame = 4,
        }

        [System.Serializable]
        public enum ViewState
        {
            Game = 0,
            Ad = 1
        }

        [System.Serializable]
        public class TimeFlagSetting
        {
            public PlayMode playMode;
            public TimeRange timeRange;
        }

        [SerializeField] bool useDebug = false;
        [SerializeField] bool useAdOnly = false;

        [Header("UI Settings")]
        public Text TXT_Show;
        public GameObject OBJ_Warning;

        [Header("Settings")]
        public string specialPath = "";
        public string prefixTip = "[TimeArrange.txt] ";
        public string readFileName = "TimeArrange.txt";

        [Header("Runtime")]
        public PlayMode currentPlayMode;
        public ViewState currentViewState;
        public TimeFlagSetting currentFlagSetting;
        [SerializeField] List<string> readFileContent = new List<string>();
        [SerializeField] bool useReadFile = false;
        
        [Header("Settings")]
        public List<TimeFlagSetting> flagSettings = new List<TimeFlagSetting>();
        public System.Action<TimeFlagSetting, DateTime> OnTimeFlagChanged;

        [EasyButtons.Button]
        public void ClearStateFlag(){
            currentFlagSetting = null;
        }

        void Start()
        {
            StartTimeArrange();

            #if !UNITY_EDITOR
                useDebug = false;
                useAdOnly = false;
            #endif

            var testmode1 = string.Format("{0}/../{1}", Application.dataPath, "debug.txt");
            if(File.Exists(testmode1)){
                useDebug = true;
            }

            var testmode2 = string.Format("{0}/../{1}", Application.dataPath, "adonly.txt");
            if(File.Exists(testmode2)){
                useAdOnly = true;
            }
        }

        public void StartTimeArrange(){
            StartCoroutine(TimeTick());
        }

        IEnumerator TimeTick(){
            yield return null;
            ReadTimeArrange();
            while(true){
                if(useAdOnly){
                    currentPlayMode = PlayMode.OnlyAd;
                    currentViewState = ViewState.Ad;
                    goto NextLoop;
                }

                // 獲取當前時間
                DateTime currentTime = DateTime.Now;

                // 遍歷所有Flag設置，找到當前時間對應的Flag
                foreach (var setting in flagSettings)
                {
                    if (setting.timeRange.IsTimeInRange(currentTime))
                    {
                        if(currentFlagSetting != setting){  
                            currentFlagSetting = setting;
                            OnTimeFlagChanged?.Invoke(currentFlagSetting, currentTime);
                        }

                        currentPlayMode = setting.playMode;
                        currentViewState = NewViewByMode(currentPlayMode, currentTime);

                        if(useDebug){
                            Debug.Log($"{currentTime} - {currentPlayMode} - {currentViewState}");
                        }

                        if(TXT_Show) TXT_Show.text = $"{prefixTip} {currentPlayMode} - {currentViewState}";
                        
                        goto NextLoop;
                    }
                }

                Debug.LogWarning("No time flag matched.");

                NextLoop:
                yield return new WaitForSeconds(1);
            }
        }

        ViewState NewViewByMode(PlayMode mode, DateTime ctime){
            if(mode == PlayMode.OnlyAd){
                return ViewState.Ad;
            }
            if(mode == PlayMode.MoreGame){
                if(ctime.Minute < 50) return ViewState.Game;
                else return ViewState.Ad;
            }
            return ViewState.Game;
        }

        public void ReadTimeArrange(){
            string root = Application.dataPath;
            string path = Path.Combine(root, "..", readFileName);
            string path2 = Path.Combine(specialPath, readFileName);
            if(File.Exists(path)){
                string content = File.ReadAllText(path);
                readFileContent = content.Split('\n').ToList(); 
                Debug.Log(content);
                useReadFile = true;

                ReadFileContent();
                return;
            }
            if(File.Exists(path2)){
                string content = File.ReadAllText(path2);
                readFileContent = content.Split('\n').ToList(); 
                Debug.Log(content);
                useReadFile = true;

                ReadFileContent();
                return;
            }
            prefixTip = "";
        }
        public void ReadFileContent(){
            if(useReadFile){
                flagSettings = new List<TimeFlagSetting>();
                foreach(var line in readFileContent){
                    try {
                        string[] parts = line.Split('-');
                        var setting = new TimeFlagSetting{
                            playMode = PlayMode.OnlyAd,
                            timeRange = new TimeRange(parts[0], parts[1])
                        };
                        flagSettings.Add(setting);
                        setting.timeRange.InitializeTimes();

                    } catch (Exception e) {
                        Debug.LogError("讀取文件內容時發生錯誤: " + e.Message);
                        if(OBJ_Warning) OBJ_Warning.SetActive(true);
                    }
                }
            }
        }
    }
}