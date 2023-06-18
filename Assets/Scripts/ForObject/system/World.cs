using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// World singleton helps us manage the game
public class World : Singleton<World>
{
    public string startScene = "Stage01";

    [ReadOnly] public int savenum = 1;

    [ReadOnly] public Difficulty difficulty = Difficulty.Medium;

    [ReadOnly] public int death = 0;
    [ReadOnly] public int time = 0;
    [ReadOnly] public int grav = 1;

    [ReadOnly] public bool gameStarted = false;
    [ReadOnly] public bool autosave = false;

    [ReadOnly] public string savedScene;
    [ReadOnly] public float savedPlayerX;
    [ReadOnly] public float savedPlayerY;
    [ReadOnly] public int savedGrav;

    public Player playerPrefab;
    public GAMEOVER gameoverPrefab;
    public AudioSource BGM;
    public bool isEnableDeathSound = false;
    public AudioSource deathSound;
    public bool isEnableDeathMusic = true;
    public AudioSource deathMusic;

    // May move these to separate class
    public Dictionary<Texture2D, MaskData> maskDataManager = new();

    public Dictionary<string, List<PixelPerfectCollider>> colliders = new();

    // I'm using PlayerInput because if I try to use the arrow keys on the keyboard directly, the gamepad stops responding.
    // * Also, when using GetKey, it becomes unable to obtain correct values during restart or scene transitions.

    public InputAction KeyLeft
    {
        get { return playerInput.currentActionMap["Left"]; }
    }

    public InputAction KeyRight
    {
        get { return playerInput.currentActionMap["Right"]; }
    }

    public InputAction KeyUp
    {
        get { return playerInput.currentActionMap["Up"]; }
    }

    public InputAction KeyDown
    {
        get { return playerInput.currentActionMap["Down"]; }
    }

    private PlayerInput playerInput;

    //For Windows
#if UNITY_STANDALONE_WIN

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(IntPtr hwnd, string lpString);

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(string className, string windowName);

    private IntPtr window;
#endif

    private void Start()
    {
        // Initialize game
#if UNITY_STANDALONE_WIN
        window = FindWindow(null, Application.productName);
#endif

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;

        // calculate Time
        StartCoroutine(CalcTime());

        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (gameStarted)
        {
            // Restart Game
            if (Keyboard.current[Key.R].wasPressedThisFrame)
            {
                if (GameObject.FindWithTag("Player"))
                {
                    death++;
                }

                SaveGame(false);
                LoadGame(false);
            }

            // Update title
#if UNITY_STANDALONE_WIN
            var title = $"{ Application.productName} [{difficulty}] SaveData{savenum} [Esc]:End Death:{death} Time:{Utility.SecToTime(time)}";
            SetWindowText(window, title);
#endif
        }

        // End Game
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // to Title
        if (Keyboard.current[Key.F2].wasPressedThisFrame)
        {
            var player = GameObject.FindWithTag("Player");
            Destroy(player);
            Destroy(gameObject);
#if UNITY_STANDALONE_WIN
            SetWindowText(window, Application.productName);
#endif
            SceneManager.LoadScene("Title");
        }
    }

    public void LoadGame(bool loadFile)
    {
        if (loadFile)
        {
            var saveJson = File.ReadAllText($"Data/save{savenum}");
            var saveFile = JsonUtility.FromJson<SaveFile>(saveJson);

            instance.death = saveFile.death;
            time = saveFile.time;

            difficulty = saveFile.difficulty;
            savedScene = saveFile.scene;

            savedPlayerX = saveFile.playerX;
            savedPlayerY = saveFile.playerY;
            savedGrav = saveFile.playerGrav;
        }
        gameStarted = true;
        autosave = false;
        grav = savedGrav;

        foreach (var p in FindObjectsOfType<Player>())
        {
            Destroy(p.gameObject);
        }

        var player = Instantiate(playerPrefab);
        player.gameObject.transform.position = new Vector3(savedPlayerX, savedPlayerY);

        deathMusic.Stop();
        deathSound.Stop();

        SceneManager.LoadScene(savedScene);
    }

    public void SaveGame(bool savePosition)
    {
        if (savePosition)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                savedScene = SceneManager.GetActiveScene().name;
                savedPlayerX = player.transform.position.x;
                savedPlayerY = player.transform.position.y;
                savedGrav = grav;
            }
        }

        var saveFile = new SaveFile()
        {
            death = death,
            time = time,
            difficulty = difficulty,
            scene = savedScene,
            playerX = savedPlayerX,
            playerY = savedPlayerY,
            playerGrav = savedGrav,
        };

        var saveJson = JsonUtility.ToJson(saveFile);
        if (!Directory.Exists("Data"))
            Directory.CreateDirectory("Data");

        File.WriteAllText($"Data/save{savenum}", saveJson);
    }

    public void KillPlayer()
    {
        // show GAMEOVER
        var mainCameraTransform = GameObject.FindWithTag("MainCamera").transform;
        Instantiate(
            gameoverPrefab,
            new Vector3(mainCameraTransform.position.x, mainCameraTransform.position.y, 0),
            mainCameraTransform.rotation
        );

        BGM.Pause();

        if (isEnableDeathMusic)
        {
            deathMusic.Play();
        }
        if (isEnableDeathSound)
        {
            deathSound.Play();
        }
        death++;
    }

    public IEnumerator CalcTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (GameObject.FindWithTag("Player"))
            {
                time++;
            }
        }
    }
}

public class MaskData
{
    public int left;
    public int right;
    public int top;
    public int bottom;

    public int width;
    public int height;

    public bool[] boolData;
}

internal enum KeyState
{
    IsPressed,
    IsReleased,
    WasPressedThisFrame,
    WasReleasedThisFrame
}