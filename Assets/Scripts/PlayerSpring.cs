using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerSpring : MonoBehaviour
{
    [Header("Spring")]
    public GameObject springSegment;
    [Range(0, 30)]
    public int numSegments = 5;
    [Range(0, 1)]
    public float totalSpringMass = 0.1f;

    [SerializeField, HideInInspector]
    private List<GameObject> segments = new List<GameObject>();
    private Dictionary<GameObject, ConfigurableJoint> segmentJoints;
    private Dictionary<GameObject, Rigidbody> segmentRigidbodies;

    [Header("Walking")]
    [Range(0, 1)]
    public float walkHeight = 0.5f;

    [Header("Grabbing")]
    [Tooltip("In pixels")]
    public float maxGrabDistance = 50;
    public float grabForceMultiplier = 1f;
    public float maxGrabForce = 1f;

    [Header("Visual")]
    public float numTwists = 20;
    [Range(1, 30)]
    public int linePointsPerSegment = 1;
    LineRenderer springPath;

    Camera mainCam;
    GameObject segmentA, segmentB;
    GameObject controlled;
    Rigidbody grabTarget;

    public void OnValidate()
    {
        SetSegments();
        SetMassPerSpring();
    }

    private void SetSegments()
    {
        while (segments.Count > 0 && segments[segments.Count - 1] == null)
        {
            segments.RemoveAt(segments.Count - 1);
        }
        while (numSegments < segments.Count)
        {
            StartCoroutine(DestroySegment(segments[segments.Count - 1]));
            segments.RemoveAt(segments.Count - 1);
        }
        while (numSegments > segments.Count)
        {
            GameObject newSegment = (GameObject)PrefabUtility.InstantiatePrefab(springSegment);
            newSegment.transform.parent = transform;
            newSegment.transform.localPosition = Vector3.zero;
            newSegment.name = "Spring " + (segments.Count + 1);
            if (segments.Count > 0)
            {
                GameObject previousSegment = segments[segments.Count - 1];
                Rigidbody curBody = newSegment.GetComponent<Rigidbody>();
                previousSegment.GetComponent<ConfigurableJoint>().connectedBody = curBody;
            }
            segments.Add(newSegment);
        }
    }

    IEnumerator DestroySegment(GameObject g)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(g);
    }

    private void SetMassPerSpring()
    {
        float massPerSpring = totalSpringMass / numSegments;
        foreach (var segment in segments)
            segment.GetComponent<Rigidbody>().mass = massPerSpring;
    }

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

    private void LateUpdate()
    {
        CalculateSpringPath();
    }

    private void Grab(GameObject target)
    {
        grabTarget = target.GetComponent<Rigidbody>();
    }

    private void Ungrab()
    {
        grabTarget = null;
    }

    private void CalculateSpringPath()
    {
        Vector3 startVector = Vector3.right * 0.5f;
        float curAngle = 0;
        float twistPerPoint = numTwists * 360 / (segments.Count * linePointsPerSegment);
        int index = 0;
        springPath.positionCount = (segments.Count - 1) * linePointsPerSegment + 1;

        for (int i = 0; i < segments.Count; ++i)
        {
            Transform segment = segments[i].transform;

            Quaternion rotation = segment.rotation * Quaternion.Euler(0, curAngle, 0);
            Vector3 newPoint = rotation * startVector + segment.position;
            springPath.SetPosition(index++, newPoint);

            curAngle += twistPerPoint;

            if (i < segments.Count - 1)
            {
                Transform next = segments[i + 1].transform;

                for (int j = 1; j < linePointsPerSegment; ++j)
                {
                    float frac = (float)j / linePointsPerSegment;

                    Quaternion slerpRotation = Quaternion.Slerp(segment.rotation, next.rotation, frac);
                    slerpRotation = slerpRotation * Quaternion.Euler(0, curAngle, 0);
                    Vector3 lerpPosition = Vector3.Lerp(segment.position, next.position, frac);
                    newPoint = slerpRotation * startVector + lerpPosition;
                    springPath.SetPosition(index++, newPoint);

                    curAngle += twistPerPoint;
                }
            }
        }
    }
}
