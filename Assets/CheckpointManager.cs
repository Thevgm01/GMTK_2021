using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    PlayerController player;
    int checkpointIndex = 0;

    // Start is called before the first frame update
    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = player.GetAveragePosition();
        if (checkpointIndex < transform.childCount && position.x >= transform.GetChild(checkpointIndex + 1).position.x)
        {
            AdvanceCheckpoint();
        }
    }

    private void AdvanceCheckpoint()
    {
        ++checkpointIndex;
    }

    public Vector3 GetCurCheckpoint()
    {
        return transform.GetChild(checkpointIndex).position;
    }
}
