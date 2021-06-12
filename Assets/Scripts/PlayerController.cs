using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    [Range(0, 1)]
    public LayerMask collisionLayers;
    public float rayDistance = 1f;
    public float walkHeight = 0.5f;
    public float walkSpeed = 1f;
    public float heightAlignmentSpeed = 3f;
    public float rotationAlignmentSpeed = 5f;

    Spring spring;

    Controllable head, tail;
    Controllable controlled;

    float hInputSum;

    // Start is called before the first frame update
    void Awake()
    {
        spring = GetComponent<Spring>();
    }

    private void Start()
    {
        head = new Controllable(spring.segments[0], collisionLayers, Vector3.up, Vector3.right);
        tail = new Controllable(spring.segments[spring.segments.Count - 1], collisionLayers, Vector3.down, Vector3.left);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (controlled == null || controlled == tail)
            {
                controlled = head;
                head.AssumeControl();
                tail.RemoveControl();
                spring.SetStiffnessPerJoint(50, spring.defaultStiffness);
            }
            else
            {
                controlled = tail;
                tail.AssumeControl();
                head.RemoveControl();
                spring.SetStiffnessPerJoint(spring.defaultStiffness, 50);
            }
        }

        hInputSum += Input.GetAxisRaw("Horizontal") * Time.deltaTime;
    }

    void FixedUpdate()
    {
        head.DoRaycastTests(rayDistance, walkHeight, heightAlignmentSpeed, rotationAlignmentSpeed);
        tail.DoRaycastTests(rayDistance, walkHeight, heightAlignmentSpeed, rotationAlignmentSpeed);
        controlled?.ApplyMovement(hInputSum, walkSpeed);

        hInputSum = 0;
    }

    public Vector3 GetAveragePosition()
    {
        return (head.position + tail.position) / 2f;
    }
}
