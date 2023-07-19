using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    public float spinSpeed;

    private Transform _transform;

    // Start is called before the first frame update
    private void Start()
    {
        _transform = transform;
    }

    // Update is called once per frame
    private void Update()
    {
        _transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + spinSpeed);
    }
}