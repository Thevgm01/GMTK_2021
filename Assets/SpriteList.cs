using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteList", menuName = "Custom/SpriteList", order = 1)]
public class SpriteList : ScriptableObject
{
    public Sprite[] sprites;
}
