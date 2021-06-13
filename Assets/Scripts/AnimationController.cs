using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public SpriteList idleLegSprites, walkingLegSprites, flyingLegSprites, lockedLegSprites;
    public SpriteList idleFaceSprites, walkingFaceSprites, strainingFaceSprites, flyingFaceSprites, lockedFaceSprites;
    public SpriteRenderer face, leftLeg, rightLeg;
    public Color activeColor, inactiveColor;

    float movementRatio = 0.1f;
    float moveTracker = 0;
    int moveIndex = 0;

    private void Awake()
    {
        rightLeg.flipX = true;
        face.sprite = idleFaceSprites.sprites[0];
        leftLeg.sprite = idleLegSprites.sprites[0];
        rightLeg.sprite = idleLegSprites.sprites[0];
    }

    public void GiveMovement(float movement, float direction, bool strain)
    {
        if (movement == 0)
        {
            face.sprite = idleFaceSprites.sprites[0];
            leftLeg.sprite = idleLegSprites.sprites[0];
            rightLeg.sprite = idleLegSprites.sprites[0];

            leftLeg.flipX = false;
            rightLeg.flipX = true;
        }
        else
        {
            if (strain)
            {
                face.sprite = strainingFaceSprites.sprites[0];
            }
            else
            {
                face.sprite = walkingFaceSprites.sprites[0];
            }

            moveTracker += movement;
            if (moveTracker >= movementRatio)
            {
                int numSprites = walkingLegSprites.sprites.Length;
                moveTracker -= movementRatio;
                moveIndex = (moveIndex - 1 + numSprites) % numSprites;
                leftLeg.sprite = walkingLegSprites.sprites[moveIndex];
                rightLeg.sprite = walkingLegSprites.sprites[(moveIndex + numSprites / 2) % numSprites];

                if (direction > 0)
                {
                    face.flipX = true;
                    leftLeg.flipX = true;
                    rightLeg.flipX = true;
                }
                else if (direction < 0)
                {
                    face.flipX = false;
                    leftLeg.flipX = false;
                    rightLeg.flipX = false;
                }
            }
        }
    }

    public void SetIdle()
    {
        face.sprite = idleFaceSprites.sprites[0];
        leftLeg.sprite = idleLegSprites.sprites[0];
        rightLeg.sprite = idleLegSprites.sprites[0];

        face.flipX = false;
        leftLeg.flipX = false;
        rightLeg.flipX = true;
    }

    public void SetFlying()
    {
        face.sprite = flyingFaceSprites.sprites[0];
        int numSprites = flyingLegSprites.sprites.Length;
        leftLeg.sprite = flyingLegSprites.sprites[Random.Range(0, numSprites - 1)];
        rightLeg.sprite = flyingLegSprites.sprites[Random.Range(0, numSprites - 1)];

        face.flipX = false;
        leftLeg.flipX = false;
        rightLeg.flipX = true;
    }

    public void SetLocked()
    {
        face.sprite = lockedFaceSprites.sprites[0];
        leftLeg.sprite = lockedLegSprites.sprites[0];
        rightLeg.sprite = lockedLegSprites.sprites[0];

        face.flipX = false;
        leftLeg.flipX = false;
        rightLeg.flipX = true;
    }

    public void SetColor(bool on)
    {
        if (on)
        {
            face.color = activeColor;
            leftLeg.color = activeColor;
            rightLeg.color = activeColor;
        }
        else
        {
            face.color = inactiveColor;
            leftLeg.color = inactiveColor;
            rightLeg.color = inactiveColor;
        }
    }
}
