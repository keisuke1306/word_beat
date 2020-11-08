using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedController : MonoBehaviour
{
    private Vector3 initPosition = new Vector3(0f, 0f, 0f);

    void FixedUpdate()
    {
        transform.position = initPosition;
    }
}