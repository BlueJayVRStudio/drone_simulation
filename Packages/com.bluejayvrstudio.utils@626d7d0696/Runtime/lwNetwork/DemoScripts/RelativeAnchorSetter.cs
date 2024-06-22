using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using bluejayvrstudio;

public class RelativeAnchorSetter : MonoBehaviour
{
    [SerializeField] GameObject CenterSetter;
    [SerializeField] GameObject DirectionSetter;

    void Update()
    {
        transform.position = CenterSetter.transform.position;
        transform.rotation = Quaternion.LookRotation(CustomM.XZPlane(DirectionSetter.transform.position-CenterSetter.transform.position), Vector3.up);

        CenterSetter.transform.rotation = transform.rotation;
    }
}
