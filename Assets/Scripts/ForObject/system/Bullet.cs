using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int timer = 40;
    private float hspeed = 16;
    private float xscale;
    private PixelPerfectCollider PixCollider;

    private void Start()
    {
        var player = FindObjectOfType<Player>();
        foreach (var i in player.GetComponentsInChildren<Transform>())
        {
            if (i.gameObject.name == "Sprite")
                xscale = i.localScale.x;
        }
        PixCollider = GetComponent<PixelPerfectCollider>();
    }

    private void Update()
    {
        if (timer-- < 0)
        {
            Destroy(gameObject);
        }

        transform.position += new Vector3(hspeed * xscale, 0);
        if (PixCollider.PlaceMeeting(transform.position.x, transform.position.y, "Block"))
        {
            Destroy(gameObject);
        }
    }
}