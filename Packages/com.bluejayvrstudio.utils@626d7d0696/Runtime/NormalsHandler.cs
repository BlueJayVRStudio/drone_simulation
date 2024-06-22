using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System;

public class NormalsHandler : MonoBehaviour, ISerializationCallbackReceiver
{
    public Dictionary<GameObject, SerializableList<SerializableContactPoint>> ContactPointLists = new Dictionary<GameObject, SerializableList<SerializableContactPoint>>();

# region Dictionary Serialization/ Nested List Serialization
    public List<GameObject> _keys = new List<GameObject>();
    public SerializableListOfLists<SerializableContactPoint> _values = new SerializableListOfLists<SerializableContactPoint>();

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Items.Clear();

        foreach (var kvp in ContactPointLists)
        {
            _keys.Add(kvp.Key);
            _values.Items.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        ContactPointLists = new Dictionary<GameObject, SerializableList<SerializableContactPoint>>();

        for (int i = 0; i != Math.Min(_keys.Count, _values.Items.Count); i++)
            ContactPointLists.Add(_keys[i], _values.Items[i]);
    }
# endregion

    void OnCollisionEnter(Collision col)
    {
        Debug.Log(col.gameObject);
        UpdateContactPoints(col);
    }

    void OnCollisionStay(Collision col)
    {
        UpdateContactPoints(col);
    }

    [SerializeField] LayerMask layerMask;
    void UpdateContactPoints(Collision col)
    {
        if (((1<<col.transform.gameObject.layer) & layerMask) == 0) return;

        // Debug.Log("colliding");

        var ContactPoints = new ContactPoint[col.contactCount];
        col.GetContacts(ContactPoints);

        var ContactPointList = new SerializableList<SerializableContactPoint>();
        foreach(ContactPoint i in ContactPoints)
        {
            var SContactPoint = new SerializableContactPoint(i);
            ContactPointList.Items.Add(SContactPoint);
            SContactPoint.CalculateSlope();
        }
        ContactPointLists[col.gameObject] = ContactPointList;
    }

    void OnCollisionExit(Collision col)
    {
        // Debug.Log("Exiting");
        if (ContactPointLists.ContainsKey(col.gameObject)) ContactPointLists.Remove(col.gameObject);
    }
}
