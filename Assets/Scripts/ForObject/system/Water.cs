using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public bool refreshDoubleJump = false;
    private PixelPerfectCollider PixCollider;

    private void Start()
    {
        PixCollider = GetComponent<PixelPerfectCollider>();
    }

    private void Update()
    {
        var col = PixCollider.InstancePlace(transform.position.x, transform.position.y, "Player");
        if (col != null)
        {
            var player = col.GetComponent<Player>();
            if (refreshDoubleJump)
                player.djump = true;
            if (player.gravityDirection == Gravity.Down)
            {
                if (player.vspeed < -2)
                    player.vspeed = -2;
            }
            else if (player.gravityDirection == Gravity.Up)
            {
                if (player.vspeed > 2)
                    player.vspeed = 2;
            }
        }
    }
}