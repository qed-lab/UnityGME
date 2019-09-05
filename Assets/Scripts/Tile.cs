using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A tile is an individual location within a room.
public class Tile : MonoBehaviour
{
    // Whether or not this tile is occupied.
    public bool occupied;

	// This tile's row coordinate. This only makes sense in the context of a room, 
	// because it's a relative index.
	public int rowCoordinate;

	// This tile's column coordinate. This only makes sense in the context of a room,
	// because it's a relative index.
	public int columnCoordinate;
			
    // Called upon loading.
    void Awake( )
    {
        occupied = (gameObject.tag.Equals("Navigation")) ? false : true;
    }
        
}
