using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueSFX : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log("Start DialogueSFX");	
	}
	
	// Update is called once per frame
	void Update () {
//		Debug.Log("playing? " + this.IsPlaying);
	}

	void OnEnable()
	{
		Debug.Log(System.DateTime.Now);
		Debug.Log("OnEnable");
	}

	void OnDisable()
	{
		Debug.Log(System.DateTime.Now);
		Debug.Log("OnDisable");
	}

	void Pause()
	{
		Debug.Log("Pause");
	}
	public void PrintThing()
	{
		Debug.Log("PrintThing");
	}

//	void onEnd()
//	{
//		Debug.Log("onEnd");
//	}
}
