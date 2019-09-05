using System.Collections;
using System.Collections.Generic;
using Mediation.Utilities;

using UnityEngine;

// A room is a location in the world. Why is this not called "location"?  Good question.
// Anyway, all rooms are tile-based.
public class Room : MonoBehaviour
{
	#region Properties

	// The number of rows from the vertical edges of the room are reserved.
	// (Reserved rows cannot have objects instantiated within them.)
	public int NumberOfBufferRows {
		get { return numberOfBufferRows; }
	}

	// The number of rows from the horizontal edges of the room are reserved.
	// (Reserved columns cannot have objects instantiated within them.)
	public int NumberOfBufferColumns {
		get { return numberOfBufferColumns; }
	}

	// The number of rows in this room.
	public int NumberOfRows {
		get { return rows; }
	}

	// The number of columns in this room.
	public int NumberOfColumns {
		get { return columns; }
	}

	private int numberOfBufferRows;
	private int numberOfBufferColumns;
	private int rows;
	private int columns;


	#endregion

	// a matrix of the tiles that make up this room
	private GameObject[,] tileMatrix;

	// these four tuples are the tile matrix row and column entries of
	// tiles that have been designated as "lots".  A lot is a 9x9 grid
	// of tiles wherein a building can be built.  For now, I assume
	// that there will only ever be four lots, corresponding to the
	// four corners of the room.
	private Tuple<int, int> firstLotTile;
	private Tuple<int, int> secondLotTile;
	private Tuple<int, int> thirdLotTile;
	private Tuple<int, int> fourthLotTile;

	// The distance under which two tiles are said to be adjacent.
	private float adjacencyThreshold;
     
	// Use this upon loading
	void Awake()
	{
		// Upon start, lookup the tile size.  This Room should be a child of the MapManager.
		MapManager mapManager = this.GetComponentInParent<MapManager>();
		this.adjacencyThreshold = (mapManager.unitSize.x) + 0.1f;
		this.tileMatrix = new GameObject[mapManager.numberTileRows, mapManager.numberTileColums];
		this.rows = mapManager.numberTileRows;
		this.columns = mapManager.numberTileColums;
		this.numberOfBufferRows = 3;
		this.numberOfBufferColumns = 4;

		// Lot tile tracking.
		int bottomLeftRow = (6);
		int topRightColumn = (12);

		firstLotTile = Tuple.New(this.numberOfBufferRows, this.numberOfBufferColumns);
		secondLotTile = Tuple.New(this.numberOfBufferRows, topRightColumn);
		thirdLotTile = Tuple.New(bottomLeftRow, this.numberOfBufferColumns);
		fourthLotTile = Tuple.New(bottomLeftRow, topRightColumn);
	}

	// Sets the specified tile at the given location in the matrix.
	public void SetTile(int row, int column, GameObject tile)
	{
		tile.GetComponent<Tile>().rowCoordinate = row;
		tile.GetComponent<Tile>().columnCoordinate = column;
		this.tileMatrix[row, column] = tile;
	}

	// Gets the tile at the given location in the matrix.
	public GameObject GetTile(int row, int column)
	{
		return this.tileMatrix[row, column];
	}

	// Returns an array of tile game objects that are marked as available and that are adjacent to the given position.
	// Adjacency is defined by a distance threshold defined in terms of the unit size of tiles.
	public List<GameObject> GetAdjacentOpenTiles(Vector2 position)
	{
		List<GameObject> adjacentOpenTiles = new List<GameObject>();

		for (int row = 0; row < this.rows; row++)
		{
			for (int column = 0; column < this.columns; column++)
			{
				GameObject gameObjectTile = this.tileMatrix[row, column];
				float distanceBetweenPositionAndTile = Vector2.Distance(position, gameObjectTile.transform.position);

				if (distanceBetweenPositionAndTile < this.adjacencyThreshold)
				{
					Tile tile = gameObjectTile.GetComponent<Tile>();

					if (!tile.occupied)
					{
						adjacentOpenTiles.Add(gameObjectTile);
					}
				}
			}
		}

		return adjacentOpenTiles;
	}
		
	// Gets the next available lot tile. This method returns null, so watch out.
	public GameObject GetNextAvailableLotTile()
	{
		int row = -1;
		int column = -1;

		// Go through each of the lot tiles.
		if (LotIsAvailable(firstLotTile))
		{
			row = firstLotTile.First;
			column = firstLotTile.Second;
			MarkLotUnavailable(firstLotTile);
		}
		else if (LotIsAvailable(secondLotTile))
		{
			row = secondLotTile.First;
			column = secondLotTile.Second;
			MarkLotUnavailable(secondLotTile);
		}
		else if (LotIsAvailable(thirdLotTile))
		{
			row = thirdLotTile.First;
			column = thirdLotTile.Second;
			MarkLotUnavailable(thirdLotTile);
		}
		else if (LotIsAvailable(fourthLotTile))
		{
			row = fourthLotTile.First;
			column = fourthLotTile.Second;
			MarkLotUnavailable(fourthLotTile);
		}

		// If nothing is available, return null.
		if (row != -1 && column != -1)
			return tileMatrix[row, column];
		else
		{
			Debug.LogError("No lot tiles available in room");
			return null;
		}
	}





	// Checks whether the lot that is identified by the given lot tile is unoccupied.
	private bool LotIsAvailable(Tuple<int,int> lotTile)
	{
		// A lot is available if its corresponding 9x9 grid space is unoccupied.
		// The lot tile coordinate is depicted graphically below:
		//
		// X X X
		// X X X
		// X O X
		//
		// Where O is the designated lot tile.  This method returns true if every
		// tile denoted by X is unoccupied.
		int lotTileRow = lotTile.First;
		int lotTileCol = lotTile.Second;


		// Iterate the 9x9 grid from top left to bottom right.
		for (int row = lotTileRow - 2; row <= lotTileRow; row++)
		{
			for (int col = lotTileCol - 1; col <= lotTileCol + 1; col++)
			{
				Tile tile = tileMatrix[row, col].GetComponent<Tile>();

				// if any tile is occupied, the lot is unavailable.
				if (tile.occupied)
					return false;
			}
		}

		return true;
	}

	// Marks the lot identified by the given tile as occupied.
	private void MarkLotUnavailable(Tuple<int,int> lotTile)
	{
		// The lot tile coordinate is depicted graphically below:
		//
		// X X X
		// X X X
		// X O X
		//
		// Where O is the designated lot tile.


		int lotTileRow = lotTile.First;
		int lotTileCol = lotTile.Second;

		// Iterate the 9x9 grid from top left to bottom right.
		for (int row = lotTileRow - 2; row <= lotTileRow; row++)
		{
			for (int col = lotTileCol - 1; col <= lotTileCol + 1; col++)
			{
				// Mark the tile occupied.
				Tile tile = tileMatrix[row, col].GetComponent<Tile>();

				if(tile != null)
					tile.occupied = true;
			}
		}

		// Mark the row in front of the building unavailable too.
		int frontOfBuildingRow = lotTileRow + 1;

		for (int col = lotTileCol - 1; col <= lotTileCol + 1; col++) 
		{
			// Mark these tiles as occupied and as in front of a building.
			Tile tile = tileMatrix[frontOfBuildingRow, col].GetComponent<Tile>();

			if (tile != null) {
				tile.occupied = true;
			}
		}
	}
		
	// Mark the surrounding eight tiles as occupied.
	public void MarkNeighborsOccupied(GameObject tile)
	{
		// The tile coordinate is depicted graphically below:
		//
		// X X X
		// X O X
		// X X X
		// Where O is the tile. All X's must be marked as occupied.

		// Get the tile row and column.
		int tileRow = tile.GetComponent<Tile>().rowCoordinate;
		int tileColumn = tile.GetComponent<Tile>().columnCoordinate;

		// Get the starting and ending indices for blocking out neighbors.
		int rowStart =  InBoundsRow(tileRow - 1);
		int columnStart = InBoundsColumn(tileColumn - 1);

		int rowEnd = InBoundsRow(tileRow + 1);
		int columnEnd = InBoundsColumn(tileColumn + 1);

		// Block out all tiles in the range.
		for (int row = rowStart; row <= rowEnd; row++)
			for (int column = columnStart; column <= columnEnd; column++)
				GetTile(row, column).GetComponent<Tile>().occupied = true;
		
	}


	// If the row value exceeds the bound, returns the boundary value.
	// Otherwise, returns the input. A row index cannot be less than zero, nor
	// greater than the number of rows - 1.
	private int InBoundsRow(int row)
	{
		return row < 0 ? 0 : (row > rows - 1 ? rows - 1 : row);
	}

	// If the column value exceeds the bound, returns the boundary value. 
	// Otherwise returns the input. A column index cannot be less than zero, nor
	// greater than the number of columns - 1.
	private int InBoundsColumn(int column)
	{
		return column < 0 ? 0 : (column > columns - 1 ? columns - 1 : column);
	}



}
