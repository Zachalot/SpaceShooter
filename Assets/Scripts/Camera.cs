using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    private float _defaultXPosition = 0f;
    private float _defaultYPosition = 1f;
    private float _defaultZPosition = -10f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(_defaultXPosition, _defaultYPosition, _defaultZPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
