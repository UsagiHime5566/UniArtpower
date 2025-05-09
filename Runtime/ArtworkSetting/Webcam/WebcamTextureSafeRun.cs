using UnityEngine;

public class WebcamTextureSafeRun : MonoBehaviour
{
#if UNITY_EDITOR
    GameObject dummy;
    void Update(){
        Debug.Log(dummy.name);
    }

    void LateUpdate(){
        Debug.Log(dummy.name);
    }
#endif
}
