using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Transform _transform;

    private float jump = -8.5f;
    private float jump2 = -7;
    private float maxSpeed = 3;
    private float maxVspeed = 9;

    [ReadOnly] public bool djump = true;

    [ReadOnly] public float hspeed = 0;
    [ReadOnly] public float vspeed = 0;
    private float gravity = -0.4f;

    public float X { get => _transform.position.x; set => _transform.position = new Vector3(value, Y, _transform.position.z); }
    public float Y { get => _transform.position.y; set => _transform.position = new Vector3(X, value, _transform.position.z); }

    private float xprevious;
    private float yprevious;

    private bool onPlatform = false;

    public GameObject sprite;
    private SpriteAnimator animator;

    public World world;
    private PixelPerfectCollider pixCollider;
    public BloodEmitter bloodEmitter;
    public Bullet bullet;

    public AudioSource jumpSound;
    public AudioSource djumpSound;
    public AudioSource shootSound;
    public AudioSource walljumpSound;

#if UNITY_EDITOR

    private void Awake()
    {
        // Do something to help us debug the level
        if (!GameObject.FindGameObjectWithTag("World"))
        {
            world = Instantiate(world);
            World.instance.gameStarted = true;
            World.instance.savedScene = SceneManager.GetActiveScene().name;
            World.instance.autosave = true;
        }
    }

#endif

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        _transform = transform;
        pixCollider = GetComponent<PixelPerfectCollider>();
        animator = sprite.GetComponent<SpriteAnimator>();

        if (World.instance.autosave)
        {
            World.instance.SaveGame(true);
            World.instance.autosave = false;
        }
    }

    private void Update()
    {
        xprevious = X;
        yprevious = Y;

        var L = World.instance.KeyLeft.IsPressed();
        var R = World.instance.KeyRight.IsPressed();

        var h = 0;

        if (R)
            h = 1;
        else if (L)
            h = -1;

        var notOnBlock = !pixCollider.PlaceMeeting(X, Y - 1, "Block");
        var onVineL = pixCollider.PlaceMeeting(X - 1, Y, "WalljumpL") && notOnBlock;
        var onVineR = pixCollider.PlaceMeeting(X + 1, Y, "WalljumpR") && notOnBlock;

        if (h != 0)
        {
            if (h == -1)
                sprite.transform.localScale = new Vector3(-1, 1);
            else if (h == 1)
                sprite.transform.localScale = new Vector3(1, 1);

            animator.currentAnimation = "Running";
            animator.imageSpeed = 0.5f;
        }
        else
        {
            animator.currentAnimation = "Idle";
            animator.imageSpeed = 0.2f;
        }
        hspeed = maxSpeed * h;

        if (!onPlatform)
        {
            if (vspeed > 0.05)
            {
                animator.currentAnimation = "Jump";
            }
            else if (vspeed < -0.05)
            {
                animator.currentAnimation = "Fall";
            }
        }
        else
        {
            if (!pixCollider.PlaceMeeting(X, Y - 4, "Platform"))
            {
                onPlatform = false;
            }
        }

        if (Abs(vspeed) > maxVspeed)
        {
            vspeed = Sign(vspeed) * maxVspeed;
        }

        if (Keyboard.current[Key.Z].wasPressedThisFrame)
            Shoot();

        if (Keyboard.current[Key.LeftShift].wasPressedThisFrame)
            Jump();

        if (Keyboard.current[Key.LeftShift].wasReleasedThisFrame)
            VJump();

        // Walljumps
        if (onVineL || onVineR)
        {
            if (onVineR)
                sprite.transform.localScale = new Vector3(-1, 1);
            else
                sprite.transform.localScale = new Vector3(1, 1);

            vspeed = -2;
            animator.currentAnimation = "Sliding";
            animator.imageSpeed = 0.5f;

            if ((onVineL && World.instance.KeyRight.WasPressedThisFrame()) || (onVineR && World.instance.KeyLeft.WasPressedThisFrame()))
            {
                if (Keyboard.current[Key.LeftShift].isPressed)
                {
                    if (onVineR)
                        hspeed = -15;
                    else
                        hspeed = 15;

                    vspeed = 9;

                    walljumpSound.Play();
                    animator.currentAnimation = "Jump";
                }
                else
                {
                    if (onVineR)
                        hspeed = -3;
                    else
                        hspeed = 3;

                    animator.currentAnimation = "Fall";
                }
            }
        }

        // Move
        vspeed += gravity;
        X += hspeed;
        Y += vspeed;

        // Collision

        // Block check
        if (pixCollider.PlaceMeeting(X, Y, "Block"))
        {
            X = xprevious;
            Y = yprevious;

            if (pixCollider.PlaceMeeting(X + hspeed, Y, "Block"))
            {
                pixCollider.MoveContactX(hspeed, "Block");
                hspeed = 0;
            }

            if (pixCollider.PlaceMeeting(X, Y + vspeed, "Block"))
            {
                pixCollider.MoveContactY(vspeed, "Block");

                if (vspeed < 0)
                {
                    djump = true;
                }
                vspeed = 0;
            }

            if (pixCollider.PlaceMeeting(X + hspeed, Y + vspeed, "Block"))
            {
                hspeed = 0;
            }

            X += hspeed;
            Y += vspeed;
            if (pixCollider.PlaceMeeting(X, Y, "Block"))
            {
                X = xprevious;
                Y = yprevious;
            }
        }

        // Platform check
        var platform = pixCollider.InstancePlace(X, Y, "Platform");
        if (platform != null)
        {
            if (Y - vspeed / 2 >= platform.transform.position.y)
            {
                var vsp = platform.GetComponent<MovingPlatform>().vspeed;
                if (vsp <= 0)
                {
                    Y = platform.transform.position.y + 9;
                    vspeed = vsp;
                }
                onPlatform = true;
                djump = true;
            }
        }

        // Killer check
        if (pixCollider.PlaceMeeting(X, Y, "Killer"))
        {
            Death();
        }

        if (Keyboard.current[Key.Q].wasPressedThisFrame)
        {
            Death();
        }

        // Update position
        _transform.position = new Vector3(X, Y);
    }

    private void Jump()
    {
        if (pixCollider.PlaceMeeting(X, Y - 1, "Block") || pixCollider.PlaceMeeting(X, Y - 1, "Platform") || onPlatform
            || pixCollider.PlaceMeeting(X, Y - 1, "Water"))
        {
            vspeed = -jump;
            djump = true;
            jumpSound.Play();
        }
        else if (djump || pixCollider.PlaceMeeting(X, Y - 1, "Water2"))
        {
            animator.currentAnimation = "Jump";
            vspeed = -jump2;
            djump = false;
            djumpSound.Play();

            if (!pixCollider.PlaceMeeting(X, Y - 1, "Water3"))
                djump = false;
            else
                djump = true;
        }
    }

    private void VJump()
    {
        if (vspeed > 0)
            vspeed *= 0.45f;
    }

    private void Shoot()
    {
        if (FindObjectsOfType<Bullet>().Length < 4)
        {
            var inst = Instantiate(bullet);
            inst.transform.position = new Vector2(X, Y);
            shootSound.Play();
        }
    }

    public void Death()
    {
        var inst = Instantiate(bloodEmitter);
        inst.transform.position = new Vector2(X, Y);
        Destroy(gameObject);
        World.instance.KillPlayer();
    }
}