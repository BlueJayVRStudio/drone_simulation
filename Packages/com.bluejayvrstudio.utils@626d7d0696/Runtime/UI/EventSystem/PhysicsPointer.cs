using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using bluejayvrstudio;
public class PhysicsPointer : TempSingleton<PhysicsPointer>
{
    private EventSystem eventSystem;
    
    public float defaultLength = 0.0f;
    public PhysicsRaycaster physicsRaycaster;
    public GraphicRaycaster graphicRaycaster;
    public GameObject Cursor;
    public VRInput vrInput;

    private LineRenderer lineRenderer = null;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        physicsRaycaster = GetComponent<PhysicsRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        vrInput = FindObjectOfType<VRInput>();
    }

    private void Update()
    {
        UpdateLength();
    }

    private void UpdateLength()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, CalculateEnd());
    }

    private Vector3 CalculateEnd()
    {
        List<RaycastResult> results = CreateForwardRaycast();

        Vector3 endPosition = DefaultEnd(defaultLength);

        if (results.Count > 0)
        {
            endPosition = results[0].worldPosition;
            Cursor.transform.position = endPosition;
        }

        return endPosition;
    }

    public List<RaycastResult> CreateForwardRaycast()
    {
        PointerEventData m_PointerEventData;
        m_PointerEventData = new PointerEventData(eventSystem);
        m_PointerEventData.position = vrInput.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        graphicRaycaster.Raycast(m_PointerEventData, results);

        return results;
    }

    private Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }


}
