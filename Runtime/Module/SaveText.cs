using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace HimeLib
{
    public class SaveText
    {
        public static string SavePathTxtFile = Application.dataPath + "/Default/SaveText.txt";
        public static void Save(string txt)
        {
            if (!File.Exists(SavePathTxtFile))
            {
                File.WriteAllText(SavePathTxtFile, txt + "\n");
                Debug.Log("File created and text saved: " + SavePathTxtFile);
            }
            else
            {
                File.AppendAllText(SavePathTxtFile, txt + "\n");
            }
        }
    }
}