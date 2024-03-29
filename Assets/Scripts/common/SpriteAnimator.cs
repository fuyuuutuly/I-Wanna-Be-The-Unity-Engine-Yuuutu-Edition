﻿using UnityEngine;
using System;

// Sprite Animator
// Used to replace unity tedious animator
// It also works well with pixel perfect collider

public class SpriteAnimator : MonoBehaviour
{
    public SpriteAnimation[] animations = new SpriteAnimation[1] { new SpriteAnimation() { name = "Default" } };

    public string startAnimation = "Default";

    private string _currentAnimation;

    public string currentAnimation
    {
        set
        {
            _currentAnimation = value;
            foreach (var i in animations)
            {
                if (i.name == _currentAnimation)
                    sprAnimation = i;
            }
        }
        get
        {
            return _currentAnimation;
        }
    }

    public SpriteAnimation sprAnimation { get; private set; }

    public float imageSpeed;
    public float imageIndex;

    public delegate void OnAnimationEnd();

    public OnAnimationEnd onAnimationEnd;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentAnimation = startAnimation;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        imageIndex += imageSpeed;

        spriteRenderer.sprite = sprAnimation.sprites[(int)imageIndex % sprAnimation.sprites.Length];

        if (imageIndex > sprAnimation.sprites.Length)
        {
            imageIndex -= (int)imageIndex;
            if (onAnimationEnd != null)
                onAnimationEnd();
        }
    }
}

[Serializable]
public class SpriteAnimation
{
    public string name;
    public Sprite[] sprites;
}