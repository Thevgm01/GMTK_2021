using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable
{
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

    public Vector3 position { get => transform.position; }

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

    public void DoRaycastTests(float rayDistance, float walkHeight, float heightAlignmentSpeed, float rotationAlignmentSpeed)
    {
        if (state == State.Free)
        {
            return;
        }

        heightAlignmentSpeed *= Time.fixedDeltaTime;
        rotationAlignmentSpeed *= Time.fixedDeltaTime;

        const int LEFT = 0, BOTTOMLEFT = 1, RIGHT = 2, BOTTOMRIGHT = 3;
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
                Vector3 raycastPosition = transform.rotation * raycastSide;
                raycastPosition += transform.position;
                Vector3 raycastDirection = dir == 0 ? localRight * sideLR : -localUp;
                raycastDirection = transform.rotation * raycastDirection;

                Physics.Raycast(raycastPosition, raycastDirection, out var hit, rayDistance, layers);
                bool rayHit = hit.distance > 0;
                if (rayHit)
                {
                    int index = side * 2 + dir;
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

        rb.isKinematic = anyHit;
        if (anyHit)
        {
            bool bothBottomTouching = hitDistances[BOTTOMLEFT] > 0 && hitDistances[BOTTOMRIGHT] > 0;
            bool eitherBottomTouching = hitDistances[BOTTOMLEFT] > 0 || hitDistances[BOTTOMRIGHT] > 0;

            Vector3 averageBottomPoint = hitPoints[BOTTOMLEFT] + hitPoints[BOTTOMRIGHT];
            for (int i = 0; i < hitPoints.Length; ++i)
                averageBottomPoint += hitPoints[i];
            averageBottomPoint /= (float)numHits;

            if (averageBottomPoint.sqrMagnitude > 0)
            {
                float desiredY = averageBottomPoint.y + walkHeight;
                Vector3 desiredPosition = new Vector3(rb.position.x, desiredY, rb.position.z);
                Vector3 newPosition = Vector3.Lerp(rb.position, desiredPosition, heightAlignmentSpeed);
                rb.MovePosition(newPosition);
            }

            if (eitherBottomTouching)
            {
                Vector3 averageNormal = hitNormals[BOTTOMLEFT] + hitNormals[BOTTOMRIGHT];
                if (bothBottomTouching)
                    averageNormal /= 2f;

                float dot = Vector3.Dot(transform.rotation * localUp, averageNormal) * 90 - 90;
                Quaternion desiredRotation = Quaternion.Euler(0, 0, dot);
                Quaternion newRotation = Quaternion.Slerp(rb.rotation, desiredRotation, rotationAlignmentSpeed);
                rb.MoveRotation(newRotation);
            }
            else
            {
                if (hitDistances[LEFT] > 0) rb.MoveRotation(Quaternion.Euler(0, 0, rb.rotation.eulerAngles.z - rotationAlignmentSpeed * 50));
                if (hitDistances[RIGHT] > 0) rb.MoveRotation(Quaternion.Euler(0, 0, rb.rotation.eulerAngles.z + rotationAlignmentSpeed * 50));
            }
        }
    }

    public void RemoveControl()
    {
        if (state == State.Controlled)
        {
            state = State.Free;
            rb.isKinematic = false;
        }
    }

    public void AssumeControl()
    {
        if (state == State.Free)
        {
            state = State.Controlled;
        }
    }

    public void ApplyMovement(float input, float moveSpeed)
    {
        if (state == State.Controlled)
        {
            rb.MovePosition(rb.position + new Vector3(input * moveSpeed, 0, 0));
        }
    }
}
