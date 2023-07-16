using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpStart : MonoBehaviour
{
    public Difficulty difficulty;
    public PixelPerfectCollider pixCollider;

    private void Start()
    {
    }

    private void Update()
    {
        if (pixCollider.PlaceMeeting(transform.position.x, transform.position.y, "Player"))
        {
            if (difficulty == Difficulty.LoadGame)
            {
                if (File.Exists($"Data/save{World.instance.savenum}"))
                {
                    // Load exists game
                    World.instance.LoadGame(true);
                }
                else
                {
                    // Restart scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                // Start new game
                World.instance.gameStarted = true;
                World.instance.autosave = true;

                World.instance.difficulty = difficulty;

                if (File.Exists($"Data/save{World.instance.savenum}"))
                    File.Delete($"Data/save{World.instance.savenum}");

                var player = GameObject.FindWithTag("Player");
                Destroy(player);

                SceneManager.LoadScene(World.instance.startScene);
            }
        }
    }
}