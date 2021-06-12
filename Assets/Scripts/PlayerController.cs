using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    [Range(0, 1)]
    public float walkHeight = 0.5f;
    /*
    [Header("Grabbing")]
    [Tooltip("In pixels")]
    public float maxGrabDistance = 50;
    public float grabForceMultiplier = 1f;
    public float maxGrabForce = 1f;
    */

    Spring spring;
    GameObject segmentA, segmentB;

    // Start is called before the first frame update
    void Awake()
    {
        spring = GetComponent<Spring>();
    }

    private void Start()
    {
        spring.SetStiffnessPerJoint(50, spring.defaultStiffness);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    void Start()
    {
        mainCam = Camera.main;
        segmentA = segments[0];
        segmentB = segments[segments.Count - 1];

        ConfigurableJoint lastJoint = segmentB.GetComponent<ConfigurableJoint>();
        Destroy(lastJoint);

        segmentJoints = new Dictionary<GameObject, ConfigurableJoint>();
        segmentRigidbodies = new Dictionary<GameObject, Rigidbody>();
        foreach (var segment in segments)
        {
            segmentJoints.Add(segment, segment.GetComponent<ConfigurableJoint>());
            segmentRigidbodies.Add(segment, segment.GetComponent<Rigidbody>());
        }

        controlled = segmentA;
        segmentRigidbodies[segmentA].isKinematic = true;

        springPath = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 segmentA_screenCoords = mainCam.WorldToScreenPoint(segmentA.transform.position);
            Vector3 segmentB_screenCoords = mainCam.WorldToScreenPoint(segmentB.transform.position);
            float segmentA_mouseDistance = Vector3.Distance(segmentA_screenCoords, Input.mousePosition);
            float segmentB_mouseDistance = Vector3.Distance(segmentB_screenCoords, Input.mousePosition);
            bool segmentA_inGrabRange = segmentA_mouseDistance <= maxGrabDistance;
            bool segmentB_inGrabRange = segmentB_mouseDistance <= maxGrabDistance;
            if (segmentA_inGrabRange && segmentA_mouseDistance < segmentB_mouseDistance)
                Grab(segmentA);
            else if (segmentB_inGrabRange && segmentB_mouseDistance < segmentA_mouseDistance)
                Grab(segmentB);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Ungrab();
        }

        Vector3 newCamPos = GetAveragePosition();
        newCamPos.z = mainCam.transform.position.z;
        mainCam.transform.position = newCamPos;
    }

    void FixedUpdate()
    {
        if (grabTarget != null)
        {
            Vector3 target_screenCoords = mainCam.WorldToScreenPoint(grabTarget.position);
            Vector3 grabForce = Input.mousePosition - target_screenCoords;
            grabForce = grabForce * grabForceMultiplier;
            grabForce = Vector3.ClampMagnitude(grabForce, maxGrabForce);
            grabTarget.AddForce(grabForce * Time.fixedDeltaTime);
        }
    }

    private void Grab(GameObject target)
    {
        grabTarget = target.GetComponent<Rigidbody>();
    }

    private void Ungrab()
    {
        grabTarget = null;
    }
    */
    public Vector3 GetAveragePosition()
    {
        return (segmentA.transform.position + segmentB.transform.position) / 2f;
    }
}
