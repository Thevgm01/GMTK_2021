using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerFollower : MonoBehaviour
{
    public PlayerController player;
    [Min(0)]
    public float cameraSpeed;

    void LateUpdate()
    {
        Vector3 newPos = player.GetAveragePosition();
        newPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
