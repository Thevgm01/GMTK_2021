using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerSpring : MonoBehaviour
{
    public GameObject springSegment;
    [Range(0, 30)]
    public int numSegments = 5;

    [SerializeField, HideInInspector]
    private List<GameObject> segments = new List<GameObject>();

    [Header("Grabbing")]
    [Tooltip("In pixels")]
    public float maxGrabDistance = 50;
    public float grabForceMultiplier = 1f;
    public float maxGrabForce = 1f;


    Camera mainCam;
    GameObject segmentA, segmentB;
    Rigidbody grabTarget;

    public void OnValidate()
    {
        SetSegments();
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
            newSegment.name = "Spring " + numSegments;
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

    void Start()
    {
        mainCam = Camera.main;
        segmentA = segments[0];
        segmentB = segments[segments.Count - 1];
        ConfigurableJoint lastJoint = segmentB.GetComponent<ConfigurableJoint>();
        Destroy(lastJoint);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
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
            Debug.Log(grabForce.magnitude);
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
}
