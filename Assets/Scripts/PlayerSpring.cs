using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpring : MonoBehaviour
{
    public GameObject springSegment;
    [Range(0, 30)]
    public int numSegments = 5;

    [SerializeField, HideInInspector]
    private List<GameObject> segments = new List<GameObject>();

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
            GameObject newSegment = Instantiate(springSegment);
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

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
