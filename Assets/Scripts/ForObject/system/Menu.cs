using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    private int select = 0;
    private Vector3 cursorStart;

    private Text[] difficultyText = new Text[3];
    private Text[] deathsText = new Text[3];
    private Text[] timeText = new Text[3];

    public string nextScene;

    private void Start()
    {
        GameObject cursor = GameObject.Find("Cursor");
        cursorStart = cursor.transform.position;

        for (var i = 0; i < 3; i++)
        {
            difficultyText[i] = GameObject.Find($"Difficulty{i + 1}").GetComponent<Text>();
            deathsText[i] = GameObject.Find($"Deaths{i + 1}").GetComponent<Text>();
            timeText[i] = GameObject.Find($"Time{i + 1}").GetComponent<Text>();

            if (!File.Exists($"Data/save{i + 1}"))
            {
                difficultyText[i].text = "No Data";
                deathsText[i].text = $"";
                timeText[i].text = $"";
            }
            else
            {
                string text = File.ReadAllText($"Data/save{i + 1}");
                SaveFile saveFile = JsonUtility.FromJson<SaveFile>(text);

                difficultyText[i].text = saveFile.difficulty.ToString();
                deathsText[i].text = $"Deaths: {saveFile.death}";
                timeText[i].text = $"Time: {saveFile.time / 3600}:{saveFile.time / 60 % 60}:{saveFile.time % 60}";
            }
        }
    }

    private void Update()
    {
        if (World.instance.KeyRight.WasPressedThisFrame())
        {
            select++;
            if (select == 3)
                select = 0;
        }
        if (World.instance.KeyLeft.WasPressedThisFrame())
        {
            select--;
            if (select == -1)
                select = 2;
        }
        if (Keyboard.current[Key.LeftShift].wasPressedThisFrame || Keyboard.current[Key.RightShift].wasPressedThisFrame)
        {
            World.instance.savenum = select + 1;
            SceneManager.LoadScene(nextScene);
        }
        GameObject cursor = GameObject.Find("Cursor");
        cursor.transform.position = cursorStart + new Vector3(256f * select, 0);
    }
}