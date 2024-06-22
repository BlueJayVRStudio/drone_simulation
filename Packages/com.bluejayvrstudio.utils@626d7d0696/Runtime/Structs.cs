using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace bluejayvrstudio
{
    [Serializable]
    public class SerializableContactPoint
    {
        public Vector3 position;
        public Vector3 normal;
        public float slope;
        public float separation;

        // Please refer to: https://docs.unity3d.com/ScriptReference/ContactPoint.html
        public SerializableContactPoint(ContactPoint contactPoint)
        {
            position = contactPoint.point;
            normal = contactPoint.normal;
            separation = contactPoint.separation;
        }

        public void CalculateSlope()
        {
            slope = 90.0f - 180.0f*Mathf.Atan(Mathf.Abs(normal.y)/new Vector3(normal.x,0,normal.z).magnitude) / Mathf.PI;
        }
    }


    [Serializable]
    public class SerializableList<T>
    {
        public List<T> Items;

        public SerializableList()
        {
            Items = new List<T>();
        }

        public SerializableList(List<T> items)
        {
            Items = items;
        }
    }

    [Serializable]
    public class SerializableListOfLists<T>
    {
        public List<SerializableList<T>> Items;

        public SerializableListOfLists()
        {
            Items = new List<SerializableList<T>>();
        }
        public SerializableListOfLists(List<List<T>> items)
        {
            Items = new List<SerializableList<T>>();

            foreach (var list in items)
            {
                Items.Add(new SerializableList<T>(list));
            }
        }

        public List<List<T>> ToListOfLists()
        {
            var listOfLists = new List<List<T>>();

            foreach (var serializableList in Items)
            {
                listOfLists.Add(serializableList.Items);
            }

            return listOfLists;
        }
    }
}
