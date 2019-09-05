using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EndScreen : MonoBehaviour {

	Text playerIdText;

	// Use this for initialization
	void Start () {
		playerIdText = GetComponent<Text>();
		playerIdText.text = "Participant ID: " + PlayerPrefs.GetInt("participantID") + "\nPlease call the proctor before continuing.";
	}

}
