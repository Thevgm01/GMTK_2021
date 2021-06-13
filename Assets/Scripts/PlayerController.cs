using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject animator;
    public CharacterSounds headSounds, tailSounds;
    public SoundList slinkySounds;
    public AudioClip slinkyRunSound;
    private AudioSource slinkyRunSource;

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
    public float maxStretchLength;

    float startDistance;
    AudioSource curSquishPlayer;

    Spring spring;
    CheckpointManager checkpoints;

    Controllable head, tail;
    Controllable controlled;

    float hInputSum;

    // Start is called before the first frame update
    void Awake()
    {
        spring = GetComponent<Spring>();
        checkpoints = FindObjectOfType<CheckpointManager>();
    }

    private void Start()
    {
        head = new Controllable(spring.segments[0], collisionLayers, Vector3.up, Vector3.right);
        tail = new Controllable(spring.segments[spring.segments.Count - 1], collisionLayers, Vector3.down, Vector3.left);
        head.SetOther(tail);
        tail.SetOther(head);
        controlled = head;
        head.SetAnimator(Instantiate(animator));
        tail.SetAnimator(Instantiate(animator));
        slinkyRunSource = AudioHelper.PlayClip2D(slinkyRunSound, volume: 0, destroyWhenDone: false);
        slinkyRunSource.loop = true;
        head.SetSounds(headSounds, slinkyRunSource);
        tail.SetSounds(tailSounds, slinkyRunSource);
        tail.ReleaseControl();

        ResetCheckpoint();
    }

    void ResetCheckpoint()
    {
        transform.position = checkpoints.GetCurCheckpoint();
        spring.ResetPositions();
        controlled = head;
        head.Free();
        tail.Free();
        SetStiffness(controlled);
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
        Controllable.maxStretchLength = maxStretchLength;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCheckpoint();
        }

        if (Application.isEditor && Input.GetKey(KeyCode.R))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                checkpoints.DecreaseCheckpoint();
                ResetCheckpoint();
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                checkpoints.AdvanceCheckpoint();
                ResetCheckpoint();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (controlled == tail)
            {
                controlled = head;
                head.AssumeControl(false);
                tail.ReleaseControl();

                if (tail.IsFree() && !head.IsLocked()) SetStiffness(controlled);
                else SetStiffness(null);
            }
            else
            {
                controlled = tail;
                tail.AssumeControl(false);
                head.ReleaseControl();

                if (head.IsFree() && !tail.IsLocked()) SetStiffness(controlled);
                else SetStiffness(null);
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

        float distance = Vector3.Distance(head.position, tail.position);
        if (distance < startDistance * 0.95f && curSquishPlayer == null)
        {
            curSquishPlayer = AudioHelper.PlayRandomClip2DFromArray(slinkySounds.sounds, volume: 0.5f);
        }
        startDistance = distance;
    }

    public Vector3 GetAveragePosition()
    {
        return (head.position + tail.position) / 2f;
    }
}
