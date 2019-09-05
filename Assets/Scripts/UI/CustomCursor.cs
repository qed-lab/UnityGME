using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
	public Texture2D defaultCursorIcon;

	// Use this for initialization
	void Start()
	{
		// The hotspot is offset from the top left of the texture to use as the target point.
		Vector2 cursorHotSpot = new Vector2(defaultCursorIcon.width / 2, defaultCursorIcon.height / 2);

		// Set the cursor.
		Cursor.SetCursor(defaultCursorIcon, cursorHotSpot, CursorMode.Auto);
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}
