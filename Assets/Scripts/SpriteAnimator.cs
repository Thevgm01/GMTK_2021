using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    SpriteRenderer sr;

    public SpriteList spriteList;
    int spriteIndex = 0;
    float frameTime = 0;

    // Start is called before the first frame update
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        frameTime += Time.deltaTime;
        if (frameTime > 1f / 12f)
        {
            frameTime -= 1f / 12f;
            spriteIndex = (spriteIndex + 1) % spriteList.sprites.Length;
            sr.sprite = spriteList.sprites[spriteIndex];
            Debug.Log(spriteIndex);
        }
    }
}
