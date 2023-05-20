using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;


// World singleton helps us manage the game
public class World : Singleton<World>
{

    [SerializeField] private string roomCaption = "I Wanna Be The Unity Engine";

    [NonSerialized] private WindowCaption windowCaption = new();

    [SerializeField] public string startScene = "Stage01";

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

    [SerializeField] public Player playerPrefab;
    [SerializeField] public AudioSource deathSound;

    // May move these to separate class
    public Dictionary<Texture2D, MaskData> maskDataManager = new();
    public Dictionary<string, List<PixelPerfectCollider>> colliders = new();

    void Start()
    {
        // Initialize game
        windowCaption.SetWindowCaption(roomCaption);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;
    }

    void Update()
    {
        if (gameStarted)
        {
            // Restart game
            if (Input.GetKeyDown(KeyCode.R))
            {
                SaveGame(false);
                LoadGame(false);
            }

            // Update title
            windowCaption.SetWindowCaption(roomCaption);
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
            Destroy(p.gameObject);

        var player = Instantiate(playerPrefab);
        player.gameObject.transform.position = new Vector3(savedPlayerX, savedPlayerY);

        SceneManager.LoadScene(savedScene);
    }

    public void SaveGame(bool savePosition)
    {
        if (savePosition)
        {
            var player = FindObjectOfType<Player>();
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
        deathSound.Play();
        death++;
    }

    public enum Difficulty
    {
        Medium = 0,
        Hard = 1,
        VeryHard = 2,
        Impossible = 3,
    }
}

class WindowCaption
{
    delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32", CharSet = CharSet.Unicode)]
    static extern bool SetWindowTextW(IntPtr hwnd, string title);

    [DllImport("user32")]
    static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);

    [DllImport("user32")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);

    IntPtr windowHandle;

    public WindowCaption()
    {
        IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;
        EnumWindows(new EnumWindowsCallBack(EnumWindCallback), handle);
    }

    public void SetWindowCaption(string caption)
    {
        SetWindowTextW(windowHandle, caption);
    }

    bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
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