using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpinner : MonoBehaviour
{
    //public float rotationSpeed = 100f;
    public float bobbingAmp = 0.1f;
    public float bobbingFreq = 1f;
    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(0,0,rotationSpeed * Time.deltaTime);
        float yAmp = startPos.y + Mathf.Sin(Time.time * bobbingFreq) * bobbingAmp;
        transform.position = new Vector3(startPos.x, yAmp, startPos.z);
    }
}
