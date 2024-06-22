// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using bluejayvrstudio;

// public class lwNetworkTransform : MonoBehaviour
// {
//     Vector3 last_position;
//     Vector3 current_position;
//     Quaternion last_rotation;
//     Quaternion current_rotation;

//     Queue<Vector3> positions;
//     Queue<Quaternion> rotations;

//     float timer;
//     float interval;
//     float AvgFactor = 6.0f;

//     void Awake() 
//     {
//         // twice the regular interval to average two ticks
//         interval = AvgFactor / NetworkInit.CurrInst.tickrate;
//         timer = 0;

//         positions = new();
//         rotations = new();
//     }

//     // Try averaging two ticks
//     void Update()
//     {
//         if (NetworkInit.CurrInst.IsServer == false)
//         {
//             timer += Time.deltaTime;

//             transform.localPosition = Vector3.Lerp(last_position, current_position, timer / interval);
//             transform.localRotation = Quaternion.Slerp(last_rotation, current_rotation, timer / interval);

//             if (timer >= interval)
//             {
//                 last_position = current_position;
//                 last_rotation = current_rotation;

//                 List<Vector3> positionsToAvg = new();
//                 List<Quaternion> rotationsToAvg = new();

//                 if (positions.Count > 0) {
//                     while (positions.Count > 0) {
//                         positionsToAvg.Add(positions.Dequeue());
//                         if (positionsToAvg.Count == AvgFactor) break;
//                     }
//                     while (rotations.Count > 0) {
//                         if (rotationsToAvg.Count == AvgFactor) break;
//                         rotationsToAvg.Add(rotations.Dequeue());
//                     }

//                     current_position = CustomM.AvgVector3(positionsToAvg.ToArray());
//                     current_rotation = CustomM.AvgQuaternions(rotationsToAvg.ToArray());
//                 }

//                 timer -= interval;
//             }
            
//         }
//     }

//     public void set_transform(Vector3 position, Quaternion rotation) {
//         positions.Enqueue(position);
//         rotations.Enqueue(rotation);
//     }

// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lwNetworkTransform : MonoBehaviour
{
    Vector3 last_position;
    Vector3 current_position;
    Quaternion last_rotation;
    Quaternion current_rotation;

    float timer = 0;

    void Update()
    {
        if (NetworkInit.CurrInst.IsServer == false)
        {
            transform.localPosition = Vector3.Lerp(last_position, current_position, timer / (1.0f / NetworkInit.CurrInst.tickrate));
            transform.localRotation = Quaternion.Slerp(last_rotation, current_rotation, timer / (1.0f / NetworkInit.CurrInst.tickrate));
            timer += Time.deltaTime;
        }
    }

    public void set_transform(Vector3 position, Quaternion rotation) {
        last_position = current_position;
        current_position = position;

        last_rotation = current_rotation;
        current_rotation = rotation;
        timer = 0;
    }

}
