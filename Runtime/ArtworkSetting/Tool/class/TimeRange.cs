using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeRange
{
    public string startTimeString; // 開始時間的字符串表示（格式：hh:mm）
    public string endTimeString; // 結束時間的字符串表示（格式：hh:mm）

    [HideInInspector]
    public TimeSpan startTime; // 開始時間的TimeSpan對象
    
    [HideInInspector]
    public TimeSpan endTime; // 結束時間的TimeSpan對象

    bool initialized = false;

    public TimeRange(string startTimeString, string endTimeString)
    {
        this.startTimeString = startTimeString;
        this.endTimeString = endTimeString;
    }

    // 檢查指定的時間是否在範圍內
    public bool IsTimeInRange(DateTime time)
    {
        InitializeTimes();
        TimeSpan currentTime = time.TimeOfDay;

        if (endTime <= startTime)
        {
            // 當結束時間小於等於開始時間時，表示跨越了一天的時間範圍
            // 因此，檢查當前時間是否在開始時間之後或者在結束時間之前即可
            return currentTime >= startTime || currentTime <= endTime;
        }
        else
        {
            // 檢查當前時間是否在開始時間和結束時間之間
            return currentTime >= startTime && currentTime <= endTime;
        }
    }

    public void InitializeTimes()
    {
        if (initialized == false)
        {
            startTime = TimeSpan.Parse(startTimeString);
            endTime = TimeSpan.Parse(endTimeString);
            initialized = true;
        }
    }
}
