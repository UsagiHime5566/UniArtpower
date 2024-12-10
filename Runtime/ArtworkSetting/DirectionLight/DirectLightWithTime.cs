using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Light))]
public class DirectLightWithTime : MonoBehaviour
{
    public int checkDurationSecond = 10;
    public float sunsetJudge = -6;

    [Header("Debug Mode")]
    public bool isDebug = false;
    [Range(0, 24)] public float cTime;


    //Private field
    Light clight;
    WaitForSeconds wait;

    void Awake(){
        clight = GetComponent<Light>();
        wait = new WaitForSeconds(checkDurationSecond);
    }

    private void OnEnable() {
        StartCoroutine(LightLoop());
    }
    
    IEnumerator LightLoop(){
        while(true){
            if(!isDebug){
                DateTime t = DateTime.Now;
                cTime = t.Hour + (t.Minute/60.0f);
            }
            SetIntensity(cTime);

            yield return wait;
        }
    }

    void SetIntensity(float currentHour){
        // 0 , 6 , 12 , 18 , 24
        // 0 , 1 , 0 ,  -1 ,  0
        // -1, 0 , 1 ,  0 ,  -1
        // 18 ,24, 6 , 12 ,  18    

        float ang = ((currentHour + sunsetJudge) / 24) * 360 * Mathf.Deg2Rad;
        clight.intensity = 0.5f + Mathf.Sin(ang) * 0.5f;
    }
}
