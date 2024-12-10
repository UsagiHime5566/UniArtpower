using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HimeLib
{
    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> myList;
    }

    [System.Serializable]
    public class StringListWrapper : ListWrapper<string> { }

    public class Tools
    {
        public static GameObject RecursiveFindChild(Transform parent, string childName)
        {
            Debug.Log("trying to find " + childName);
            foreach (Transform child in parent)
            {
                Debug.Log("child is " + child.gameObject.name);
                if (child.name == childName)
                    return child.gameObject;
                else
                    return RecursiveFindChild(child, childName);
            }

            return null;
        }
    }
}

public static class ExtentTransform
{
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;

            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
}