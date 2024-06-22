using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using bluejayvrstudio;
public class OVR_PC_InputHandling : TempSingleton<OVR_PC_InputHandling>
{
    public BaseInputModule PC;

    public bool SwitchCamera = false;
    public Camera XrCamera;
    public Camera RaycastCamera;
    public Camera camera;
    public Canvas canvas;
    public Mask mask;

    public GameObject Cursor;
    public Transform RightHandAnchor;

    public GraphicRaycaster Raycaster;
    public EventSystem eventSystem;

    public Transform World_UI_Position;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    void Awake()
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        XrCamera = FindObjectOfType<XRCamera>().transform.GetComponent<Camera>();
    }

    // IF VR!
#else
    void Awake()
    {
        canvas.renderMode = RenderMode.WorldSpace;
        // World_UI_Position = FindObjectOfType<UI_Anchor>().gameObject.transform;
        XrCamera = FindObjectOfType<XRCamera>().transform.GetComponent<Camera>();
        canvas.worldCamera = RaycastCamera;
        canvas.transform.SetParent(World_UI_Position);
        canvas.GetComponent<RectTransform>().localScale = new Vector3(1,1,1) * 0.0005f;
        canvas.GetComponent<RectTransform>().localPosition = new Vector2(0,0);
        canvas.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0,0,0));
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 2000);
        canvas.transform.localPosition += new Vector3(0,0,0);

        // FOR DEBUG
        // canvas.transform.SetParent(null);

        mask.enabled = true;
    }
#endif

    // FOR DEBUG
    void Update()
    {
        // Debug.Log(eventSystem.isFocused);
    }
}