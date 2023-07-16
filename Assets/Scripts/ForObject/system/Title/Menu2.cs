using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Menu2 : MonoBehaviour
{
    public AudioSource audioDecide;
    public AudioSource audioSelect;
    public AudioSource audioCancel;
    public GameObject pressShiftKey;
    public List<GameObject> files;

    private MenuMode mode = MenuMode.PressShiftKey;
    private int select = 0;
    private Difficulty seletedDifficulty;

    private string[] difficultyString = new string[3];
    private string[] deathsString = new string[3];
    private string[] timeString = new string[3];

    private Text[] difficultyText = new Text[3];
    private Text[] deathsText = new Text[3];
    private Text[] timeText = new Text[3];

    private void Start()
    {
        for (var i = 0; i < 3; i++)
        {
            if (File.Exists($"Data/save{i + 1}"))
            {
                string text = File.ReadAllText($"Data/save{i + 1}");
                SaveFile saveFile = JsonUtility.FromJson<SaveFile>(text);

                difficultyString[i] = saveFile.difficulty.ToString();
                deathsString[i] = $"Deaths: {saveFile.death}";
                timeString[i] = $"Time: {Utility.SecToTime(saveFile.time)}";
            }
        }
    }

    private void Update()
    {
        switch (mode)
        {
            case MenuMode.PressShiftKey:
                if (Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.RightShift].wasPressedThisFrame)
                {
                    audioDecide.Play();

                    mode = MenuMode.FileSelect;
                    pressShiftKey.SetActive(false);
                    files.ForEach((f) => { f.SetActive(true); });

                    for (var i = 0; i < 3; i++)
                    {
                        difficultyText[i] = GameObject.Find($"Difficulty{i + 1}").GetComponent<Text>();
                        deathsText[i] = GameObject.Find($"Deaths{i + 1}").GetComponent<Text>();
                        timeText[i] = GameObject.Find($"Time{i + 1}").GetComponent<Text>();

                        difficultyText[i].text = difficultyStringToText(difficultyString[i]);
                        deathsText[i].text = deathsString[i];
                        timeText[i].text = timeString[i];
                    }
                }
                break;

            case MenuMode.FileSelect:
                if (Keyboard.current[Key.Z].wasPressedThisFrame)
                {
                    audioCancel.Play();

                    mode = MenuMode.PressShiftKey;
                    pressShiftKey.SetActive(true);
                    files.ForEach((f) => { f.SetActive(false); });
                }
                else
                {
                    if (World.instance.KeyRight.WasPressedThisFrame())
                    {
                        audioSelect.Play();

                        select++;
                        if (select == 3)
                            select = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            files[i].GetComponent<Outline>().enabled = (i == select);
                        }
                    }
                    else if (World.instance.KeyLeft.WasPressedThisFrame())
                    {
                        audioSelect.Play();

                        select--;
                        if (select == -1)
                            select = 2;
                        for (int i = 0; i < 3; i++)
                        {
                            files[i].GetComponent<Outline>().enabled = (i == select);
                        }
                    }
                    else if (Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.RightShift].wasPressedThisFrame)
                    {
                        audioDecide.Play();

                        mode = MenuMode.DifficultySelect;
                        if (difficultyString[select] != null && difficultyString[select] != "")
                        {
                            seletedDifficulty = Difficulty.LoadGame;
                            difficultyText[select].text = difficultySelectToText(seletedDifficulty);
                        }
                        else
                        {
                            seletedDifficulty = Difficulty.Medium;
                            difficultyText[select].text = difficultySelectToText(seletedDifficulty);
                        }

                        pressShiftKey.SetActive(false);
                        files.ForEach((f) => { f.SetActive(true); });
                    }
                }

                break;

            case MenuMode.DifficultySelect:
                if (Keyboard.current[Key.Z].wasPressedThisFrame)
                {
                    audioCancel.Play();

                    mode = MenuMode.FileSelect;
                    difficultyText[select].text = difficultyStringToText(difficultyString[select]);
                }
                else
                {
                    if (World.instance.KeyRight.WasPressedThisFrame())
                    {
                        audioSelect.Play();

                        seletedDifficulty++;
                        if (difficultyString[select] != null && difficultyString[select] != "")
                        {
                            if ((int)seletedDifficulty == 5)
                                seletedDifficulty = Difficulty.Medium;
                        }
                        else
                        {
                            if ((int)seletedDifficulty == 4)
                                seletedDifficulty = Difficulty.Medium;
                        }

                        difficultyText[select].text = difficultySelectToText(seletedDifficulty);
                    }
                    else if (World.instance.KeyLeft.WasPressedThisFrame())
                    {
                        audioSelect.Play();

                        seletedDifficulty--;
                        if (difficultyString[select] != null && difficultyString[select] != "")
                        {
                            if ((int)seletedDifficulty == -1)
                                seletedDifficulty = Difficulty.LoadGame;
                        }
                        else
                        {
                            if ((int)seletedDifficulty == -1)
                                seletedDifficulty = Difficulty.Impossible;
                        }

                        difficultyText[select].text = difficultySelectToText(seletedDifficulty);
                    }
                    else if (Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.RightShift].wasPressedThisFrame)
                    {
                        World.instance.savenum = select + 1;
                        if (seletedDifficulty == Difficulty.LoadGame)
                        {
                            // Load exists game
                            World.instance.LoadGame(true);
                        }
                        else
                        {
                            // Start new game
                            World.instance.gameStarted = true;
                            World.instance.autosave = true;

                            World.instance.difficulty = seletedDifficulty;

                            if (File.Exists($"Data/save{World.instance.savenum}"))
                                File.Delete($"Data/save{World.instance.savenum}");

                            SceneManager.LoadScene(World.instance.startScene);
                        }
                    }
                }

                break;
        }
    }

    private string difficultyStringToText(string dif)
    {
        if (dif == null || dif == "")
        {
            return "No Data";
        }
        else
        {
            return dif;
        }
    }

    private string difficultySelectToText(Difficulty dif)
    {
        switch (dif)
        {
            case Difficulty.Medium:
                return "< Medium >";

            case Difficulty.Hard:
                return "< Hard >";

            case Difficulty.VeryHard:
                return "< Very Hard >";

            case Difficulty.Impossible:
                return "< Impossible >";

            case Difficulty.LoadGame:
                return "< Load Game >";

            default:
                return "";
        }
    }
}

internal enum MenuMode
{
    PressShiftKey,
    FileSelect,
    DifficultySelect
}