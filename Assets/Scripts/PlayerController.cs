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
    public float lockHeight = 0.2f;
    public float walkSpeed = 1f;
    public float heightAlignmentSpeed = 3f;
    public float rotationAlignmentSpeed = 5f;
    public float jumpImpulse = 2;

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
        head.SetOther(tail);
        tail.SetOther(head);
        controlled = head;
    }

    // Update is called once per frame
    void Update()
    {
        Controllable.rayDistance = rayDistance;
        Controllable.walkHeight = walkHeight;
        Controllable.lockHeight = lockHeight;
        Controllable.walkSpeed = walkSpeed;
        Controllable.heightAlignmentSpeed = heightAlignmentSpeed * Time.fixedDeltaTime;
        Controllable.rotationAlignmentSpeed = rotationAlignmentSpeed * Time.fixedDeltaTime;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (controlled == tail)
            {
                controlled = head;
                head.AssumeControl(false);
                tail.ReleaseControl();

                if (tail.IsLocked()) SetStiffness(null);
                else SetStiffness(controlled);
            }
            else
            {
                controlled = tail;
                tail.AssumeControl(false);
                head.ReleaseControl();

                if (head.IsLocked()) SetStiffness(null);
                else SetStiffness(controlled);
            }
        }

        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        hInputSum += hAxis * Time.deltaTime;

        if (!controlled.IsLocked() && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
        {
            controlled.AssumeControl(false);
            SetStiffness(controlled);
        }

        if (vAxis < 0)
        {
            controlled.Lock();
        }
        else if (vAxis > 0)
        {
            controlled.AssumeControl(true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (controlled.IsFree())
            {
                head.Free();
                tail.Free();
            }
            else
            {
                controlled.Jump(jumpImpulse);
                SetStiffness(null);
            }
        }
    }

    void SetStiffness(Controllable front)
    {
        if (front == null) spring.ResetStiffness();
        else if (front == head) spring.SetStiffnessPerJoint(50, spring.defaultStiffness);
        else if (front == tail) spring.SetStiffnessPerJoint(spring.defaultStiffness, 50);
    }

    void FixedUpdate()
    {
        head.DoRaycastTests();
        tail.DoRaycastTests();
        controlled.AddHInput(hInputSum);
        head.ApplyMovement();
        tail.ApplyMovement();

        hInputSum = 0;
    }

    public Vector3 GetAveragePosition()
    {
        return (head.position + tail.position) / 2f;
    }
}
