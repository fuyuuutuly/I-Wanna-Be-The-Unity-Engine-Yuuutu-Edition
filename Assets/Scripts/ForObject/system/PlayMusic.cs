using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    public AudioClip bGMAudioClip;
    public bool replayOnRestart = false;

    // Start is called before the first frame update
    private void Start()
    {
        var world = GameObject.FindWithTag("World");
        var bGMAudio = world.GetComponent<World>().BGM;

        if (replayOnRestart)
        {
            bGMAudio.Stop();
            bGMAudio.clip = bGMAudioClip;
            bGMAudio.Play();
        }
        else
        {
            if (bGMAudio.clip == bGMAudioClip && bGMAudio.isPlaying == true)
            {
            }
            else
            {
                bGMAudio.clip = bGMAudioClip;
                bGMAudio.Play();
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}