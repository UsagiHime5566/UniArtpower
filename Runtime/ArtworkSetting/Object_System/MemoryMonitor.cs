using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

public class MemoryMonitor : MonoBehaviour
{
    ProfilerRecorder systemMemoryRecorder;
    void OnEnable() {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    void OnDisable() {
        systemMemoryRecorder.Dispose();
    }

    public string GetMemoryUsageString(){
        return $"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB";
    }

    public long GetMemoryUsageMB(){
        return systemMemoryRecorder.LastValue / (1024 * 1024);
    }
}
