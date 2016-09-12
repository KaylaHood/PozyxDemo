using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class HandMovement : MonoBehaviour {

    public Rigidbody RightHand;
    public Rigidbody LeftHand;
    public float HandMovementSpeed = 0.01f;

    // Key Controls
    public KeyCode RightHandRight = KeyCode.RightArrow;
    public KeyCode RightHandLeft = KeyCode.LeftArrow;
    public KeyCode LeftHandRight = KeyCode.D;
    public KeyCode LeftHandLeft = KeyCode.A;

    // Analog Controls

    public float Direction = 0.0f;

	// Use this for initialization
	void Start () {
        Direction = RightHand.transform.localPosition.x - LeftHand.transform.localPosition.x;
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKey(RightHandRight))
        {
            RightHand.transform.Translate(new Vector3(HandMovementSpeed, 0.0f, 0.0f));
            Debug.Log("Right Hand moves Right");
        }	
        else if(Input.GetKey(RightHandLeft))
        {
            RightHand.transform.Translate(new Vector3(-HandMovementSpeed, 0.0f, 0.0f));
            Debug.Log("Right Hand moves Left");
        }	
        else if(Input.GetKey(LeftHandRight))
        {
            LeftHand.transform.Translate(new Vector3(HandMovementSpeed, 0.0f, 0.0f));
            Debug.Log("Left Hand moves Right");
        }	
        else if(Input.GetKey(LeftHandLeft))
        {
            LeftHand.transform.Translate(new Vector3(-HandMovementSpeed, 0.0f, 0.0f));
            Debug.Log("Left Hand moves Left");
        }
        Direction = System.Math.Abs(RightHand.transform.localPosition.x) - System.Math.Abs(LeftHand.transform.localPosition.x);
        Debug.Log("HandDistance: " + Direction);
	}
}
