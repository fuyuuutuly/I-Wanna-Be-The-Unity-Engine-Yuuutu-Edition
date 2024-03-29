﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PlayerCamera : MonoBehaviour
{
    private PixelPerfectCamera pixCamera;
    private float xStart;
    private float yStart;

    public float xScreenMin = 0;
    public float xScreenMax = 0;
    public float yScreenMin = 0;
    public float yScreenMax = 0;

    public bool playerOnCenter = false;

    private void Start()
    {
        pixCamera = GetComponent<PixelPerfectCamera>();

        xStart = transform.position.x;
        yStart = transform.position.y;
    }

    private void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player != null)
        {
            var xFollow = player.X - xStart + pixCamera.refResolutionX / 2;
            var yFollow = player.Y - yStart + pixCamera.refResolutionY / 2;

            var width = pixCamera.refResolutionX;
            var height = pixCamera.refResolutionY;

            float newX, newY;
            if (playerOnCenter)
            {
                newX = player.X;
                if (newX < xStart + width * xScreenMin)
                {
                    newX = xStart + width * xScreenMin;
                }
                if (newX > xStart + width * xScreenMax)
                {
                    newX = xStart + width * xScreenMax;
                }

                newY = player.Y;
                if (newY < yStart + height * yScreenMin)
                {
                    newY = yStart + height * yScreenMin;
                }
                if (newY > yStart + height * yScreenMax)
                {
                    newY = yStart + height * yScreenMax;
                }
            }
            else
            {
                newX = Mathf.Floor(xFollow / width) * width + xStart;
                if (newX < xStart + width * xScreenMin)
                {
                    newX = xStart + width * xScreenMin;
                }
                if (newX > xStart + width * xScreenMax)
                {
                    newX = xStart + width * xScreenMax;
                }

                newY = Mathf.Floor(yFollow / height) * height + yStart;
                if (newY < yStart + height * yScreenMin)
                {
                    newY = yStart + height * yScreenMin;
                }
                if (newY > yStart + height * yScreenMax)
                {
                    newY = yStart + height * yScreenMax;
                }
            }

            transform.position = new Vector3(newX, newY, transform.position.z);
        }
    }
}