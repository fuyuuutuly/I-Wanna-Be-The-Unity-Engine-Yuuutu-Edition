using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// World singleton helps us manage the game
public class World : Singleton<World>
{
    public string roomCaption = "I Wanna Be The Unity Engine";

    private WindowCaption windowCaption = new();

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
    public GameObject gameoverPrefab;
    public AudioSource BGM;
    public bool isEnableDeathSound = false;
    public AudioSource deathSound;
    public bool isEnableDeathMusic = true;
    public AudioSource deathMusic;

    // May move these to separate class
    public Dictionary<Texture2D, MaskData> maskDataManager = new();

    public Dictionary<string, List<PixelPerfectCollider>> colliders = new();

    private void Start()
    {
        // Initialize game
        windowCaption.SetWindowCaption(roomCaption);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;

        // calculate Time
        StartCoroutine(CalcTime());
    }

    private void Update()
    {
        if (gameStarted)
        {
            // Restart Game
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GameObject.FindWithTag("Player"))
                {
                    death++;
                }

                SaveGame(false);
                LoadGame(false);
            }

            // Update title
            var title = $"{roomCaption} [{difficulty}] SaveData{savenum} [Esc]:End Death:{death} Time:{Utility.SecToTime(time)}";
            windowCaption.SetWindowCaption(title);
            Debug.Log(title);
        }

        // End Game
        if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // to Title
        if (Input.GetKey(KeyCode.F2))
        {
            var player = GameObject.FindWithTag("Player");
            Destroy(player);
            Destroy(gameObject);
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

internal class WindowCaption
{
    private delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern bool SetWindowTextW(IntPtr hwnd, string title);

    [DllImport("user32")]
    private static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);

    [DllImport("user32")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);

    private IntPtr windowHandle;

    public WindowCaption()
    {
        IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;
        EnumWindows(new EnumWindowsCallBack(EnumWindCallback), handle);
    }

    public void SetWindowCaption(string caption)
    {
        SetWindowTextW(windowHandle, caption);
    }

    private bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
    {
        IntPtr pid = IntPtr.Zero;
        GetWindowThreadProcessId(hwnd, ref pid);
        if (pid == lParam)
        {
            windowHandle = hwnd;
            return true;
        }
        return false;
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