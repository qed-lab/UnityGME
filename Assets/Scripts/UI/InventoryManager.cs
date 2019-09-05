using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Manages the inventory game objects.
public class InventoryManager : MonoBehaviour
{
	// The interface to the command builder.
	public CommandBuilder commandBuilder;

	// The array of buttons that represent clickable inventory items.
	public Button[] inventoryItems;

	#region Properties

	// Returns the number of items in the player's inventory.
	public int InventoryCount {
		get { return inventoryCount; }
	}

	// The number of items in the player's inventory.
	private int inventoryCount;

	#endregion


	// The inventory manager requires access to the state manager.
	private StateManager stateManager;

	// A string that is used in placeholder text.
	private readonly string DASH = "-";

	// Use this for initialization
	void Start()
	{
		stateManager = GameObject.Find("Level").GetComponent<StateManager>();
		inventoryCount = 0;
	}
	
	// FixedUpdate is called exactly once per frame
	void FixedUpdate()
	{
		// Get the list of objects at the player's location (i.e. on the player)
		List<GameObject> objs = stateManager.GetObjectsAt(stateManager.PlayerName);

		// Sanity check: we can only display as many items as the size of the inventory
		if (objs != null && objs.Count <= inventoryItems.Length)
		{
			inventoryCount = objs.Count;
			int i = 0;

			// For each item in that list, find the corresponding button in the array and change its sprite.
			foreach (GameObject obj in objs)
			{
				Sprite itemSprite = obj.GetComponent<SpriteRenderer>().sprite;
				Button inventoryItem = inventoryItems[i];

				inventoryItem.GetComponent<Image>().sprite = itemSprite;
				inventoryItem.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
				inventoryItem.GetComponent<RectTransform>().sizeDelta = new Vector2(itemSprite.rect.width, itemSprite.rect.height);
				inventoryItem.GetComponent<RectTransform>().localScale = new Vector3(3f, 3f, 3f);
				inventoryItem.GetComponent<Button>().interactable = true;
				inventoryItem.name = obj.name;
				i++;
			}

			// For each other item, reset the button sprite.
			for (; i < inventoryItems.Length; i++)
			{
				Button emptyInventoryItem = inventoryItems[i];
				emptyInventoryItem.name = "Item" + DASH + (i + 1);
				emptyInventoryItem.GetComponent<Image>().sprite = null;
				emptyInventoryItem.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
				emptyInventoryItem.GetComponent<Button>().interactable = false;
			}
		}
	}

	// Sets the entity on the command builder given the index of the button that has been pressed.
	public void SetEntityInCommandBuilder(int index)
	{
		if (index >= inventoryItems.Length)
		{
			// inventory index is out of bounds, so just exit
			Debug.Log("Index " + index + " is out of bounds.");
			return;
		}
		else
		{
			Button selected = inventoryItems[index];

			if (!selected.name.Contains(DASH))
			{
				// The name will only contain a dash if the button is empty.
				// Otherwise, it represents the name of the item.
				commandBuilder.SetEntity(selected.name);
			}
		}
	}
}
