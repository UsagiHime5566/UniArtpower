#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoraLib {
    public class FindAssetTool : Editor
    {
        public static T FindAssetByName<T>(string name) where T : Object
        {
            var results = AssetDatabase.FindAssets(name);
            foreach (string guid in results)
            {
                T temp = (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T));
                return temp;
            }

            return default(T);
        }
    }
}
#endif