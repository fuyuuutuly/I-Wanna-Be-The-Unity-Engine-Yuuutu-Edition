using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpRefresher : MonoBehaviour
{
    public int refreshTime = 100;

    private int timer = -1;
    private int mode = 0;
    private float yStart;

    private void Start()
    {
        yStart = transform.position.y;
    }

    private void Update()
    {
        if (mode == 0)
        {
            if (transform.position.y < yStart + 4)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
            }
            else
            {
                mode = 1;
            }
        }
        else
        {
            if (transform.position.y > yStart - 4)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
            }
            else
            {
                mode = 0;
            }
        }

        if (GetComponent<SpriteRenderer>().enabled)
        {
            var player = GetComponent<PixelPerfectCollider>().InstancePlace(transform.position.x, transform.position.y, "Player");
            if (player != null)
            {
                player.GetComponent<Player>().djump = true;
                GetComponent<SpriteRenderer>().enabled = false;
                timer = refreshTime;
            }
        }
        if (timer != -1)
        {
            if (timer-- == 0)
            {
                timer = -1;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }
}