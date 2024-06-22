using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace bluejayvrstudio
{
    public class VRInput : BaseInput
    {
        public Camera eventCamera = null;

        public OVRInput.Button clickButton = OVRInput.Button.SecondaryIndexTrigger;
        public OVRInput.Controller controller = OVRInput.Controller.All;

        public GameObject RaycastSurface;
        public GameObject cursor;
        public Transform PointerAnchor;
        public LayerMask layerMask;

        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;

        public GameObject canvas;

        protected override async void Start()
        {
            // GetComponent<BaseInputModule>().inputOverride = this;
            while (true)
            {
                try
                {
                    EventSystem.current.currentInputModule.inputOverride = this;
                    Debug.Log("Overriden");
                    break;
                }
                catch
                {
                    await Task.Yield();
                }
            }
        }

        void Update()
        {
            m_LastMousePosition = m_MousePosition;
            m_MousePosition = cursor.transform.position;
        }

        public override bool GetMouseButton(int button)
        {
            if (button == 0) return OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
            else if (button == 1) return OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
            else if (button == 2) return OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown);
            else return false;
        }

        public override bool GetMouseButtonDown(int button)
        {
            m_MousePosition = m_LastMousePosition = cursor.transform.localPosition;

            if (button == 0) return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
            else if (button == 1) return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
            else if (button == 2) return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickDown);
            else return false;
        }
        public override bool GetMouseButtonUp(int button)
        {
            if (button == 0) return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);
            else if (button == 1) return OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger);
            else if (button == 2) return OVRInput.GetUp(OVRInput.Button.SecondaryThumbstickDown);
            else return false;
        }

        public override bool mousePresent
        {
            get { return true; }
        }

        public override Vector2 mousePosition
        {
            get
            {
                return new Vector2(eventCamera.pixelWidth /2, eventCamera.pixelHeight / 2);
            }
        }
    }
}