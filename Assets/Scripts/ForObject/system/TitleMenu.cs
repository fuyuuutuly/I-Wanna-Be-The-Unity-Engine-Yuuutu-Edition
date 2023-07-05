using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField]
    private string nextScene;

    private void Update()
    {
        if (Keyboard.current[Key.LeftShift].wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}