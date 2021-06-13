using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundList", menuName = "Custom/SoundList", order = 1)]
public class SoundList : ScriptableObject
{
    public AudioClip[] sounds;
}
