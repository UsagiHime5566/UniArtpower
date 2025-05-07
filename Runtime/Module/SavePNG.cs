using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace HimeLib
{
    public class SavePNG
    {
        public static System.Action<string> OnPNGSaved;
        public static string lastFileName;
        public static void Save(Texture2D tex, string path)
        {
            if(tex == null) return;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();

            //Write to a file in the project folder
            //string path = Application.dataPath + "/../SavedPaint.png";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            string fullPath = Path.Combine(path, fileName);

            File.WriteAllBytes(fullPath, bytes);

            //Debug.Log(bytes.Length/1024  + "Kb was saved as: " + fullPath);

            OnPNGSaved?.Invoke(fullPath);
            lastFileName = fileName;
        }
    }
}