using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Save : MonoBehaviour
{
    public Difficulty displayDifficulty;
    private bool canSave = true;
    private PixelPerfectCollider PixCollider;
    private SpriteAnimator animator;
    private int timer = -1;

    private void Start()
    {
        if ((int)World.instance.difficulty > (int)displayDifficulty)
        {
            Destroy(gameObject);
            return;
        }
        PixCollider = GetComponent<PixelPerfectCollider>();
        animator = GetComponent<SpriteAnimator>();

        if (displayDifficulty == Difficulty.Medium)
        {
            animator.currentAnimation = "Medium";
        }

        animator.onAnimationEnd = OnAnimationEnd;
    }

    private void Update()
    {
        if (PixCollider.PlaceMeeting(transform.position.x, transform.position.y, "Player"))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                OnSave();
            }
        }
        if (PixCollider.PlaceMeeting(transform.position.x, transform.position.y, "Bullet"))
        {
            OnSave();
        }
        if (timer != -1)
        {
            if (timer-- == 0)
            {
                timer = -1;
                canSave = true;
            }
        }
    }

    private void OnSave()
    {
        if (canSave)
        {
            var player = FindObjectOfType<Player>();
            if (player != null)
            {
                canSave = false;
                animator.imageIndex = 1;
                animator.imageSpeed = 0.017f;
                timer = 30;
                World.instance.SaveGame(true);
            }
        }
    }

    private void OnAnimationEnd()
    {
        animator.imageSpeed = animator.imageIndex = 0;
    }
}