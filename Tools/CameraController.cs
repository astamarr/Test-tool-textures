using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float translationSpeed = 5.0f;
    public float rotationSpeed = 45.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * translationSpeed;
        transform.position += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * translationSpeed;
        transform.rotation = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed, Vector3.up)*transform.rotation;
        transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed, Vector3.right);
    }
}
