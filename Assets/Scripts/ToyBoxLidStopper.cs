using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyBoxLidStopper : MonoBehaviour
{
    Rigidbody rb;
    HingeJoint hinge;
    Quaternion maxQuat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        maxQuat = Quaternion.Euler(hinge.limits.max, 0, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dot = Mathf.Abs(Quaternion.Dot(rb.rotation, maxQuat));
        Debug.Log(dot);
        if (dot < 0.0001f)
        {
            rb.isKinematic = true;
            Destroy(hinge);
            Destroy(this);
        }
    }
}
