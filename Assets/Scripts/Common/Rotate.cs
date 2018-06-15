using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public float speed = 5f;
    public bool isClockWise;

    void Update()
    {

        if (isClockWise)
            transform.Rotate(Vector3.forward, speed * Time.deltaTime);
        else
            transform.Rotate(Vector3.forward, -speed * Time.deltaTime);

    }


}
