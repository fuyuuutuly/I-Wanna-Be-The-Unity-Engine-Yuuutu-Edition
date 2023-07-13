using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Warp : MonoBehaviour
{
    public PixelPerfectCollider pixCollider;
    public string nextScene;
    public bool enableChangePosition;
    public float x;
    public float y;

    private void Start()
    {
    }

    private void Update()
    {
        if (pixCollider.PlaceMeeting(transform.position.x, transform.position.y, "Player"))
        {
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            if (enableChangePosition)
            {
                player.X = x;
                player.Y = y;
            }
            else
            {
                Destroy(player.gameObject);
            }

            SceneManager.LoadScene(nextScene);
        }
    }
}