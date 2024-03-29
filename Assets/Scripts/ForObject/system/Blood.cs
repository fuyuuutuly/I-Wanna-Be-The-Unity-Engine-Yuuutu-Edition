﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class Blood : MonoBehaviour
{
    private float x, y, hspeed, vspeed, gravity, speed, direction = 0;
    public PixelPerfectCollider pixCollider;

    private Transform _transform;

    private void Start()
    {
        _transform = transform;
        x = _transform.position.x;
        y = _transform.position.y;
        direction = Random.Range(0, 35) * 10;
        speed = Random.Range(0, 6);
        gravity = -(0.1f + Random.Range(0, 0.2f));
        hspeed = speed * Cos(direction * Deg2Rad);
        vspeed = speed * Sin(direction * Deg2Rad);
    }

    private void Update()
    {
        if (gravity != 0)
        {
            vspeed += gravity;

            x += hspeed;
            y += vspeed;

            if (pixCollider.PlaceMeeting(x, y, "Block"))
            {
                pixCollider.MoveContactX(hspeed, "Block");
                pixCollider.MoveContactY(vspeed, "Block");

                hspeed = 0;
                vspeed = 0;
                gravity = 0;
                return;
            }

            _transform.position = new Vector2(x, y);
        }
    }
}