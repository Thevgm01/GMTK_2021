using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable
{
    public static float rayDistance, walkHeight, lockHeight, walkSpeed, heightAlignmentSpeed, rotationAlignmentSpeed;

    private enum State
    {
        Free,
        Locked,
        Controlled
    }

    private GameObject segment;
    private Transform transform;
    private Rigidbody rb;
    private float radius;
    private LayerMask layers;
    private Vector3 localUp;
    private Vector3 localRight;
    private State state;

    private Vector3 overallMove;
    private bool canMoveLeft, canMoveRight;
    private const int LEFT = 0, BOTTOMLEFT = 1, RIGHT = 2, BOTTOMRIGHT = 3;
    private float minDot = 0.8f;

    public Vector3 position { get => rb.position; }

    Controllable other;

    public Controllable(GameObject segment, LayerMask layers, Vector3 localUp, Vector3 localRight)
    {
        this.segment = segment;
        this.transform = segment.transform;
        this.rb = segment.GetComponent<Rigidbody>();
        this.radius = segment.GetComponent<BoxCollider>().bounds.size.x / 2f;
        this.layers = layers;
        this.localUp = localUp;
        this.localRight = localRight;
        this.state = State.Free;
    }

    public void SetOther(Controllable other)
    {
        this.other = other;
    }

    public void DoRaycastTests()
    {
        if (state == State.Free)
        {
            return;
        }

        // Gather raycast hit info
        bool[] hits = new bool[4];
        float[] hitDistances = new float[4];
        Vector3[] hitPoints = new Vector3[4];
        Vector3[] hitNormals = new Vector3[4];
        bool anyHit = false;
        int numHits = 0;

        for (int side = 0; side <= 1; side++)
        {
            float sideLR = side * 2f - 1f;
            Vector3 raycastSide = localRight * radius * sideLR;
            for (int dir = 0; dir <= 1; dir++)
            {
                Vector3 raycastPosition = transform.rotation * raycastSide * (dir == 0 ? 1f : 0.8f);
                raycastPosition += transform.position;
                Vector3 raycastDirection = dir == 0 ? localRight * sideLR : -localUp;
                raycastDirection = transform.rotation * raycastDirection;

                Physics.Raycast(raycastPosition, raycastDirection, out var hit, rayDistance, layers);
                bool rayHit = hit.distance > 0;
                if (rayHit)
                {
                    int index = side * 2 + dir;
                    hits[index] = true;
                    hitDistances[index] = hit.distance;
                    hitPoints[index] = hit.point;
                    hitNormals[index] = hit.normal;
                    anyHit = true;
                    ++numHits;
                }
                Debug.DrawRay(
                    raycastPosition,
                    raycastDirection * (hit.distance > 0 && hit.distance < 1 ? hit.distance : 1),
                    hit.collider != null ? Color.green : Color.white);
            }
        }

        // Move and rotate the segment
        rb.isKinematic = anyHit;
        if (anyHit)
        {
            Vector3 averageBottomPoint = hitPoints[BOTTOMLEFT] + hitPoints[BOTTOMRIGHT];
            float averageBottomDistance = hitDistances[BOTTOMLEFT] + hitDistances[BOTTOMRIGHT];
            Vector3 averageBottomNormal = hitNormals[BOTTOMLEFT] + hitNormals[BOTTOMRIGHT];
            if (hits[BOTTOMLEFT] && hits[BOTTOMRIGHT])
            {
                averageBottomPoint /= 2f;
                averageBottomDistance /= 2f;
                averageBottomNormal /= 2f;
            }
            bool[] validNormals = new bool[4];
            for (int i = 0; i < 4; ++i)
            {
                validNormals[i] = CheckUpDot(hitNormals[i]);
            }

            bool canHugWalls = other.IsLocked();

            // Movement
            // If either of the bottom probes hit a surface, move the center of the segment to just above said surface
            if (hits[BOTTOMLEFT] || hits[BOTTOMRIGHT])
            {
                float desiredHeight = IsLocked() ? lockHeight : walkHeight;
                Vector3 desiredPosition = transform.position - transform.rotation * localUp * (averageBottomDistance - desiredHeight);
                overallMove += (desiredPosition - rb.position) * heightAlignmentSpeed;
            }
            else if (hits[LEFT] || hits[RIGHT])
            {
                overallMove += new Vector3(0, -heightAlignmentSpeed, 0);
            }

            canMoveLeft = !hits[LEFT] || hitDistances[LEFT] > 0.2f;
            canMoveRight = !hits[RIGHT] || hitDistances[RIGHT] > 0.2f;

            // Rotation
            float angle = 0;
            if (hits[LEFT] &&        (validNormals[LEFT]        || canHugWalls)) 
                angle -= rayDistance - hitDistances[LEFT];

            if (hits[BOTTOMLEFT] &&  (validNormals[BOTTOMLEFT]  || canHugWalls)) 
                angle -= rayDistance - hitDistances[BOTTOMLEFT];

            if (hits[RIGHT] &&       (validNormals[RIGHT]       || canHugWalls)) 
                angle += rayDistance - hitDistances[RIGHT];

            if (hits[BOTTOMRIGHT] && (validNormals[BOTTOMRIGHT] || canHugWalls)) 
                angle += rayDistance - hitDistances[BOTTOMRIGHT];

            rb.MoveRotation(Quaternion.Euler(0, 0, rb.rotation.eulerAngles.z + angle * rotationAlignmentSpeed * 50));
        }
    }

    bool CheckUpDot(Vector3 test)
    {
        return Vector3.Dot(test, Vector3.up) > minDot;
    }

    public void Free()
    {
        state = State.Free;
        rb.isKinematic = false;
    }

    public void AssumeControl(bool force)
    {
        if (force || state == State.Free)
        {
            state = State.Controlled;
        }
    }

    public void ReleaseControl()
    {
        if (state == State.Controlled)
        {
            Free();
        }
    }

    public void AddHInput(float input)
    {
        if (state == State.Controlled)
        {
            float horizontal = input * walkSpeed;
            if ((horizontal < 0 && canMoveLeft) || (horizontal > 0 && canMoveRight))
                overallMove += transform.rotation * localRight * horizontal;
        }
    }

    public void ApplyMovement()
    {
        rb.MovePosition(rb.position + overallMove);
        overallMove = Vector3.zero;
    }

    public void Lock()
    {
        state = State.Locked;
    }

    public bool IsLocked()
    {
        return state == State.Locked;
    }

    public bool IsFree()
    {
        return state == State.Free;
    }

    public void Jump(float impulse)
    {
        if (state != State.Free)
        {
            Free();
            rb.AddForce(transform.rotation * localUp * impulse * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }
}
