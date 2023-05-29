using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAMEOVER : MonoBehaviour
{
    public SpriteRenderer sprite;

    // Start is called before the first frame update
    private void Start()
    {
        // show GAMEOVER
        StartCoroutine(Utility.Delay(0.6f, () =>
        {
            sprite.enabled = true;
        }));
    }

    // Update is called once per frame
    private void Update()
    {
    }
}