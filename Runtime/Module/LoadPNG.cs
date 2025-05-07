using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace HimeLib
{
    public class LoadPNG
    {
        public static async Task<Texture2D> LoadFolderRandomAsync(string folderPath){
            if (Directory.Exists(folderPath))
            {
                string[] pngFiles = Directory.GetFiles(folderPath, "*.png");

                if (pngFiles.Length > 0)
                {
                    return await LoadFileAsync(pngFiles[Random.Range(0, pngFiles.Length)]);
                }
                else {
                    Debug.LogError("No PNG files found in the specified folder.");
                }
            }
            else {
                Debug.LogError("The specified folder path does not exist.");
            }

            return null;
        }

        public static async Task<Texture2D> LoadFileAsync(string filePath){
            byte[] fileData = await Task.Run(() => File.ReadAllBytes(filePath));
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) // 自动解析 PNG 文件
            {
                //Debug.Log("Image loaded successfully!");
                return texture;
            }
            return null;
        }

        // 保留原有的同步方法以保持向后兼容
        public static Texture2D LoadFolderRandom(string folderPath){
            if (Directory.Exists(folderPath))
            {
                string[] pngFiles = Directory.GetFiles(folderPath, "*.png");

                if (pngFiles.Length > 0)
                {
                    return LoadFile(pngFiles[Random.Range(0, pngFiles.Length)]);
                }
                else {
                    Debug.LogError("No PNG files found in the specified folder.");
                }
            }
            else {
                Debug.LogError("The specified folder path does not exist.");
            }

            return null;
        }

        public static Texture2D LoadFile(string filePath){
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) // 自动解析 PNG 文件
            {
                //Debug.Log("Image loaded successfully!");
                return texture;
            }
            return null;
        }

        public static string GetFileNameRandom(string folderPath){
            if (Directory.Exists(folderPath))
            {
                string[] pngFiles = Directory.GetFiles(folderPath, "*.png");

                if (pngFiles.Length > 0)
                {
                    return pngFiles[Random.Range(0, pngFiles.Length)];
                }
                else {
                    Debug.LogError("No PNG files found in the specified folder.");
                }
            }
            else {
                Debug.LogError("The specified folder path does not exist.");
            }
            return "";
        }

        public static System.DateTime FileNameToDate(string n){
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(n);

            // Assuming the filename (without extension) is in the format "yyyyMMddHHmmss"
            try
            {
                return System.DateTime.ParseExact(fileNameWithoutExtension, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (System.FormatException)
            {
                Debug.LogError($"Failed to parse date from filename: {n}");
                return System.DateTime.MinValue; // Default value in case of failure
            }
        }
    }
}