using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Save : MonoBehaviour
{
    private bool canSave = true;
    private PixelPerfectCollider PixCollider;
    private SpriteAnimator animtor;
    private int timer = -1;

    private void Start()
    {
        if ((int)World.instance.difficulty > (int)Difficulty.Hard)
        {
            Destroy(gameObject);
            return;
        }
        PixCollider = GetComponent<PixelPerfectCollider>();
        animtor = GetComponent<SpriteAnimator>();

        animtor.onAnimationEnd = OnAnimationEnd;
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
                animtor.imageIndex = 1;
                animtor.imageSpeed = 0.017f;
                timer = 30;
                World.instance.SaveGame(true);
            }
        }
    }

    private void OnAnimationEnd()
    {
        animtor.imageSpeed = animtor.imageIndex = 0;
    }
}