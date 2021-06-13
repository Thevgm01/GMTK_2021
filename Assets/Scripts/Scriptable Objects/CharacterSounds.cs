using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSounds", menuName = "Custom/CharacterSounds", order = 1)]
public class CharacterSounds : ScriptableObject
{
    public SoundList jumpSounds, lockSounds;
}
