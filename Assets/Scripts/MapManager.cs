using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.PlanTools;

// Generates the mapNodes from a PDDL representation.
public class MapManager : MonoBehaviour
{
    // The unit size of the tiles.
    [HideInInspector] public Vector3 unitSize;

    // The width of the map in world coordinates.
    [HideInInspector] public float roomWidth;

    // The height of the map in world coordinates.
    [HideInInspector] public float roomHeight;

    // The number of rows of tiles in each room.
    [HideInInspector] public int numberTileRows;

    // The number of columns of tiles in each room.
    [HideInInspector] public int numberTileColums;

    // The state manager for the game.
    private StateManager stateManager;

    // The internal representation of the map.
    private Hashtable mapNodes;

    // Where the tiles are stored under the "Resources" folder.
    private static readonly string TILES_FOLDER = "Tiles/";

    // The main method for this class. Creates a level given a PDDL representation.
    public void CreateLevel( )
    {
        this.stateManager = this.gameObject.GetComponent<StateManager>();

        // Determine map-relevant sizes.
        GameObject rock = Instantiate(Resources.Load(TILES_FOLDER + "Rock", typeof(GameObject))) as GameObject;
        this.unitSize = rock.GetComponent<Renderer>().bounds.size;
        Object.Destroy(rock); // rock is no longer needed

        // Calculate the world coordinates of the on-camera screen's bounding rectangle
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.farClipPlane));
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, Camera.main.farClipPlane));
        KeyValuePair<Vector3, Vector3> screenBoundingPoints = new KeyValuePair<Vector3, Vector3>(topLeft, bottomRight);

        // Record map-wide values.
        this.roomWidth = Mathf.Abs(topLeft.x - bottomRight.x);
        this.roomHeight = Mathf.Abs(topLeft.y - bottomRight.y);
        this.numberTileColums = (int)(this.roomWidth / this.unitSize.x);
        this.numberTileRows = (int)(this.roomHeight / this.unitSize.y);

        // Create the node-based representation of the world map.
        mapNodes = CreateNodeMap();

        // Create rooms to be placed as dictated by the node-based representation.
        List<string> unplaced = new List<string>();
        List<string> placed = new List<string>();
        foreach (string location in stateManager.Locations)
        {
            GameObject room = new GameObject();
            room.name = location;
            room.tag = "Location";
            room.transform.parent = this.transform;

            if (!stateManager.At(stateManager.PlayerName).Equals(location))
            {
                unplaced.Add(location);
            }
            else
            {
                placed.Add(location);
            }

            // Create rooms based on the input prefab.
            if (stateManager.Prefab(location).Equals("sand"))
            {
                CreateSandRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("woods"))
            {
                CreateWoodsRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("cave"))
            {
                CreateCaveRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("town"))
            {
                CreateTownRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("beach"))
            {
                CreateBeachRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("cliffedge"))
            {
                CreateCliffRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("junk"))
            {
                CreateJunkRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("woodenhouse"))
            {
                CreateWoodenHouseRoom(unitSize, topLeft, bottomRight, room);
            }
            else if (stateManager.Prefab(location).Equals("brickhouse"))
            {
                CreateBrickHouseRoom(unitSize, topLeft, bottomRight, room);
            }
        }

        // Physically arrange the created rooms as given by the node map.
        PlaceRooms(unplaced, placed, mapNodes, screenBoundingPoints);

        // Refresh the state.
        stateManager.Refresh();
    }
    
    // Creates the node-based representation of the mapNodes.
    private Hashtable CreateNodeMap( )
    {
        Hashtable mapNodes = new Hashtable();
        List<string> completed = new List<string>();
        foreach (string location in stateManager.Locations)
        {
            if (!mapNodes.ContainsKey(location))
            {
                MapNode node = new MapNode();
                node.Name = location;
                List<string> connections = stateManager.Connections(location);
                foreach (string connection in connections)
                {
                    if (!mapNodes.ContainsKey(connection))
                    {
                        MapNode connectedNode = new MapNode();
                        connectedNode.Name = connection;
                        if (node.Up.Equals(""))
                        {
                            node.Up = connection;
                            connectedNode.Down = location;
                        }
                        else if (node.Down.Equals(""))
                        {
                            node.Down = connection;
                            connectedNode.Up = location;
                        }
                        else if (node.Left.Equals(""))
                        {
                            node.Left = connection;
                            connectedNode.Right = location;
                        }
                        else if (node.Right.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
                        mapNodes.Add(connection, connectedNode);
                    }
                    else
                    {
                        MapNode connectedNode = mapNodes[connection] as MapNode;
                        if (node.Up.Equals("") && connectedNode.Down.Equals(""))
                        {
                            node.Up = connection;
                            connectedNode.Down = location;
                        }
                        else if (node.Down.Equals("") && connectedNode.Up.Equals(""))
                        {
                            node.Down = connection;
                            connectedNode.Up = location;
                        }
                        else if (node.Left.Equals("") && connectedNode.Right.Equals(""))
                        {
                            node.Left = connection;
                            connectedNode.Right = location;
                        }
                        else if (node.Right.Equals("") && connectedNode.Left.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
                        mapNodes[connection] = connectedNode;
                    }
                }
                
                mapNodes.Add(location, node);
            }
            else
            {
                MapNode node = mapNodes[location] as MapNode;
                List<string> connections = stateManager.Connections(location);
                foreach (string connection in connections)
                {
                    if (!mapNodes.ContainsKey(connection))
                    {
                        MapNode connectedNode = new MapNode();
                        connectedNode.Name = connection;
                        if (node.Up.Equals(""))
                        {
                            node.Up = connection;
                            connectedNode.Down = location;
                        }
                        else if (node.Down.Equals(""))
                        {
                            node.Down = connection;
                            connectedNode.Up = location;
                        }
                        else if (node.Left.Equals(""))
                        {
                            node.Left = connection;
                            connectedNode.Right = location;
                        }
                        else if (node.Right.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
                        mapNodes.Add(connection, connectedNode);
                    }
                    else if (!completed.Contains(connection))
                    {
                        MapNode connectedNode = mapNodes[connection] as MapNode;
                        if (node.Up.Equals("") && connectedNode.Down.Equals(""))
                        {
                            node.Up = connection;
                            connectedNode.Down = location;
                        }
                        else if (node.Down.Equals("") && connectedNode.Up.Equals(""))
                        {
                            node.Down = connection;
                            connectedNode.Up = location;
                        }
                        else if (node.Left.Equals("") && connectedNode.Right.Equals(""))
                        {
                            node.Left = connection;
                            connectedNode.Right = location;
                        }
                        else if (node.Right.Equals("") && connectedNode.Left.Equals(""))
                        {
                            node.Right = connection;
                            connectedNode.Left = location;
                        }
                        mapNodes[connection] = connectedNode;
                    }
                }
                
                mapNodes[location] = node;
            }

            completed.Add(location);
        }

        return mapNodes;
    }
    
    // Creates a Sand Room
    private void CreateSandRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {    	
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Sand", "Rock", "Gate");
    }
    
    // Creates a Woods Room
    private void CreateWoodsRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Sand", "Bush", "Gate");
    }
    
    // Creates a Cave Room
    private void CreateCaveRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Gravel", "Stone", "Gate");
    }

    // Creates a Town Room
    private void CreateTownRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Pavement", "Wall", "Gate");
    }

    // Creates a Wooden House Room
    private void CreateWoodenHouseRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Plank", "Beam", "Gate");
    }

    // Creates a Brick House Room
    private void CreateBrickHouseRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Plank", "Wall", "Gate");
    }

    // Creates a Beach Room
    private void CreateBeachRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Sand", "Ocean", "Gate");
    }

    // Creates a Cliff Room
    private void CreateCliffRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Cliffrock", "Ocean", "Gate");
    }

    // Creates a Junk Room
    private void CreateJunkRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room)
    {
        CreateGenericRoom(unit, topLeft, bottomRight, room, "Junk", "Wall", "Gate");
    }

    // Creates a Room given the specification of tiles.
    // The floor tile will be what covers the ground.
    // The walls tile will be what surrounds the room across the cardinal directions.
    // The door tile will be used in case this room is connected to other rooms.
    private void CreateGenericRoom(Vector3 unit, Vector3 topLeft, Vector3 bottomRight, GameObject room, string floor, string walls, string doors)
    {		
        // Cache resources for later use.
        Object floorPrefab = Resources.Load(TILES_FOLDER + floor, typeof(GameObject));
        Object wallPrefab = Resources.Load(TILES_FOLDER + walls, typeof(GameObject));
        Object doorPrefab = Resources.Load(doors, typeof(GameObject));
        Transform roomTransform = this.transform.Find(room.name);

        // Add a room script to this room.
        roomTransform.gameObject.AddComponent<Room>();
        
        MapNode node = mapNodes[room.name] as MapNode;
        int columns = this.numberTileColums;
        int rows = this.numberTileRows;

        for (int i = 0; i < columns; i++)
        {
            float x = topLeft.x + (unit.x / 2) + (i * unit.x);
            float y = topLeft.y - (unit.y / 2);
            float z = topLeft.z;
            Vector3 position = new Vector3(x, y, z);
            GameObject instantiatedTile;

            // --------------------------------
            // ---- Top Row Tile Placement ----
            // --------------------------------

            // if there is no room above, place a wall
            if (node.Up.Equals(""))
            {
                instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
            }

            // otherwise, 
			else
            {
                bool hasDoorUp = stateManager.DoorBetween(room.name, node.Up);

                // if the room above has no door connecting to it,
                if (!hasDoorUp)
                {
                    // if we're placing tiles in columns other than the center ones, place a wall tile.
                    if (i < (columns / 2) - 1 || i > columns / 2 + 1)
                    {
                        instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                    }

					// otherwise, because there is no door, place a floor tile and make it a doorway
                    else
                    {
                        instantiatedTile = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
                        instantiatedTile.AddComponent<Doorway>().Setup(Direction.North, node.Up);
                    }
                }

                // otherwise, if the room above has a door connecting to it,
				else
                {
                    // if we're placing tiles in columns other than the center ones, place a wall tile.
                    if (i < (columns / 2) - 1 || i > columns / 2 + 1)
                    {
                        instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                    }

					// otherwise, we're adding a door in this location.
					else
                    {
                        instantiatedTile = Instantiate(doorPrefab, position, Quaternion.identity) as GameObject;

                        if (i + 1 > columns / 2 + 1)
                            instantiatedTile.transform.Rotate(0, 180, 0);

                        instantiatedTile.name = stateManager.DoorName(room.name, node.Up);

                        // Set where the door leads to.
                        instantiatedTile.GetComponent<Door>().Setup(Direction.North, node.Up);
                        stateManager.AddObjectToLocation(instantiatedTile, room.name);
                    }
                }
            }

            // set the parent of the instantiated tile to be the room transform
            instantiatedTile.transform.parent = roomTransform;

            // set the tile's script and register it in the room
            roomTransform.GetComponent<Room>().SetTile(0, i, instantiatedTile);


            // --------------------------------
            // -- Middle Rows Tile Placement --
            // --------------------------------

            bool hasDoorRight = stateManager.DoorBetween(room.name, node.Right);
            bool hasDoorLeft = stateManager.DoorBetween(room.name, node.Left);

            for (int j = 1; j < rows - 1; j++)
            {
                // Update the y coordinate of the tile, and the corresponding position.
                y = topLeft.y - (unit.y / 2) - (j * unit.y);
                position = new Vector3(x, y, z);
                instantiatedTile = null;


                // if we're at the first column and there is no room to the left, or
                // if we're at the last column and there is no room to the right, place a wall tile
                if ((i == 0 && node.Left.Equals("")) || (i > columns - 1.5 && node.Right.Equals("")))
                {
                    instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                }

                // otherwise, if we're at the first column and there is a room on the left, or
                // if we're at the last column and there is a room on the right...
				else if ((i == 0 && !node.Left.Equals("")) || (i > columns - 1.5 && !node.Right.Equals("")))
                {
                    // if we're placing tiles in rows other than the center rows, place a wall tile
                    if (j < (rows / 2) - 1 || j > rows / 2 + 1)
                    {
                        instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                    }
					
                    // otherwise, 
					else
                    {
                        // if we're at the first column and there is a door on the left, or
                        // we're at the last column and there is a door on the right,
                        // the tile to instantiate is a door
                        if ((i == 0 && hasDoorLeft || (i > columns - 1.5 && hasDoorRight)))
                        {
                            instantiatedTile = Instantiate(doorPrefab, position, Quaternion.identity) as GameObject;

                            // Rotate it, depending on whether we're placing a left door or a right door
                            if (j + 1 > rows / 2 + 1)
                                instantiatedTile.transform.Rotate(0, 0, 90);
                            else
                                instantiatedTile.transform.Rotate(0, 0, 270);

                            // Set the name, depending on whether it is a left door or a right door
                            if (i == 0)
                            {
                                // Set where the door leads to.
                                instantiatedTile.name = stateManager.DoorName(room.name, node.Left);
                                instantiatedTile.GetComponent<Door>().Setup(Direction.West, node.Left);
                            }
                            else
                            {
                                // Set where the door leads to.
                                instantiatedTile.name = stateManager.DoorName(room.name, node.Right);
                                instantiatedTile.GetComponent<Door>().Setup(Direction.East, node.Right);
                            }
								
                            // Add it as an object to the given location
                            stateManager.AddObjectToLocation(instantiatedTile, room.name);
                        }

						// otherwise, place a floor tile
						else
                        {
                            instantiatedTile = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
                            instantiatedTile.AddComponent<Doorway>();

                            if (i == 0)
                            {
                                instantiatedTile.GetComponent<Doorway>().Setup(Direction.West, node.Left);
                            }
                            else
                            {
                                instantiatedTile.GetComponent<Doorway>().Setup(Direction.East, node.Right);
                            }
                        }
                    }
                }

                // otherwise, place a floor tile
                else
                {
                    instantiatedTile = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
                    Destroy(instantiatedTile.GetComponent<BoxCollider2D>()); // remove colliders in the middle of the room
                }

                // set the parent of the instantiated tile to be the room transform
                instantiatedTile.transform.parent = roomTransform;

                // set the tile's script and register it in the room
                roomTransform.GetComponent<Room>().SetTile(j, i, instantiatedTile);
            }


            // --------------------------------
            // -- Bottom Row Tile Placement ---
            // --------------------------------

            // Once again, update the y position and the corresponding position
            y = bottomRight.y + (unit.y / 2);
            position = new Vector3(x, y, z);
            instantiatedTile = null;

            // if there is no room below,
            if (node.Down.Equals(""))
            {
                instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
            }
			
            // otherwise, if there is a room below, 
			else
            {
                bool hasDoorDown = stateManager.DoorBetween(room.name, node.Down);

                // if there is no door between this room and the room below,
                if (!hasDoorDown)
                {
                    // if we're placing tiles in columns other than the center ones, place a wall tile.
                    if (i < (columns / 2) - 1 || i > columns / 2 + 1)
                    {
                        instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                    }
						
                    // otherwise, place a floor
					else
                    {
                        instantiatedTile = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
                        instantiatedTile.AddComponent<Doorway>().Setup(Direction.South, node.Down);
                    }
						
                }

                // otherwise, if there is a door between this room and the room below,
				else
                {
                    // if we are not placing a tile in the middle columns, place a wall
                    if (i < (columns / 2) - 1 || i > columns / 2 + 1)
                    {
                        instantiatedTile = Instantiate(wallPrefab, position, Quaternion.identity) as GameObject;
                    }
						
                    // otherwise, place a door
					else
                    {
                        instantiatedTile = Instantiate(doorPrefab, position, Quaternion.identity) as GameObject;
                        						
                        if (i + 1 > columns / 2 + 1)
                            instantiatedTile.transform.Rotate(0, 180, 0);

                        instantiatedTile.name = stateManager.DoorName(room.name, node.Down);
                        instantiatedTile.GetComponent<Door>().Setup(Direction.South, node.Down);

                        stateManager.AddObjectToLocation(instantiatedTile, room.name);
                    }
                }
            }

            // set the parent of the instantiated tile to be the room transform
            instantiatedTile.transform.parent = roomTransform;

            // set the tile's script and register it in the room
            roomTransform.GetComponent<Room>().SetTile(rows - 1, i, instantiatedTile);
        }
    }

    // Places rooms in the physical world as given by the mapNodes table.
    private void PlaceRooms(List<string> unplaced, List<string> placed, Hashtable mapNodes, KeyValuePair<Vector3, Vector3> screenBoundingPoints)
    {
        Vector3 topLeft = screenBoundingPoints.Key;
        Vector3 bottomRight = screenBoundingPoints.Value;

        bool found = true;
        while (unplaced.Count > 0 && found)
        {
            found = false;
            List<string> add = new List<string>();
            List<string> rem = new List<string>();
            foreach (string location in placed)
            {
                GameObject placedObject = GameObject.Find(location);
                MapNode node = mapNodes[location] as MapNode;
                if (unplaced.Contains(node.Up))
                {
                    GameObject unplacedObject = GameObject.Find(node.Up);
                    unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.y - bottomRight.y) * Vector3.up;
                    unplaced.Remove(node.Up);
                    add.Add(node.Up);
                    found = true;
                }

                if (unplaced.Contains(node.Down))
                {
                    GameObject unplacedObject = GameObject.Find(node.Down);
                    unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.y - bottomRight.y) * Vector3.down;
                    unplaced.Remove(node.Down);
                    add.Add(node.Down);
                    found = true;
                }

                if (unplaced.Contains(node.Left))
                {
                    GameObject unplacedObject = GameObject.Find(node.Left);
                    unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.x - bottomRight.x) * Vector3.left;
                    unplaced.Remove(node.Left);
                    add.Add(node.Left);
                    found = true;
                }

                if (unplaced.Contains(node.Right))
                {
                    GameObject unplacedObject = GameObject.Find(node.Right);
                    unplacedObject.transform.position = placedObject.transform.position + Mathf.Abs(topLeft.x - bottomRight.x) * Vector3.right;
                    unplaced.Remove(node.Right);
                    add.Add(node.Right);
                    found = true;
                }

                rem.Add(location);
            }

            if (unplaced.Count > 0 && !found)
            {
                string location = unplaced[0];
                GameObject unplacedObject = GameObject.Find(location);
                unplacedObject.transform.position = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));
                unplaced.Remove(location);
                add.Add(location);
                found = true;
            }

            foreach (string addition in add)
                placed.Add(addition);

            foreach (string removal in rem)
                placed.Remove(removal);
        }
    }
}

// Represents a node in the graph that denotes the world mapNodes. Nodes in the mapNodes
// can only be connected to other mapNodes nodes along cardinal directions (N, S, E, W).
public class MapNode
{
    private string name;
    // The name of this node
    private string up;
    // The name of the node to the north
    private string down;
    // The name of the node to the south.
    private string left;
    // The name of the node to the west.
    private string right;
    // The name of the node to the east.

    public string Name {
        get { return name; }
        set { name = value; }
    }

    public string Up {
        get { return up; }
        set { up = value; }
    }

    public string Down {
        get { return down; }
        set { down = value; }
    }

    public string Left {
        get { return left; }
        set { left = value; }
    }

    public string Right {
        get { return right; }
        set { right = value; }
    }

    public MapNode( )
    {
        name = "";
        up = "";
        down = "";
        left = "";
        right = "";
    }
}
	