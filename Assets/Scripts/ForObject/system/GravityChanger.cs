using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityChanger : MonoBehaviour
{
    public Gravity gravityDirection;
    private Transform _transform;
    private PixelPerfectCollider pixCollider;

    // Start is called before the first frame update
    private void Start()
    {
        _transform = transform;
        pixCollider = GetComponent<PixelPerfectCollider>();
    }

    // Update is called once per frame
    private void Update()
    {
        Player player = pixCollider.InstancePlace(_transform.position.x, _transform.position.y, "Player")?.GetComponent<Player>();
        if (player != null)
        {
            if (player.gravityDirection != gravityDirection)
            {
                player.gravityDirection = gravityDirection;
                player.djump = true;
                if (gravityDirection == Gravity.Up)
                {
                    player.Y += 3;
                }
                else if (gravityDirection == Gravity.Down)
                {
                    player.Y -= 3;
                }
                player.vspeed = 0;
            }
        }
    }
}