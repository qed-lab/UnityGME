using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A blackboard.
public class Blackboard : MonoBehaviour
{
	// The blackboard table.
	private Hashtable blackboard;

	// Use this for initialization
	void Start()
	{
		blackboard = new Hashtable();
	}

	// Puts a key, value pair into the hash table.
	public void Put(string key, object value)
	{
		blackboard.Add(key, value);
	}

	// Gets (but does not remove) the value mapped to the given key. Returns null if no such key exists.
	public object Get(string key)
	{
		if (!blackboard.ContainsKey(key))
			return null;
		else
			return blackboard[key];
	}

	public void Remove(string key)
	{
		if (blackboard.ContainsKey(key))
			blackboard.Remove(key);
	}

}
