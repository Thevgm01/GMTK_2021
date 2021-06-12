using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    [Range(0, 1)]
    public float walkHeight = 0.5f;

    Spring spring;

    private enum State
    {
        Free,
        Locked,
        Controlled
    }

    private class Controllable
    {
        private GameObject segment;
        private Transform transform;
        private Rigidbody rb;
        private float radius;
        private Vector3 localUp;
        private Vector3 localRight;
        private State state;

        public Vector3 position { get => transform.position; }

        public Controllable(GameObject segment, Vector3 localUp, Vector3 localRight)
        {
            this.segment = segment;
            transform = segment.transform;
            rb = segment.GetComponent<Rigidbody>();
            radius = segment.GetComponent<BoxCollider>().bounds.size.x / 2f;
            this.localUp = localUp;
            this.localRight = localRight;
            state = State.Free;
        }

        public void DoRaycastTests()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 raycastPosition = localRight * radius * side;
                for (int dir = 0; dir <= 1; dir++)
                {
                    Vector3 rotatedPosition = transform.rotation * raycastPosition;
                    rotatedPosition += transform.position;
                    Vector3 raycastDirection = dir == 0 ? raycastDirection = localRight * side : -localUp;
                    raycastDirection = transform.rotation * raycastDirection;
                    Debug.DrawRay(rotatedPosition, raycastDirection);
                }
            }
        }
    }

    Controllable head, tail;
    Controllable controlled;

    // Start is called before the first frame update
    void Awake()
    {
        spring = GetComponent<Spring>();
    }

    private void Start()
    {
        head = new Controllable(spring.segments[0], Vector3.up, Vector3.right);
        tail = new Controllable(spring.segments[spring.segments.Count - 1], Vector3.down, Vector3.left);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (controlled.Equals(head) || controlled == null)
            {
                controlled = head;
            }
            else
            {
                controlled = tail;
            }
        }
    }

    private void FixedUpdate()
    {
        head.DoRaycastTests();
        tail.DoRaycastTests();
        //Physics.Raycast(controlled.transform.position)
    }

    public Vector3 GetAveragePosition()
    {
        return (head.position + tail.position) / 2f;
    }
}
