using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HimeLib
{
    public class Vector2Viewer : MonoBehaviour
    {
        public ReadEmuSignal readEmuSignal;
        public Transform pointParent;
        public float pointSize = 0.1f;
        public Color pointColor = Color.cyan;

        [SerializeField] List<Transform> pointPool = new List<Transform>();
        void Start()
        {
            if (readEmuSignal != null)
            {
                //readEmuSignal.OnReadVector2Signal += OnReadVector2Signal;
                readEmuSignal.OnReadEmuSignal += OnReadEmuSignal;
            }
        }

        void OnReadVector2Signal(List<Vector2> vector2Points)
        {
            for (int i = 0; i < vector2Points.Count; i++)
            {
                if (i >= pointPool.Count)
                {
                    GameObject newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    newPoint.transform.parent = pointParent;
                    newPoint.transform.localScale = Vector3.one * pointSize;
                    newPoint.transform.localPosition = Vector3.zero;
                    newPoint.transform.localRotation = Quaternion.identity;
                    newPoint.name = "point_" + i;
                    newPoint.GetComponent<Renderer>().material.color = pointColor;
                    pointPool.Add(newPoint.transform);
                }
                pointPool[i].position = vector2Points[i];
            }
        }

        void OnReadEmuSignal(List<Urg.DetectedLocation> emuLocations)
        {
            List<Vector2> vector2Points = new List<Vector2>();
            for (int i = 0; i < emuLocations.Count; i++)
            {
                vector2Points.Add(emuLocations[i].ToPosition2D());
            }
            OnReadVector2Signal(vector2Points);
        }
    }
}