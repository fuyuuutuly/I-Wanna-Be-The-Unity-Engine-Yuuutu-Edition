using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStart : MonoBehaviour
{
    public GameObject player;

    void Awake()
    {
        if (FindObjectsOfType<Player>().Length == 0)
        {
            var inst = Instantiate(player);
            inst.transform.position = gameObject.transform.position + new Vector3(17, -23);
        }
    }
}
