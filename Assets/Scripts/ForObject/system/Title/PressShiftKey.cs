using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressShiftKey : MonoBehaviour
{
    public Text text;
    private float alpha = 1.0f;
    private int mode = 0;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (mode == 0)
        {
            if (alpha > 0)
            {
                alpha -= 0.02f;
            }
            else
            {
                mode = 1;
            }
        }
        else
        {
            if (alpha < 1)
            {
                alpha += 0.02f;
            }
            else
            {
                mode = 0;
            }
        }
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }
}