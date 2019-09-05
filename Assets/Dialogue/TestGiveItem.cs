using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
//using UnityEngine.EventSystems.PointerEventData;

public class TestGiveItem : MonoBehaviour {

	public string itemName;
	// Use this for initialization
	void Start () {
		UnityEngine.UI.Button btn = gameObject.GetComponent<UnityEngine.UI.Button>();
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener (() => GiveItem (itemName));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void GiveItem(string name) {
		if (name == "sword") {
			DialogueLua.SetVariable ("QUARTER_MASTER_HAS_SWORD", true);
		} else if (name == "shield") {
			DialogueLua.SetVariable ("QUARTER_MASTER_HAS_SHIELD", true);
		} else if (name == "random") {
			DialogueLua.SetVariable ("QUARTER_MASTER_GIVEN_NONQUEST_ITEM", true);
		}

		Debug.Log ("giving item: " + name);
	}
}
