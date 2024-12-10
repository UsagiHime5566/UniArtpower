using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class FpsMonitor : MonoBehaviour
{
    public Text TXT_fps;
    ProfilerRecorder mainThreadTimeRecorder;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;

        #if ENABLE_UNSAFE
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }
        #endif

        return r;
    }

    void OnEnable()
    {
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);

        #if !ENABLE_UNSAFE
        Debug.LogWarning("尚未自行定義 ENABLE_UNSAFE，將不會檢測FPS，需要在專案勾選allow unsafe code並且新增自定義ENABLE_UNSAFE");
        #endif
    }

    void OnDisable()
    {
        mainThreadTimeRecorder.Dispose();
    }

    void Update(){
        if(TXT_fps) TXT_fps.text = GetFPSString();
    }

    public double GetFPS()
    {
        return GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
    }

    public string GetFPSString()
    {
        return $"Frame Time: {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms";
    }
}