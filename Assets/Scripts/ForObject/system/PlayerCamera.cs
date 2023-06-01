using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PlayerCamera : MonoBehaviour
{
    private PixelPerfectCamera PixCamera;
    private float xStart;
    private float yStart;

    private void Start()
    {
        PixCamera = GetComponent<PixelPerfectCamera>();

        xStart = transform.position.x;
        yStart = transform.position.y;
    }

    private void Update()
    {
        var player = GameObject.FindObjectOfType<Player>();
        if (player != null)
        {
            var xFollow = player.x - xStart + PixCamera.refResolutionX / 2;
            var yFollow = player.y - yStart + PixCamera.refResolutionY / 2;

            var width = PixCamera.refResolutionX;
            var height = PixCamera.refResolutionY;

            transform.position = new Vector3(Mathf.Floor(xFollow / width) * width + xStart,
                Mathf.Floor(yFollow / height) * height + yStart, transform.position.z);
        }
    }
}