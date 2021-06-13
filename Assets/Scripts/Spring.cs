using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spring : MonoBehaviour
{
    [Header("Spring")]
    public GameObject springSegment;
    [Range(0, 30)]
    public int numSegments = 5;
    [Range(0, 1)]
    public float totalSpringMass = 0.1f;

    [HideInInspector]
    public List<GameObject> segments = new List<GameObject>();
    public Dictionary<GameObject, ConfigurableJoint> segmentJoints;
    public Dictionary<GameObject, Rigidbody> segmentRigidbodies;
    float _defaultStiffness;
    float _springRadius;
    public float defaultStiffness { get => _defaultStiffness; }
    public float springRadius { get => _springRadius; }

    [Header("Visual")]
    public float numTwists = 20;
    [Range(1, 30)]
    public int linePointsPerSegment = 1;
    LineRenderer springPath;

    public void OnValidate()
    {
        SetSegments();
        SetMassPerSpring(totalSpringMass);
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
                ConfigurableJoint previousJoint = previousSegment.GetComponent<ConfigurableJoint>();
                previousJoint.connectedBody = curBody;
                newSegment.transform.localPosition += -previousJoint.connectedAnchor * previousJoint.transform.localScale.y * segments.Count;
            }
            segments.Add(newSegment);
        }
    }

    IEnumerator DestroySegment(GameObject g)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(g);
    }

    public void SetMassPerSpring(float mass)
    {
        totalSpringMass = mass;
        float massPerSpring = totalSpringMass / numSegments;
        foreach (var segment in segments)
            segment.GetComponent<Rigidbody>().mass = massPerSpring;
    }

    public void SetStiffnessPerJoint(float startStiffness, float endStiffness)
    {
        for (int i = 0; i < segments.Count - 1; ++i)
        {
            float frac = (float)i / (segments.Count - 2);
            float spring = Mathf.Lerp(startStiffness, endStiffness, frac);
            var segmentJoint = segmentJoints[segments[i]];
            var drive = segmentJoint.angularYZDrive;
            drive.positionSpring = spring;
            segmentJoint.angularYZDrive = drive;
        }
    }

    public void ResetStiffness()
    {
        SetStiffnessPerJoint(defaultStiffness, defaultStiffness);
    }

    void Awake()
    {
        GameObject lastSegment = segments[segments.Count - 1];
        ConfigurableJoint lastJoint = lastSegment.GetComponent<ConfigurableJoint>();
        lastJoint.connectedAnchor = lastSegment.transform.position;
        Destroy(lastJoint);

        segmentJoints = new Dictionary<GameObject, ConfigurableJoint>();
        segmentRigidbodies = new Dictionary<GameObject, Rigidbody>();
        foreach (var segment in segments)
        {
            segmentJoints.Add(segment, segment.GetComponent<ConfigurableJoint>());
            segmentRigidbodies.Add(segment, segment.GetComponent<Rigidbody>());
        }
        _defaultStiffness = segmentJoints[segments[0]].angularYZDrive.positionSpring;
        _springRadius = segments[0].GetComponent<BoxCollider>().bounds.size.x / 2f;

        springPath = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        CalculateSpringPath();
    }

    private void CalculateSpringPath()
    {
        Vector3 startVector = Vector3.right * springRadius;
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
