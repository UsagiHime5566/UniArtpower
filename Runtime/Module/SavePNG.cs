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
        
        // 通用的保存方法
        private static void SaveInternal(Texture2D tex, string path, string fileName)
        {
            if(tex == null) return;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();

            string fullPath = Path.Combine(path, fileName);

            File.WriteAllBytes(fullPath, bytes);

            //Debug.Log(bytes.Length/1024  + "Kb was saved as: " + fullPath);

            OnPNGSaved?.Invoke(fullPath);
            lastFileName = fileName;
        }

        public static void Save(Texture2D tex, string path)
        {
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            SaveInternal(tex, path, fileName);
        }

        public static void SaveLongFileName(Texture2D tex, string path)
        {
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + Random.Range(0,36).ToString("X") + Random.Range(0,36).ToString("X") + ".png";
            SaveInternal(tex, path, fileName);
        }
    }
}