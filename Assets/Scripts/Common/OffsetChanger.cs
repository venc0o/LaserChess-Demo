using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetChanger : MonoBehaviour {

    public float speed = 10f;

    public bool OffsX = true;
    public bool offsY;

    Material mat;

    float offset = 1;

	void Start ()
    {
        if (GetComponent<LineRenderer>() != null)
            mat = GetComponent<LineRenderer>().material;
        else
            mat = GetComponent<MeshRenderer>().material;
    }
	

	void Update ()
    {

        if (mat == null)
            return;

        offset -= Time.deltaTime * (speed/10);

        if (OffsX && offsY)
            mat.SetTextureOffset("_MainTex", new Vector2(offset, offset));
        else if (OffsX)
            mat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        else
            mat.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}
