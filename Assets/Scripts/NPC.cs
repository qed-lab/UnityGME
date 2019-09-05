using UnityEngine;
using System.Collections;
using System.Globalization;

[RequireComponent(typeof(ClickHandler))]
public class NPC : MonoBehaviour
{
	// The original description of this NPC.
	private string originalDescription;

	// Whether this NPC is asleep.
	private bool asleep;

	// title of NPC's dialogue tree
	public string conversation;

	// Called upon loading.
	void Awake()
	{
		// Store the original description.
		originalDescription = gameObject.GetComponent<ClickHandler>().description;
	}

	void Update()
	{
		// Add the name to the NPC.
		gameObject.GetComponent<ClickHandler>().description = originalDescription +
		" Goes by the name " + Capitalize(gameObject.name) + ".";
	}

	// Capitalize the given string and return it
	private string Capitalize(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}

		return char.ToUpper(s[0]) + s.Substring(1);
	}
}
