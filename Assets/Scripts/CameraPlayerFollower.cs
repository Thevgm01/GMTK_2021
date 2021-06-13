using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerFollower : MonoBehaviour
{
    [Min(0)]
    public float cameraSpeed;

    PlayerController player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void LateUpdate()
    {
        Vector3 newPos = player.GetAveragePosition();
        newPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
