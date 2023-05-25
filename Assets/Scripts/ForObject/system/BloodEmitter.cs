using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodEmitter : MonoBehaviour
{
    float timer = 20;
    public GameObject blood;

    void Update()
    {
        timer -= 1;
        if (timer <= 0)
        {
            Destroy(gameObject);
            return;
        }
        for (var i = 0; i < 40; i++)
        {
            // Create blood instance...
            var inst = Instantiate(blood);
            inst.transform.position = transform.position;
        }
    }
}
