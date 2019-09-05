using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.Interfaces;
using Mediation.PlanTools;

// Instantiates and manages Unity game objects based on the underlying predicate state from GME.
public class StateManager : MonoBehaviour
{
	// Audio clips to play in different parts of the world.
	public AudioClip inTown;
	public AudioClip indoors;
	public AudioClip beach;

	// A reference to the camera's audio source.
	private AudioSource mainCameraMusicPlayer;

	// A reference to the blackboard
	private Blackboard blackboard;

	// A reference to the problem.
	private Problem problem;

	// The current state of the world from GME.
	private List<IPredicate> predicates;

	// A table of Unity game objects, stored by location.
	private Hashtable objects = new Hashtable();

	// A table of enabled object properties, stored by object.
	private Hashtable properties = new Hashtable();

	// Tracks whether it's the player's turn to act.
	private bool playerTurn = true;

	// A cache of the player's location.
	private string playerLocationCache;

	// An interface for the player's turn variable.
	public bool PlayerTurn {
		get { return playerTurn; }
		set { playerTurn = value; }
	}
	
	// An interface for the current world state predicates.
	public List<IPredicate> Predicates {
		get { return predicates; }
		set { predicates = value; }
	}

	// A reference to the problem.
	public Problem Problem {
		get { return problem; }
		set { problem = value; }
	}
	
	// Exposes the GetPlayer() method.
	public string PlayerName {
		get { return GetPlayer(); }
	}

	// Exposes the GetLocations() method.
	public List<string> Locations {
		get { return GetLocations(); }
	}

	// Returns a list of strings that denote entrances.
	public List<string> Entrances {
		get { return GetEntrances(); }
	}

	// Returns the Player Script
	public Player Player {
		get { return GameObject.Find(PlayerName).GetComponent<Player>(); }
	}

	// Returns the Player's GameObject
	public GameObject PlayerGameObject {
		get { return GameObject.Find(PlayerName); }
	}

	// Use this for initialization.
	void Awake()
	{
		blackboard = GameObject.Find("Blackboard").GetComponent<Blackboard>();
		mainCameraMusicPlayer = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
		playerLocationCache = "";
	}
	
	// Updates the Unity scene to match the underlying predicate state description.
	public void Refresh()
	{
		// Syncronize Unity game objects with predicate's state.
		RefreshObjects();
		
		// Syncronize the Unity player inventory with predicate's state.
		RefreshInventory();
		
		// Syncronize Unity game object properties with predicate's state.
		RefreshProperties();

		// Syncronize the background music with predicate's state.
		RefreshBackgroundMusic();
	}

	// Updates the background music on the basis of the player's current location.
	private void RefreshBackgroundMusic()
	{
		string playerLocation = At(PlayerName);

		if (!playerLocationCache.Equals(playerLocation))
		{
			// Check if the location change represents a shift between the general areas
			// beach / indoors / outdoors
			if (IsBeach(playerLocation))
			{
				// If we are at the beach and were not at the beach before, change the music.
				if (!IsBeach(playerLocationCache))
				{
					mainCameraMusicPlayer.Stop();
					mainCameraMusicPlayer.clip = beach;
					mainCameraMusicPlayer.Play();               
				}
			}
			else if (IsIndoors(playerLocation))
			{
				// If we are indoors and were not indoors before, change the music.
				if (!IsIndoors(playerLocationCache))
				{
					mainCameraMusicPlayer.Stop();
					mainCameraMusicPlayer.clip = indoors;
					mainCameraMusicPlayer.Play();
				}
			}
			else
			{
				// If we are outdoors and were not outdoors before, change the music.
				if (!IsOverworld(playerLocationCache))
				{
					mainCameraMusicPlayer.Stop();
					mainCameraMusicPlayer.clip = inTown;
					mainCameraMusicPlayer.Play();
				}
			}

			// Update the cache.
			playerLocationCache = playerLocation;
		}
	}

	// Whether the given location is at a location wit a beach.
	private bool IsBeach(string location)
	{
		return location.Equals("docks") || location.Equals("cliff") || location.Equals("junkyard");
	}

	// Whether the given location is indoors according to the map.
	public bool IsIndoors(string location)
	{
		return location.Equals("bar")
		|| location.Equals("forge")
		|| location.Equals("mansion")
		|| location.Equals("fort")
		|| location.Equals("shop")
		|| location.Equals("bank")
		|| location.Equals("hut")
		|| location.Equals("basement")
		|| location.Equals("storage");
	}

	// Whether the given location is the tutorial according to the map.
	public bool IsTutorialLocation(string location)
	{
		return location.Equals("basement") || location.Equals("storage");
	}

	// Whether the given location is in the overworld according to the map.
	private bool IsOverworld(string location)
	{
		// By definition, if the player is not at the beach and not indoors, the player is in the overworld.
		return (!IsBeach(location) && !IsIndoors(location));
	}

	// Whether the given location is outdoors according to the map.
	public bool IsOutdoors(string location)
	{
		return (IsBeach(location) || IsOverworld(location));
	}

	// Updates Unity game objects to match the predicate description.
	public void RefreshObjects()
	{
		// Store the room the player is currently at.
		string location = At(PlayerName);

		// Store a list of game objects that are colocated with the player in the Unity scene.
		List<GameObject> locationObjects = GetObjectsAt(location);
		
		// Store a list of names of objects that are colocated with the player in the predicate state.
		List<string> things = ThingsAt(location);
		
		// Create a list of Unity objects to remove from the scene.
		List<GameObject> remove = new List<GameObject>();

		#region Object Removal

		// If there are objects colocated with the player.
		if (locationObjects != null)
		{
			// Identify objects to remove:
			// Loop through the colocated objects.
			foreach (GameObject obj in locationObjects)
			{
				// If the current object is not in the underlying predicate state...
				if (!things.Contains(obj.name))
				{
					// Add the object to the list of those to remove.
					remove.Add(obj);
				}
			}

			// Remove identified objects:
			// Loop through the objects to be removed from the scene.
			foreach (GameObject obj in remove)
			{
				// Remove the object from the list of location objects.
				locationObjects.Remove(obj);

				// Destory the object.
				Object.Destroy(obj);
			}
		}

		#endregion
			
		#region Object Addition

		// If there are objects colocated with the player in the predicate state...
		if (things != null)
		{
			// Loop through the set of colocated objects.
			foreach (string thing in things)
			{
				// If there are objects colocated with the player in the Unity scene...
				if (locationObjects != null)
				{
					// If the current predicate object is not in the Unity scene...
					if (locationObjects.Find(x => x.name.Equals(thing)) == null)
						// Instantiate the current predicate object as a Unity game object colocated with the player.
						locationObjects.Add(InstantiateAt(thing, location));
				}
				else
				{
					// Create a new list of game objects.
					locationObjects = new List<GameObject>();
					
					// Instantiate the current predicate object as a Unity game object colocated with the player.
					locationObjects.Add(InstantiateAt(thing, location));
				}
			}
		}

		#endregion
		
		// Associate the current location objects with their location.
		SetObjectsAt(location, locationObjects);
	}
	
	// Updates Unity game object properties to match the predicate description.
	public void RefreshProperties()
	{		
		// Loop through the objects colocated with the player in the current predicate state.
		foreach (string thing in ThingsAt(At(PlayerName)))
		{
			// Store the current object's properties from the predicate state.
			List<string> currentProperties = Properties(thing);
			
			// Store the current object's stored properties from the last update.
			List<string> propertiesHash = properties[thing] as List<string>;
			
			// A list of properties to add to the object.
			List<string> newProperties = new List<string>();
			
			// A list of properties to remove from the object.
			List<string> oldProperties = new List<string>();
			
			// If the object has previously stored properties...
			if (propertiesHash != null)
			{
				// Loop through each stored property.
				foreach (string property in propertiesHash)
					// If the property does not persist in the current state...
					if (!currentProperties.Contains(property))
						// Add the property to the remove list.
						oldProperties.Add(property);
						
				// Loop through each current property.
				foreach (string property in currentProperties)
					// If the property is new...
					if (!propertiesHash.Contains(property))
						// Add the property to the add list.
						newProperties.Add(property);
			}
			// Otherwise, if there are no stored properties...
			else
				// Add all current properties to the add list.
				newProperties = currentProperties;
			
			// Find the Unity game object that corresponds with the current object.
			List<GameObject> thingGOs = new List<GameObject>();

			if (!Prefab(thing).Equals("gate"))
				thingGOs.Add(GameObject.Find(thing));
			else
				foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Door"))
					if (obj.name.Equals(thing))
						thingGOs.Add(obj);
			
			foreach (GameObject thingGO in thingGOs)
			{				
				// Get the game object's animator.
				Animator animator = thingGO.GetComponent<Animator>();
				
				// If the object has an animator...
				if (animator != null)
				{
					// Turn off warning logging in case we touch properties that don't exist.
					animator.logWarnings = false;
					
					// Loop through the new properties.
					foreach (string property in newProperties)
						// Set the new properties to true.
						animator.SetBool(property, true);
						
					// Loop through the old properties.
					foreach (string property in oldProperties)
						// Set the old properties to false.
						animator.SetBool(property, false);
						
					animator.logWarnings = true;
				}
			}
			
			// Store the object's current properties in the hashtable.
			properties[thing] = currentProperties;
		}
	}
	
	// Updates Unity's player inventory to match the predicate description.
	public void RefreshInventory()
	{
		// Store a list of the current Unity player inventory.
		List<GameObject> inventoryObjects = GetObjectsAt(PlayerName);
		
		// Store a list of the predicate player inventory.
		List<string> things = Has(PlayerName);
		
		// Create a list of objects to remove from Unity's player inventory.
		List<GameObject> remove = new List<GameObject>();
		
		// If the inventory is not empty...
		if (inventoryObjects != null)
		{
			// Loop through the game objects in Unity's inventory.
			foreach (GameObject obj in inventoryObjects)
				// If the current object is not in the predicate list...
				if (!things.Contains(obj.name))
					// Add the object to the remove list.
					remove.Add(obj);
			
			// Loop through the game objects in the remove list.
			foreach (GameObject obj in remove)
			{
				// Remove the object from Unity's player inventory.
				inventoryObjects.Remove(obj);
				
				// Destory the object.
				Object.Destroy(obj);
			}
		}
		
		// If the player has at least one thing in their inventory in the predicate state...
		if (things != null)
		{
			// Loop through the things the player is carrying in the predicate state.
			for (int i = 0; i < things.Count; i++)
			{
				// If the player has at least one thing in their Unity inventory...
				if (inventoryObjects != null)
				{
					// If the current predicate object is not in the user's Unity inventory...
					if (inventoryObjects.Find(x => x.name.Equals(things[i])) == null)
					{
						// Instantiate the object and add it to the inventory list.
						inventoryObjects.Add(InstantiateInventoryItem(things[i], i));
					}
				}

				// Otherwise, if the player has nothing in their inventory...
				else
				{
					// Initialize the Unity inventory list.
					inventoryObjects = new List<GameObject>();
					
					// Instantiate the object and add it to the inventory list.
					inventoryObjects.Add(InstantiateInventoryItem(things[i], i));
				}
			}
		}
		
		// Store the inventory objects under the player's name.
		SetObjectsAt(PlayerName, inventoryObjects);
	}

	// Returns the type of the given object.
	public string TypeOf(string thing)
	{
		// Iterate the dictionary of objects by type,
		foreach (DictionaryEntry typeObjects in problem.ObjectsByType)
		{
			// The value represents the objects of the type given by the key
			List<string> objects = typeObjects.Value as List<string>;

			// If the value contains the object we're looking for,
			if (objects.Contains(thing))
				// the key represents the type, so return it.
				return typeObjects.Key as string;
		}

		return "";
	}


	// Returns the location an object is at in the current predicate state.
	public string At(string thing)
	{
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'at'...
			if (pred.Name.Equals("at"))
				// If the first term is equal to the object...
				if (pred.TermAt(0).ToString().Equals(thing))
					// Return the second predicate term.
					return pred.TermAt(1).ToString();
		
		// We couldn't find what we were looking for.
		return "";
	}

	// Returns true if (wants-item ?character ?item) is true in the current predicate state.
	public bool WantsItem(string character, string item)
	{
		// Loop through the current predicates
		foreach (Predicate pred in predicates)
		{
			// If the predicate is 'wants-item'
			if (pred.Name.Equals("wants-item"))
			{
				// And the terms are the given character and item,
				if (pred.TermAt(0).ToString().Equals(character) && pred.TermAt(1).ToString().Equals(item))
				{
					// Then the character wants it.
					return true;
				}
			}
		}

		// We couldn't find (wants-item ?character ?item) in the current state.
		return false;
	}
	
	// Returns a list of all inventory items of a character in the current predicate state.
	public List<string> Has(string character)
	{
		// Create a new list to hold the items.
		List<string> inventory = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'has'...
			if (pred.Name.Equals("has"))
				// If the first term is the character...
				if (pred.TermAt(0).ToString().Equals(character))
					// Add the second term of the predicate to the list.
					inventory.Add(pred.TermAt(1).ToString());
		
		if (inventory != null)
			inventory = inventory.OrderBy(o => o).ToList();
		
		// Return the inventory list.
		return inventory;
	}

	// Returns the game object type of a predicate state object.
	public string Prefab(string thing)
	{
		// Loops through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'type'...
			if (pred.Name.Equals("prefab"))
				// If the first term is the object...
				if (pred.TermAt(0).ToString().Equals(thing))
					// Return the second term.
					return pred.TermAt(1).ToString();
        
		// We couldn't find what we were looking for.
		return "";
	}
	
	// Returns a list of properties of a predicate state object.
	public List<string> Properties(string thing)
	{
		// Create a new list.
		List<string> properties = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's first term equals the object...
			if (pred.TermAt(0).ToString().Equals(thing))
				// If the predicate has an arity of one...
				if (pred.Arity == 1)
					// Add the predicate's name to the list.
					properties.Add(pred.Name);
		
		// Return the list of properties.
		return properties;
	}
	
	// Return a list of all things at a location in the predicate state.
	public List<string> ThingsAt(string room)
	{
		// Make a new list.
		List<string> things = new List<string>();
		
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
		{
			// If the predicate's name is 'at'...
			if (pred.Name.Equals("at"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).ToString().Equals(room))
                    // Add the first term to the list.
                    things.Add(pred.TermAt(0).ToString());
			}
			else if (pred.Name.Equals("doorbetween"))
			{
				// If the second term is equal to the location...
				if (pred.TermAt(1).ToString().Equals(room))
                    // Add the first term to the list.
                    things.Add(pred.TermAt(0).ToString());
			}
		}

		// Sort the list of things at the room such that entrances and exits are at 
		// the front of the list.  This is so we can generate entrances and exits
		// (as well as associated buildings) first, in order to avoid situations
		// where we cannot generate a room due to items or avatars scattered about.
		things = things.OrderByDescending(
			x => (x.Contains("entrance") || x.Contains("exit"))
		).ToList();

			
		// Return the list of objects.
		return things;
	}
	
	// Checks if there is a door between two locations.
	public bool DoorBetween(string room1, string room2)
	{
		foreach (Predicate pred in predicates)
		{
			if (pred.Name.Equals("doorbetween"))
			{
				if (pred.TermAt(1).ToString().Equals(room1) && pred.TermAt(2).ToString().Equals(room2))
					return true;   
			}
		}
			
		return false;
	}

	// Returns the name of the door connecting the two rooms, or the empty string if it fails to find it.
	public string DoorName(string room1, string room2)
	{
		foreach (Predicate pred in predicates)
			if (pred.Name.Equals("doorbetween"))
			if (pred.TermAt(1).ToString().Equals(room1) && pred.TermAt(2).ToString().Equals(room2))
				return pred.TermAt(0).ToString();
		
		return "";
	}
        
	// Returns the name of the object that represents the symmetric exit to the entrance at the given location,
	// or the empty string if it ifails to find it.
	public string SymmetricExitName(string entranceLocation, string entranceLeadsToLocation)
	{
		// We're looking for ?f such that:
		// 
		// at ?e ?entranceLocation
		// leadsto ?e ?entranceLeadsToLocation
		// at ?f ?entranceLeadsToLocation
		// leads to ?f ?entranceLocation
		//
		// This method assumes that both:
		// (at ?e ?entranceLocation), and 
		// (leadsto ?e ?entranceLeadsToLocation) is true for some ?e.
		foreach (Predicate p in predicates)
		{
			if (p.Name.Equals("leadsto"))
			if (p.TermAt(1).ToString().Equals(entranceLocation))
			if (ThingsAt(entranceLeadsToLocation).Contains(p.TermAt(0).ToString()))
				return p.TermAt(0).ToString();
		}


		return "";
	}

	// Returns the name of the room the entrance leads to, or the empty string if it fails to find it.
	public string EntranceLeadsTo(string entrance)
	{
		foreach (Predicate pred in predicates)
		{
			if (pred.Name.Equals("leadsto"))
			{
				if (pred.TermAt(0).ToString().Equals(entrance))
					return pred.TermAt(1).ToString();    
			}
		}
            
		return "";
	}

	// Checks if the given string is an entrance at the given room
	public bool IsEntranceAt(string entrance, string room)
	{
		bool objectIsInRoom = ThingsAt(room).Contains(entrance);
		bool isOfEntranceType = (problem.ObjectsByType["entrance"] as List<string>).Contains(entrance);
		return objectIsInRoom && isOfEntranceType;
	}
	
	// Returns a list of connected rooms in the predicate state.
	public List<string> Connections(string room)
	{
		// Create a new list.
		List<string> connections = new List<string>();
		
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate name is 'connected'...
			if (pred.Name.Equals("connected"))
				// If the first term is the location...
				if (pred.TermAt(0).ToString().Equals(room))
					// Add the second term to the list.
					connections.Add(pred.TermAt(1).ToString());
        
		// Return the list of connected locations.
		return connections;
	}
	
	// Creates a hashtable of all connections in the predicate state.
	public Hashtable AllConnections()
	{
		// Create a new hashtable.
		Hashtable connections = new Hashtable();
		
		// Loop through the locations listed in the state.
		foreach (string location in GetLocations())
			// Add each location, connections pair to the hashtable.
			connections.Add(location, Connections(location));

		// Return the table of connections.
		return connections;
	}
	
	// Place an object at a location.
	public void PutObject(GameObject obj, string location)
	{
		// If the location exists in the objects table...
		if (objects.ContainsKey(location))
		{
			// Pull the location's current objects from the table.
			List<GameObject> locationObjects = objects[location] as List<GameObject>;
			
			// If there are objects at the location...
			if (locationObjects != null)
			{
				// If the current object is not at the location...
				if (!locationObjects.Contains(obj))
				{
					// Add the object to the location.
					locationObjects.Add(obj);

					// Push the object list back to the table.
					SetObjectsAt(location, locationObjects);
				}
			}
		}
		else
		{
			// Create a new list of objects.
			List<GameObject> locationObjects = new List<GameObject>();
			
			// Add the current object to the list.
			locationObjects.Add(obj);
			
			// Push the list to the table.
			objects.Add(location, locationObjects);
		}
	}
	
	// Remove a game object from a location in the table.
	public void RemoveObject(string objName, string location)
	{
		// If the table contains the location...
		if (objects.ContainsKey(location))
		{
			// Store the list of objects at that location.
			List<GameObject> locationObjects = objects[location] as List<GameObject>;
			
			// Find the game object in the list.
			GameObject obj = locationObjects.Find(x => x.name.Equals(objName));
			
			// Remove the object from the list.
			locationObjects.Remove(obj);
			
			// Push back the updated list.
			SetObjectsAt(location, locationObjects);
		}
	}
	
	// Returns the game objects at a location from the table.
	public List<GameObject> GetObjectsAt(string location)
	{
		return objects[location] as List<GameObject>;
	}

	public void AddObjectToLocation(GameObject obj, string location)
	{
		List<GameObject> objs = new List<GameObject>();
		
		if (objects.ContainsKey(location))
			objs = objects[location] as List<GameObject>;
			
		objs.Add(obj);
		SetObjectsAt(location, objs);
	}
	
	// Store a set of objects at a location.
	public void SetObjectsAt(string location, List<GameObject> objs)
	{
		objects[location] = objs;
	}

	// Get all the entrances from the current predicate state.
	private List<string> GetEntrances()
	{
		// Create a new list.
		List<string> entrances = new List<string>();

		// Add objects registered with the type 'entrance' to the list.
		if (problem.ObjectsByType["entrance"] != null)
			entrances.AddRange(problem.ObjectsByType["entrance"] as List<string>);

		// Return the list.
		return entrances;
	}

	// Get all the locations from the current predicate state.
	private List<string> GetLocations()
	{
		// Create a new list.
		List<string> locations = new List<string>();
		
		// Loop through the current predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'location'...
			if (pred.Name.Equals("location"))
				// Add the first term to the list.
				locations.Add(pred.TermAt(0).ToString());

		// Add objects registered as locations to the list.
		if (problem.ObjectsByType["location"] != null)
			locations.AddRange(problem.ObjectsByType["location"] as List<string>);
	
		// Return the list.
		return locations;
	}

	// Get the player's name from the current predicate state.
	private string GetPlayer()
	{
		// Loop through the predicates.
		foreach (Predicate pred in predicates)
			// If the predicate's name is 'player'...
			if (pred.Name.Equals("player"))
				// Return the first term.
				return pred.TermAt(0).ToString();
        
		// We couldn't find what we were looking for.
		return "";
	}
    
	// Given a Unity location chunk, returns an open tile.
	public GameObject GetOpenTile(GameObject chunk, bool markNeighboorsOccupied)
	{
		// Create a list to store open tiles in the Unity location.
		List<GameObject> validTiles = new List<GameObject>();

		// The chunk corresponds to a room.
		Room room = chunk.GetComponent<Room>();

		// Create a buffer of tiles so that items are never instantiated next to the walls
		int rowBuffer = room.NumberOfBufferRows;
		int colBuffer = room.NumberOfBufferColumns;

		// For reach tile in the given room (excluding the buffer zone)
		for (int i = rowBuffer; i < room.NumberOfRows - rowBuffer; i++)
		{
			for (int j = colBuffer; j < room.NumberOfColumns - colBuffer; j++)
			{
				// Get the corresponding game object at that location.
				GameObject tileGO = room.GetTile(i, j);

				// If the tile is not occupied, it is a candidate open tile.
				if (!tileGO.GetComponent<Tile>().occupied)
				{
					validTiles.Add(tileGO);
				}
			}
		}

		// Get one tile at random from the list of open tiles we compiled.
		GameObject tile = validTiles[Random.Range(0, validTiles.Count)];

		// Mark that tile as occupied.
		tile.GetComponent<Tile>().occupied = true;

		// If the flag is set, mark the neighbors as occupied as well.
		if(markNeighboorsOccupied) {
			room.MarkNeighborsOccupied(tile);
		}

		// Return it.
		return tile;
	}

	// Given a Unity location chunk, returns a specific kind of open tile,
	// capable of supporting a building.
	public GameObject GetOutdoorLotTile(GameObject chunk)
	{
		// The chunk corresponds to a room.
		Room room = chunk.GetComponent<Room>();

		// Attempt to get a tile that corresponds to an outdoor lot.
		GameObject lotTile = room.GetNextAvailableLotTile();

		if (lotTile == null)
		{
			Debug.LogError("No lot tile available in room " + chunk.name + ". Returning empty GameObject.");
			return new GameObject();
		}
		else
			return lotTile;
	}

	// Given a Unity location chunk and a tile in that chunk, get the tile directly beneath it (same col, next row).
	// This method wins the award for the most obtuse name of all time.
	public GameObject MarkTileBelowAsOccupiedAndReturnIt(GameObject chunk, GameObject tileGO)
	{
		// The chunk corresponds to a room.
		Room room = chunk.GetComponent<Room>();

		for (int i = 0; i < room.NumberOfRows; i++)
		{
			for (int j = 0; j < room.NumberOfColumns; j++)
			{
				// Get the corresponding tile at that location.
				GameObject roomTileGO = room.GetTile(i, j);

				// If it matches the tile we input,
				if (tileGO.Equals(roomTileGO))
				{
					// Boundary check: we can't get a tile below the bottom row.
					if (i + 1 != room.NumberOfRows)
					{
						// Get the tile below the input tile,
						GameObject tileBelow = room.GetTile(i + 1, j);

						// Mark it as occupied,
						if (!tileBelow.GetComponent<Tile>().occupied)
							tileBelow.GetComponent<Tile>().occupied = true;

						// And return it.
						return tileBelow;
					}
				}
			}
		}

		// If we've come this far, we got nothing.
		return tileGO;
	}


	// Given a predicate object name and a location, instantiate a Unity game object at that location.
	public GameObject InstantiateAt(string thing, string location)
	{
		// Find and store the Unity game object for the given location.
		GameObject locationGameObject = GameObject.Find(location);

		GameObject thingGO;

		// If an item has been placed in the blackboard, then there is a specific spawn position for this item.
		// This is primarily used for instantiating the player at a specific location across rooms as well as
		// entrances that need to be placed relative to building sprites.
		if (blackboard != null && blackboard.Get(thing) != null)
		{
			// Get the spawn position.
			Vector3 spawnPosition = (Vector3)blackboard.Get(thing);

			// Remove the spawn position
			blackboard.Remove(thing);

			// Special case for trying to instantiate buildings,
			if (TypeOf(thing).Equals("entrance") && !IsIndoors(location))
			{
				// Instntiate a new game object that matches the predicate object's type at the chosen tile.
				thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), spawnPosition, Quaternion.identity) as GameObject;

				// Get the location where this door is heading.
				string buildingName = EntranceLeadsTo(thing);

				// Since we're building an entrance in an outdoor location, 
				// we need to find and enable the right building sprite.
				thingGO.GetComponentInChildren<Building>().SetBuildingSprite(buildingName);
			}

            // Otherwise,
            else
			{
				// Instantiate a new game object that matches the predicate object's type at the chosen tile.
				thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), spawnPosition, Quaternion.identity) as GameObject;    
			}

		}

        // Special case for trying to instantiate entrances.
        else if (TypeOf(thing).Equals("entrance"))
		{
			// Store the tile.
			GameObject tileGO;

			// If the entrance is in an outdoor location, then we're actually going to instantiate a building around the entrance.
			// The building is for flavor, but helps segment that the player will be going to a different location.
			if (!IsIndoors(location))
			{
				// Get an open lot tile.
				tileGO = GetOutdoorLotTile(locationGameObject);

				// Instntiate a new game object that matches the predicate object's type at the chosen tile.
				thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), tileGO.transform.position, Quaternion.identity) as GameObject;

				// Get the location where this door is heading.
				string buildingName = EntranceLeadsTo(thing);

				// Since we're building an entrance in an outdoor location, we need to find and enable the right building sprite.
				thingGO.GetComponentInChildren<Building>().SetBuildingSprite(buildingName);
			}

			// If we're indoors, we do the regular thing, but we also block the surrounding 8 tiles so that there aren't any
			// NPCs in the adjacent tiles (meaning that it's always ok to instantiate the player anywhere in those eight
			// tiles.
			else
			{
				// Get an open tile and mark neighbors as occipied.
				bool markNeighboorsAsOccupied = true;
				tileGO = GetOpenTile(locationGameObject, markNeighboorsAsOccupied);

				// Instantiate a new game object that matches the predicate object's type at the chosen tile.
				thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), tileGO.transform.position, Quaternion.identity) as GameObject;
			}

			MarkTileBelowAsOccupiedAndReturnIt(locationGameObject, tileGO); // this marks the tile below as occupied.
		}
		else
		{
			// Get an open tile
			GameObject tileGO = GetOpenTile(locationGameObject, false);

			// Instantiate a new game object that matches the predicate object's type at the chosen tile.
			thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), tileGO.transform.position, Quaternion.identity) as GameObject;
		}



		// Set the game object's parent to the room.
		thingGO.transform.parent = locationGameObject.transform;
		
		// Set the new game object's name to that of the predicate object.
		thingGO.name = thing;
		
		// Return the new game object.
		return thingGO;
	}
    
	// Given a predicate object name and a position, instantiate it in the player's inventory.
	private GameObject InstantiateInventoryItem(string thing, int position)
	{
		// Find the inventory game object.
		GameObject inventoryGO = GameObject.Find("InventoryContainer");
		
		// Instantiate the inventory object at the specified position.
		GameObject thingGO = Instantiate(Resources.Load(Prefab(thing), typeof(GameObject)), inventoryGO.transform) as GameObject;

		// Set the game object's name to the predicate name.
		thingGO.name = thing;

		// Disable the sprite renderer so it is invisible.
		thingGO.GetComponent<SpriteRenderer>().enabled = false;
		
		// Return the instantiated object.
		return thingGO;
	}

	public string PositionToLocation(Vector3 position)
	{
		// Find all level chunks.
		GameObject[] locationGameObjects = GameObject.FindGameObjectsWithTag("Location");
		
		// The room string.
		string room = "";
		
		// The room distance.
		float distance = Mathf.Infinity;
		
		// Loop through the location game objects.
		foreach (GameObject locationGameObject in locationGameObjects)
		{
			// Check the old room distance.
			if (Vector3.Distance(position, locationGameObject.transform.position) < distance)
			{
				room = locationGameObject.name;
				distance = Vector3.Distance(position, locationGameObject.transform.position);
			}
		}
		
		return room;
	}
}
