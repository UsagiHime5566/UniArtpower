using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionTXT : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = $"v {Application.version}";
    }
}
