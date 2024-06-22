// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlaneVectors : MonoBehaviour
// {
//     public Vector3 Normal;
//     public Vector3 Away;

//     public bool _Capsule = false;
//     public bool _Dome = false;
//     public bool _FollowTorso = false;
//     public GameObject PlayerMovementCapsule;
//     public GameObject Torso;

//     public Controller PlayerController;

//     void Start()
//     {
//         float angle = transform.eulerAngles.y;
//         Normal = new Vector3(-Mathf.Cos(Mathf.PI*angle/180.0f),0,Mathf.Sin(Mathf.PI*angle/180.0f));
//         Normal = Normal.normalized;
//         Away = -Normal;
//         // Debug.Log(Away.x + ", " + Away.y + ", " + Away.z);
//         // This is equivalent
//         // Debug.Log((-transform.up).x + ", " + (-transform.up).y + ", " + (-transform.up).z);
//         // Debug.Log("Yeet");
//     }

//     void Update()
//     {
//         if (_Capsule || _Dome)
//         {
//             if (_FollowTorso) transform.position = new Vector3(Torso.transform.position.x, transform.position.y, Torso.transform.position.z);
//             Away = (transform.position-PlayerMovementCapsule.transform.position);
//             Away -= new Vector3(0,Away.y,0);
//             Away = Away.normalized;
//             Normal = -Away;
//             if (_Dome)
//             {
//                 Away = -Away;
//                 Normal = -Normal;
//             }
//         }
//     }

//     void OnDestroy()
//     {
//         if (transform.name == "WallAgentCapsule" && PlayerController != null) PlayerController.Colliding.Remove(transform.gameObject);
//     }

// }
