using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class ControllerSwitch : MonoBehaviour {

	public UnityEngine.Object one, two;// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.A)){
			GetComponent<AirConsole> ().controllerHtml = one;
		}
		if (Input.GetKeyDown (KeyCode.B)) {
			GetComponent<AirConsole> ().controllerHtml = two;
		}
	}
}
