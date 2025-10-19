using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace HimeLib {
    public class TimeArrange : SingletonMono<TimeArrange>
    {
        [System.Serializable]
        public enum PlayMode
        {
            No = 0,
            OnlyGame = 1,
            OnlyAd = 2,
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

        public Text TXT_Show;

        [Header("Runtime")]
        public PlayMode currentPlayMode;
        public ViewState currentViewState;
        public TimeFlagSetting currentFlagSetting;

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

                        if(TXT_Show) TXT_Show.text = $"{currentPlayMode} - {currentViewState}";
                        
                        goto NextLoop;
                    }
                }

                // 每15分鐘才會顯示一次
                if (currentTime.Minute % 15 == 0 && currentTime.Second == 0)
                {
                    Debug.LogWarning("No time flag matched. (" + currentTime.ToString("yyyy-MM-dd HH:mm:ss") + ")");
                }

                NextLoop:
                yield return new WaitForSeconds(1);
            }
        }

        ViewState NewViewByMode(PlayMode mode, DateTime ctime){
            if(mode == PlayMode.OnlyAd){
                return ViewState.Ad;
            }
            // if(mode == PlayMode.MoreGame){
            //     if(ctime.Minute < 50) return ViewState.Game;
            //     else return ViewState.Ad;
            // }
            return ViewState.Game;
        }
    }
}