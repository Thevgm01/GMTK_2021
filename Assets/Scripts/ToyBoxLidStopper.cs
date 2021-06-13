using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyBoxLidStopper : MonoBehaviour
{
    Rigidbody rb;
    HingeJoint hinge;
    Quaternion maxQuat;

    bool awaitStop = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        maxQuat = Quaternion.Euler(hinge.limits.max, 0, 0);
    }

    void FixedUpdate()
    {
        if (rb.angularVelocity != Vector3.zero)
        {
            awaitStop = true;
        }
        else if (awaitStop)
        {
            rb.isKinematic = true;
            Destroy(hinge);
            Destroy(this);
        }
    }
}
