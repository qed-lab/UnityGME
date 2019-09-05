using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class MyLuaFunctions : MonoBehaviour {
	
	void OnEnable() {
		Lua.RegisterFunction("GoToScene", this, typeof(MyLuaFunctions).GetMethod("GoToScene"));
	}

	void OnDisable() {
		Lua.UnregisterFunction("GoToScene");
	}

	public void GoToScene(string name) {
		SceneManager.LoadScene(name);
	}
}