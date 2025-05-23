﻿using UnityEngine;
using UnityEditor;
 
public class TakeScreenshotInEditor : ScriptableObject
{
    public static string fileName = "Editor Screenshot ";
    public static int startNumber = 1;
 
    [MenuItem ("Custom/Take Screenshot of Game View %^w")]
    static void TakeScreenshot()
    {
        int number = startNumber;
        string name = "" + number;
 
        while (System.IO.File.Exists(fileName + name + ".png"))
        {
            number++;
            name = "" + number;
        }
 
        startNumber = number + 1;
 
        ScreenCapture.CaptureScreenshot(fileName + name + ".png");
    }
}