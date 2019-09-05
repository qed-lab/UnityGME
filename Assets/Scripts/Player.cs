using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Mediation.KnowledgeTools;
using Mediation.Utilities;
using PixelCrushers.DialogueSystem;

// Handles player behavior and calls the mediator on transition events.
public class Player : MonoBehaviour
{
	// Audio clips to play when actions succeed.
	public AudioClip pickUp;
	public AudioClip open;
	public AudioClip close;
	public AudioClip give;
	public AudioClip unlockDoor;
	public AudioClip wakePerson;

	// Whether the player object can act.
	public bool CanAct { 
		get { return canAct; }
	}

	// Whether the player object can move.
	public bool CanMove { 
		get { return canMove; }
	}

	private bool canAct;
	private bool canMove;

	private Mediator mediator;
	private StateManager stateManager;
	private InventoryManager inventoryManager;
	private CameraController cameraController;
	private Text command;
	private Blackboard blackboard;
	private bool isFullyInstantiated;
	private bool isOverencumbered;

	// Controls how fast the player's character moves.
	private float maxPlayerSpeed = 1.6f;
	private float playerSpeed = 0f;
	private float itemSpeedPenalty = 0.2f;

	// The player's animator.
	private Animator animator;

	// Game objects the player's character is colliding with.
	private List<GameObject> colliding;

	// The auxiliary script that keeps track of the tutorial for this player.
	private TutorialGame tutorialGame;

	// A record of the last action that was attempted by the player.
	private string action;

	// A reference to the quest action handler script to determine if the player is in conversation.
	private QuestActionHandler questActionHandler;

	#region MonoBehaviour Methods

	// Called upon load.
	void Awake()
	{
		// Register that the player is not fully instantiated.
		isFullyInstantiated = false;

		// Register that the player is not overencumbered.
		isOverencumbered = false;

		// Disallow the player to move and act by default.
		// This is to avoid a bug that occurs when you try to move the player's character before it is rendered.
		Disable();

		// Create an empty list of colliding objects.
		colliding = new List<GameObject>();
	}

	// Use this for initialization
	void Start()
	{
		// Find and Store the mediator script.
		mediator = GameObject.Find("Mediator").GetComponent<Mediator>();

		// Find and Store the state manager's script.
		stateManager = GameObject.Find("Level").GetComponent<StateManager>();

		// Find and Store the inventory manager's script.
		inventoryManager = GameObject.Find("Inventory").GetComponent<InventoryManager>();

		// Find and Store the camera controller script.
		cameraController = Camera.main.GetComponent<CameraController>();

		// Find and Store the UI Text that displays the command.
		command = GameObject.Find("Command").GetComponent<Text>();

		// Store the animator.
		animator = this.GetComponent<Animator>();

		// Find and Store the blackboard.
		blackboard = GameObject.Find("Blackboard").GetComponent<Blackboard>();

		// See if you need to move the player.
		object spawn = blackboard.Get(stateManager.PlayerName);
		if (spawn != null)
		{
			Vector3 spawnPosition = (Vector3)spawn;
			GetComponent<Rigidbody2D>().MovePosition(spawnPosition);
		}

		// Get the tutorial game manager.
		tutorialGame = GameObject.Find("Level").GetComponent<TutorialGame>();

		// Get the quest action handler script
		questActionHandler = GameObject.Find("Dialogue Manager Custom").GetComponent<QuestActionHandler>();
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		// Encumbrance Mechanic: For every item you carry, your speed decreases by 0.3.
		// Max number of items you can carry before your speed drops to zero: 6.
		playerSpeed = maxPlayerSpeed - (itemSpeedPenalty * inventoryManager.InventoryCount);

		// Detect if the player is overencumbered.
		if (this.playerSpeed < 0.1f)
		{
			playerSpeed = 0.0f;
			isOverencumbered = true;
		}

		// If we have 0.1 or higher speed, we can still move!
		else
		{
			isOverencumbered = false;
		}
			

		// Do not let the player move until the player is visible
		if (!isFullyInstantiated && stateManager.PlayerGameObject.GetComponent<Renderer>().isVisible)
		{
			isFullyInstantiated = true;
			Enable();
		}

		// If it's the player's turn, the player can move, and the camera is not moving...
		if (stateManager.PlayerTurn && CanMove && iTween.Count(Camera.main.gameObject) == 0 && !questActionHandler.playerInConversation)
		{
			// Update player on the basis of movement input.
			MovementUpdate();
		}

		// Otherwise...
		else
		{
			// Remember the player is not moving.
			animator.SetBool("Moving", false);
		}
	}

	// Fires on a trigger collision entry.
	void OnTriggerEnter2D(Collider2D other)
	{
		string colTag = other.gameObject.tag;
		bool isRelevantCollision = colTag.Equals("Item") ||
		                           colTag.Equals("Door") ||
		                           colTag.Equals("Doorway") ||
		                           colTag.Equals("NPC") ||
		                           colTag.Equals("Entrance");

		// If the collision object is tagged as an item, door, entrance, or NPC, and
		// we have not already recorded it...
		if (isRelevantCollision && !colliding.Contains(other.gameObject)) {
			colliding.Add(other.gameObject); // ...add the object to our list.
		}
	}

	// Fires on a trigger collision exit.
	void OnTriggerExit2D(Collider2D other)
	{
		// If we were colliding with a game object...
		if (colliding.Contains(other.gameObject)) {
			colliding.Remove(other.gameObject); // ...remove it from our list.
		}
	}

	#endregion

	// Disables the player.
	public void Disable()
	{
		canAct = false;
		canMove = false;
	}

	// Enables the player.
	public void Enable()
	{
		canAct = true;
		canMove = true;
	}

	// Executes the given command.
	public void ExecuteCommand(string verb, string firstEntity, string secondEntity)
	{
		if (CanAct)
		{
			DialogueManager.StopConversation();

			// Open or Close
			if (verb.Equals("open") || verb.Equals("close"))
			{
				HandleOpenOrClose(verb, firstEntity);
			}

			// Give
			else if (verb.Equals("give"))
			{
				HandleGive(firstEntity, secondEntity);
			}

			// Pick Up
			else if (verb.Equals("pick up"))
			{
				HandlePickup(firstEntity);
			}

			// Look At
			else if (verb.Equals("look at"))
			{
				HandleLookat(firstEntity);
			}

			// Talk To
			else if (verb.Equals("talk to"))
			{
				HandleTalkTo(firstEntity);
			}

			// Drop
			else if (verb.Equals("drop"))
			{
				HandleDrop(firstEntity);
			}

			// Go To
			else if (verb.Equals("go to"))
			{
				HandleGoto(firstEntity);
			}

			// Use
			else if (verb.Equals("use"))
			{
				HandleUse(firstEntity, secondEntity);
			}

			// At this point, both instance variables: action and command.text have been set.
			// We can therefore log the action and its execution status here.
			Tuple<string, string> log;

			if (command.text.Equals(CommandBuilder.ACTION_ERROR))
				log = Tuple.New(action, "failure");
			else if (command.text.Equals(CommandBuilder.PROXIMITY_ERROR))
				log = Tuple.New(action, "proximity error");
			else if (command.text.Equals(CommandBuilder.MEDIATION_ERROR))
				log = Tuple.New(action, "system error");
			else if (command.text.Equals(CommandBuilder.OVERENCUMBERED_PICKUP_ERROR))
				log = Tuple.New(action, "overencumbered");
			else
			{
				log = Tuple.New(action, "success");

				// FIXME: This is a hack, but it works.  While we're in the tutorial part of the game
				// this method will trigger the appropriate conversations.  A better solution would be
				// an event-driven approach so that interested parties could subscribe to the Mediator
				// and be informed when an action happens or when there is a change in state.
				if(DialogueLua.GetVariable("TUTORIAL_FINISHED").AsBool == false)
					tutorialGame.UpdateTutorialState(action);
			}

			// Add the log item to the action log in the mediator.
			mediator.actionLog.Add(log);
		}
	}

	#region Execute Command Delegates

	// Delegate for the "open" and "close" commands.
	private void HandleOpenOrClose(string verb, string firstEntity)
	{
		// Get the parameters for the logical action.
		string target = firstEntity;
		string agent = stateManager.PlayerName;
		string location = stateManager.At(stateManager.PlayerName);

		// Setup the action
		this.action = "(" + verb + " " + agent + " " + target + " " + location + ")";

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);
			
		if (actionIsPossible)
		{
			// FIXME: This is a hack.  I need to be able to stop the player from exiting the bar while the tutorial
			// is active, so if we get an "open" action come in (i.e. the player is trying to open the door out of 
			// the bar) while the player is in the tutorial, deny the action.
			if (verb.Equals("open") &&
			    DialogueLua.GetVariable("WIZARD_TUTORIAL_CLOSE_ACTION_STARTED").AsBool == true &&
			    DialogueLua.GetVariable("TUTORIAL_FINISHED").AsBool == false &&
			    location.Equals("bar"))
			{
				command.text = CommandBuilder.TUTORIAL_ERROR;
				return;
			}

			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// If the player is colliding with the target object,
			GameObject collidedEntrance = CollidingWithTagged(target, "Entrance");
			GameObject collidedDoor = CollidingWithTagged(target, "Door");

			if (collidedEntrance != null || collidedDoor != null) 
			{
				// Attempt to execute the logical action!
				bool actionSucceeded = mediator.PlayerUpdate(action);

				if (actionSucceeded)
				{
					// Clear the command.
					command.text = "";

					// Play the open or close sound!
					if (verb.Equals("open"))
						GetComponent<AudioSource>().PlayOneShot(open);
					else
						GetComponent<AudioSource>().PlayOneShot(close);
				}

				else
				{
					command.text = CommandBuilder.MEDIATION_ERROR;
				}


			}
		}

		else
		{
			// "I don't think I'm able to do that."
			command.text = CommandBuilder.ACTION_ERROR;
		}	
	}

	// Delgate for the "give" command.
	private void HandleGive(string firstEntity, string secondEntity)
	{
		// Get the parameters for the logical action.
		string sender = stateManager.PlayerName;
		string item = firstEntity;
		string receiver = secondEntity;
		string location = stateManager.At(stateManager.PlayerName);

		// Setup the action
		this.action = "(give " + sender + " " + item + " " + receiver + " " + location + ")";

		// This command is a bit tricky to connect to the UI.  The command may not be
		// possible because either the command doesn't make sense, or, because the 
		// character you're trying to give the item to doesn't want the item.
		// Logically, there is no way to distinguish between them. So we have to do
		// some extra checking here.
		string typeOfFirstEntity = stateManager.TypeOf(firstEntity);
		string typeOfSecondEntity = stateManager.TypeOf(secondEntity);

		// If the first entity is not an item or the second entity is not a character,
		// the command simply doesn't make sense. Notify and return.
		if (!typeOfFirstEntity.Equals("item") || !typeOfSecondEntity.Equals("character"))
		{
			command.text = CommandBuilder.ACTION_ERROR;
			return;
		}

		// Otherwise, if the character doesn't want the item, notify the user and exit.
		if (!stateManager.WantsItem(receiver, item))
		{
			// NOTIFY WITH DIALOGUE
			DialogueManager.StartConversation("I Don't Want That");
			command.text = CommandBuilder.ACTION_ERROR;
			return;
		}

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);

		if (actionIsPossible)
		{
			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// If the player is colliding with the NPC that they want to give the item to,
			GameObject collided = CollidingWithTagged(receiver, "NPC");

			if (collided != null)
			{
				// Attempt to execute the logical action!
				bool actionSucceeded = mediator.PlayerUpdate(action);

				if (actionSucceeded)
				{
					// Clear the command.
					command.text = "";

					// update dialogue variables
					SetDialogueVariable(firstEntity, secondEntity);

					// Play give sound!
					GetComponent<AudioSource>().PlayOneShot(give);
				}

				else
				{
					command.text = CommandBuilder.MEDIATION_ERROR;	
				}
			}
		}

		else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	// Delegate for the "look at" command.
	private void HandleLookat(string firstEntity)
	{
		// Get the parameters for the logical action.
		string character = stateManager.PlayerName;
		string location = stateManager.At(stateManager.PlayerName);

		// See what the item is.
		GameObject entity = GameObject.Find(firstEntity);

		// FIXME: I'm not satisfied with this, because it's clunky and prone to errors.
		// But it works for now, so let's not mess with it.

		if (entity != null)
		{
			// TODO: Eventually, this look at command should actually be passed to the mediator
			// through an update. But for some reason, the "look-at" action isn't supported ever
			// in the PDDL file.  I'm not sure why.

			// Record the action string.
			action = "(look-at " + character + " " + firstEntity + " " + location + ")";

			// If it's a regular entity, get the description.
			if (entity.GetComponent<ClickHandler>() != null)
				command.text = entity.GetComponent<ClickHandler>().description;

			// If it's a door, get where it's going.
			else if (entity.GetComponent<Door>() != null)
				command.text = "A locked gate.";

			// If it's a room, you're looking at it through an unlocked door.
			else if (entity.GetComponent<Room>() != null)
				command.text = "An unlocked gate. Beyond it, you see the " + entity.name + ".";
			
		}
	}

	// Delegate for the "pick up" command.
	private void HandleDrop(string firstEntity)
	{
		// Get the parameters for the logical action.
		string item = firstEntity;
		string agent = stateManager.PlayerName;
		string location = stateManager.At(stateManager.PlayerName);

		// Setup the action
		action = "(drop " + agent + " " + item + " " + location + ")";

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);

		if (actionIsPossible)
		{
			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// Get the tiles next to the player.
			Room room = GameObject.Find(location).GetComponent<Room>();
			List<GameObject> adjacentTiles = room.GetAdjacentOpenTiles(stateManager.PlayerGameObject.transform.position);

			// If the player is adjacent to an unoccupied tile,
			if (adjacentTiles.Count > 0)
			{
				// Attempt to execute the logical action!
				bool actionSucceeded = mediator.PlayerUpdate(action);

				if (actionSucceeded)
				{
					// Get one tile at random from the list of open tiles we compiled.
					GameObject tile = adjacentTiles[Random.Range(0, adjacentTiles.Count)];

					// Set the spawn location for the item to be a random adjacent tile.
					blackboard.Put(item, tile.transform.position);

					// Clear the command.
					command.text = "";

					// Play drop sound! (same as the give sound)
					GetComponent<AudioSource>().PlayOneShot(give);
				}
				else
				{
					command.text = CommandBuilder.MEDIATION_ERROR;	
				}
			}

			// If there are no adjacent tiles available, it is an action error.
			else
			{
				command.text = CommandBuilder.ACTION_ERROR;
			}
		}
		else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	// Delegate for the "pickup" command.
	private void HandlePickup(string firstEntity)
	{
		// Get the parameters for the logical action.
		string target = firstEntity;
		string agent = stateManager.PlayerName;
		string location = stateManager.At(stateManager.PlayerName);

		// Setup the action
		action = "(pickup " + agent + " " + target + " " + location + ")";

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);

		if (actionIsPossible)
		{
			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// If the player is colliding with the item that they want to pick up,
			GameObject collided = CollidingWithTagged(target, "Item");

			if (collided != null)
			{
				// If the player is over encumbered (too many items)
				if (isOverencumbered)
				{
					command.text = CommandBuilder.OVERENCUMBERED_PICKUP_ERROR;
				}

				// The player doesn't already have too many items.
				else
				{
					// Attempt to execute the logical action!
					bool actionSucceeded = mediator.PlayerUpdate(action);

					if (actionSucceeded)
					{
						// Remove the colliding object.
						colliding.Remove(collided);

						// Clear the command.
						command.text = "";

						// Play pick up sound!
						GetComponent<AudioSource>().PlayOneShot(pickUp);
					}
				
					else
					{
						command.text = CommandBuilder.MEDIATION_ERROR;	
					}
				}
			}
		}
		else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	// Delegate for the "talk to" command
	private void HandleTalkTo(string firstEntity)
	{
		// Get the parameters for the logical action.
		string hearer = firstEntity;
		string speaker = stateManager.PlayerName;
		string location = stateManager.At(stateManager.PlayerName);

		// Setup the action
		action = "(talk-to " + speaker + " " + hearer + " " + location + ")";

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);

		if (actionIsPossible)
		{
			// Great! Check the physics of the world to see if we can do this.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// If the player is colliding with the NPC that they want to talk to,
			GameObject collided = CollidingWithTagged(firstEntity, "NPC");

			if (collided != null)
			{
				// And that object contains an NPC component,
				GameObject conversantGO = collided;
				NPC conversant = conversantGO.GetComponent<NPC>();

				if (conversant != null)
				{
					// Attempt to execute the logical action!
					bool actionSucceeded = mediator.PlayerUpdate(action);

					if (actionSucceeded)
					{
						// Start the conversation!
						DialogueManager.StartConversation(conversant.conversation);

						// Clear the command.
						command.text = "";
					}

					else
					{
						command.text = CommandBuilder.MEDIATION_ERROR;  
					}
				}

				else
				{
					Debug.Log("DialogueManager: Conversant is not an NPC");
				}

			}
				
			//
			else
			{
				command.text = CommandBuilder.PROXIMITY_ERROR;
			}
		}

		// 
		else
		{
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	// Delegate for the "go to" command.
	private void HandleGoto(string firstEntity)
	{
		// Attempt a split on the entity by whitespace
		char[] whitespace = { ' ' };
		string[] words = firstEntity.Split(whitespace);
		string keyword = words[words.Length - 1];

		// Get the parameters for the logical action.
		string room1 = stateManager.PositionToLocation(stateManager.Player.transform.position); // the room the player is in
		string target = keyword; // where the player wishes to go
		string agent = stateManager.PlayerName;

		// Setup the action string.
		action = "";

		// If the target is an entrance at the room,
		if (stateManager.IsEntranceAt(target, room1))
		{
			// and that entrance leads somewhere, setup the action.
			string room2 = stateManager.EntranceLeadsTo(target);
			if (!room2.Equals(""))
			{
				action = "(move-through-entrance " + agent + " "
				+ room1 + " " + target + " " + room2 + ")";
			}
		}

        // If the target and the room have a door between them, 
        else if (stateManager.DoorBetween(room1, target))
		{
			string door = stateManager.DoorName(room1, target);
			action = "(move-through-door " + agent + " " + room1
			+ " " + door + " " + target + ")";
		}

        // Otherwise, the target should be a room with no door between.
        else
		{
			action = "(move-through-doorway " + agent + " " + room1 + " " + target + ")";	
		}
            
		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);
		bool actionSucceeded = false;

		if (actionIsPossible)
		{
			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// Search the list of colliding objects, 
			foreach (GameObject collided in colliding)
			{
				// If the collided object is a door,
				if (collided.tag.Equals("Door"))
				{
					Door door = collided.GetComponent<Door>();

					// if the door we're colliding with is the one we've input to the command,
					if (door.ConnectsTo.Equals(target))
					{
						// Attempt to execute the logical action!
						actionSucceeded = mediator.PlayerUpdate(action);

						if (actionSucceeded)
						{
							// Remove player from this room.
							stateManager.RemoveObject(stateManager.PlayerName, room1);
							Destroy(stateManager.Player.gameObject);
							
							// Update camera
							cameraController.LerpCamera(door.Direction);

							// Record the new spawn position for the player.
							SetFutureSpawnLocation(stateManager.PlayerGameObject.transform.position, door.Direction);

							// Clear the command.
							command.text = "";

							// Exit the loop.
							break;
						}
						else
						{
							command.text = CommandBuilder.MEDIATION_ERROR;
						}
					}
				}

				// If the collided object is a doorway,
				else if (collided.tag.Equals("Doorway"))
				{
					Doorway doorway = collided.GetComponent<Doorway>();

					// if the doorway we're colliding with is the one we've input to the command,
					if (doorway.ConnectsTo.Equals(target))
					{
						// Attempt to execute the logical action!
						actionSucceeded = mediator.PlayerUpdate(action);

						if (actionSucceeded)
						{
							// Remove player from this room.
							stateManager.RemoveObject(stateManager.PlayerName, room1);
							Destroy(stateManager.Player.gameObject);

							// Update camera
							cameraController.LerpCamera(doorway.Direction);

							// Record the new spawn position for the player.
							SetFutureSpawnLocation(stateManager.PlayerGameObject.transform.position, doorway.Direction);

							// Clear the command.
							command.text = "";

							// Break the loop.
							break;
						}
						else
						{
							command.text = CommandBuilder.MEDIATION_ERROR;
						}
					}
				}

                // If the collided object is an entrance,
                else if (collided.tag.Equals("Entrance"))
				{
					// If the entrance we're colliding is the one we've input,
					if (collided.name.Equals(target))
					{
						// Attempt to execute the logical action!
						actionSucceeded = mediator.PlayerUpdate(action);

						if (actionSucceeded)
						{
							// Remove player from this room.
							stateManager.RemoveObject(stateManager.PlayerName, room1);
							Destroy(stateManager.Player.gameObject);

							// Update camera
							string room2 = stateManager.EntranceLeadsTo(target);
							GameObject room2gameObject = GameObject.Find(room2);
							cameraController.FadeOutCamera(room2gameObject.gameObject.transform.position);

							// Check if there's a symmetric exit where the player is going
							string symmetricExit = stateManager.SymmetricExitName(room1, room2);
							if (!symmetricExit.Equals(""))
							{
								// If there is such a symmetry, then the player will spawn at that exit.
								// It is more consistent with the real world.
								GameObject exit = GameObject.Find(symmetricExit);

								// if the exit is null, it means we haven't yet created the exit
								// (we're entering for the first time).
								if (exit == null)
								{
									// therefore, set the spawn location for the exit first.

									// if the location we're building this entrance at is outside,
									// then we need to get a special tile
									if (stateManager.IsOutdoors(room2))
										exit = stateManager.GetOutdoorLotTile(room2gameObject);
									else
										exit = stateManager.GetOpenTile(room2gameObject, false);
                                    
									blackboard.Put(symmetricExit, exit.transform.position);
								}

								Vector3 exitLocation = exit.gameObject.transform.position;

								// Record the new spawn position for the player.
								SetFutureSpawnLocation(exitLocation);
							}

							// Clear the command.
							command.text = "";

							// Break the loop.
							break;
						}
					}
				}

				if (actionSucceeded) // clear the list of colliding objects
					colliding.Clear();
			}
		}
		else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	// Delegate for the "use" command.
	private void HandleUse(string firstEntity, string secondEntity)
	{
		// Get the parameters for the logical action.
		string agent = stateManager.PlayerName;
		// string location = stateManager.At(stateManager.PlayerName);

		// The first and second entities can apply to a variety of commands.
		// Thus, we need to know what are the types of these entities in order to attempt an action.
		string typeOfFirstEntity = stateManager.TypeOf(firstEntity);
		string typeOfSecondEntity = stateManager.TypeOf(secondEntity);

		// NOTE: Interestingly, I feel that the most checking I should do here is "type" checking.
		// e.g. "is-a key" or "is-a door"; I'm not sure why I feel this - maybe because semantic
		// checking would not allow players to make mistakes?
		// The problem is that several potential "use" commands could involve using items on characters
		// (like the wake-person), so how would you disambiguate without additional semantic information?

		// If the player is trying to use an item on a door,
		if (typeOfFirstEntity.Equals("item") && typeOfSecondEntity.Equals("entrance"))
			HandleUnlockEntrance(agent, firstEntity, secondEntity);

        // If none of the context-sensitive commands work, then you've made a mistake.
        else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	#endregion

	#region "Use" Command Variants

	// Delegate for the "unlock-door" variant of the "use" command.
	private void HandleUnlockEntrance(string agent, string key, string entrance)
	{
		// Build the action.
		string location = stateManager.At(agent);
		action = "(unlock-entrance " + agent + " " + key + " " + entrance + " " + location + ")";

		// Test if the action is possible.
		bool actionIsPossible = mediator.IsApplicable(action);

		if (actionIsPossible)
		{
			// FIXME: This is a hack.  I need to be able to stop the player from using the key in the basement 
			// before the player has looked at the exit, so if we get a legal "use" action come in (i.e. the player
			// is trying to unlock the basement exit), deny the action.
			if (DialogueLua.GetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_STARTED").AsBool == true &&
			    DialogueLua.GetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_COMPLETED").AsBool == false &&
			    location.Equals("basement"))
			{
				command.text = CommandBuilder.TUTORIAL_ERROR;
				return;
			}

			// Great! Check the physics of the world to see if I can actually do that.
			// Assume we can't do it.
			command.text = CommandBuilder.PROXIMITY_ERROR;

			// If the player is colliding with the Entrance that they want to use the key on,
			GameObject collided = CollidingWithTagged(entrance, "Entrance");

			if (collided != null)
			{
				// Attempt to execute the logical action!
				bool actionSucceeded = mediator.PlayerUpdate(action);

				if (actionSucceeded)
				{
					// Clear the command.
					command.text = "";

					// Play unlock sound!
					GetComponent<AudioSource>().PlayOneShot(unlockDoor);
				}

				else
				{
					command.text = CommandBuilder.MEDIATION_ERROR;	
				}
			}
		}

		else
		{
			// I don't think I'm able to do that.
			command.text = CommandBuilder.ACTION_ERROR;
		}
	}

	#endregion

	// Searches in the list of colliding GameObjects for the first one named as given.
	// Returns null if no such GameObject is found.
	private GameObject CollidingWith(string name)
	{
		foreach (GameObject collided in colliding) {
			if (collided.name.Equals(name)) {
				return collided;
			}
		}

		return null;
	}

	// Searches in the list of colliding GameObjects for the first one named and tagged as given.
	// Returns null if no such GameObject is found.
	private GameObject CollidingWithTagged(string name, string tag)
	{
		foreach (GameObject collided in colliding) {

			if (collided.tag.Equals(tag) && collided.name.Equals(name)) {
				return collided;
			}
		}

		return null;
	}
		
	// Handles the player's on-screen movement. Does not update the logical state on the basis of movement.
	private void MovementUpdate()
	{
		// If the player is sending movement input...	
		if (Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")) > 0)
		{
			// If the player's speed is zero, trying to move will not work. This means the player is overencumbered.
			if (isOverencumbered)
			{
				command.text = CommandBuilder.OVERENCUMBERED_WALK_ERROR;
			}

			// Otherwise, move the player.
			else
			{
				// Remember that the player is moving.
				animator.SetBool("Moving", true);

				// If the player is moving horizontally...
				if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(Input.GetAxis("Vertical")))
				{
					// If the horizontal movement is greater than zero...
					if (Input.GetAxis("Horizontal") > 0)
					{
						// Move the player.
						GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + new Vector2(playerSpeed, 0) * Time.deltaTime);

						// Animate the player.
						animator.SetInteger("Direction", 1);

						// If we have hit the right boundary, set the player at the boundary.
						float xMax = cameraController.BottomRightWorldPoint.x - (GetComponent<SpriteRenderer>().bounds.size.x);

						if (GetComponent<Rigidbody2D>().position.x >= xMax)
							GetComponent<Rigidbody2D>().MovePosition(new Vector2(xMax, GetComponent<Rigidbody2D>().position.y));

					}

                	// Otherwise, if it's less than zero...
                	else if (Input.GetAxis("Horizontal") < 0)
					{
						// Move the player.
						GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + new Vector2(playerSpeed * -1, 0) * Time.deltaTime);

						// Animate the player.
						animator.SetInteger("Direction", 3);

						// If we have hit the right boundary, set the player at the boundary.
						float xMin = cameraController.TopLeftWorldPoint.x + (GetComponent<SpriteRenderer>().bounds.size.x);

						if (GetComponent<Rigidbody2D>().position.x <= xMin)
							GetComponent<Rigidbody2D>().MovePosition(new Vector2(xMin, GetComponent<Rigidbody2D>().position.y));
					}
				}

            	// Otherwise, if the player is moving vertically...
            	else
				{
					// If the vertical movement is greater than zero...
					if (Input.GetAxis("Vertical") > 0)
					{
						// Move the player.
						GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + new Vector2(0, playerSpeed) * Time.deltaTime);
							
						// Animate the player.
						animator.SetInteger("Direction", 0);

						// If we have hit the top boundary, set the player at the boundary.
						float yMax = cameraController.TopLeftWorldPoint.y - (GetComponent<SpriteRenderer>().bounds.size.y);
							
						if (GetComponent<Rigidbody2D>().position.y >= yMax)
							GetComponent<Rigidbody2D>().MovePosition(new Vector2(GetComponent<Rigidbody2D>().position.x, yMax));
					
					}
    
					// Otherwise, if the vertical movement is less than zero...
                	else if (Input.GetAxis("Vertical") < 0)
					{
						// Move the player.
						GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + new Vector2(0, playerSpeed * -1) * Time.deltaTime);

						// Animate the player.
						animator.SetInteger("Direction", 2);

						// If we have hit the bottom boundary, set the player at the boundary.
						float yMin = cameraController.BottomRightWorldPoint.y + (GetComponent<SpriteRenderer>().bounds.size.y);

						if (GetComponent<Rigidbody2D>().position.y <= yMin)
							GetComponent<Rigidbody2D>().MovePosition(new Vector2(GetComponent<Rigidbody2D>().position.x, yMin));
					}
				}
			}
		}
        
        // Otherwise, if the player is not moving...
        else
		{
			// Remember the player is not moving.
			animator.SetBool("Moving", false);
		} 
	}

	// Set the future spawn location of the player sprite below the given starting position.
	private void SetFutureSpawnLocation(Vector3 startingPosition)
	{
		float yOffset = GetComponent<SpriteRenderer>().bounds.size.y;

		float newX = startingPosition.x;
		float newY = startingPosition.y - yOffset; // always place below the given position.
		float newZ = startingPosition.z;

		Vector3 futureSpawnLocation = new Vector3(newX, newY, newZ);
		blackboard.Put(stateManager.PlayerName, futureSpawnLocation);
	}

	// Set the future spawn location of the player sprite, relative to
	// the player's current location and a direction of travel.
	private void SetFutureSpawnLocation(Vector3 currentLocation, Direction direction)
	{
		// Get the current position of the player.
		Vector3 futureSpawnPosition;
		float newX = currentLocation.x;
		float newY = currentLocation.y;
		float newZ = currentLocation.z;

		float xOffset = GetComponent<SpriteRenderer>().bounds.size.x * 3.0f;
		float yOffset = GetComponent<SpriteRenderer>().bounds.size.y * 3.0f;

		// Offset it in the direction of movement.
		switch (direction)
		{
		case Direction.North:
			newY += yOffset;
			break;

		case Direction.South:
			newY -= yOffset;
			break;

		case Direction.East:
			newX += xOffset;
			break;

		case Direction.West:
			newX -= xOffset;
			break;
		}

		futureSpawnPosition = new Vector3(newX, newY, newZ);
		blackboard.Put(stateManager.PlayerName, futureSpawnPosition);
	}

	/**
	 * Sets DialogueManager variable based on item given to an NPC
	 */
	private void SetDialogueVariable(string item, string npc)
	{
		//*************************
		// Equip Quest
		//*************************
		if (item == "knightsword" && npc == "ian")
		{
			DialogueLua.SetVariable("QUARTER_MASTER_HAS_SWORD", true);
			DialogueManager.StartConversation("Equip Quest");
		}
		else if (item == "knightshield" && npc == "ian")
		{
			DialogueLua.SetVariable("QUARTER_MASTER_HAS_SHIELD", true);
			DialogueManager.StartConversation("Equip Quest");
		}
		else if (npc == "ian")
		{
			DialogueLua.SetVariable("QUARTER_MASTER_GIVEN_NONQUEST_ITEM", true);
		}

		//*************************
		// Pilgrimage Quest
		//*************************
		else if (item == "tastycupcake" && npc == "alli")
		{
			DialogueLua.SetVariable("TROLL_GIVEN_FOOD", true);
			DialogueManager.StartConversation("Pilgrimage Quest"); 
		}
		else if (npc == "alli")
		{
			DialogueLua.SetVariable("TROLL_GIVEN_NONQUEST_ITEM", true);
		}

		//*************************
		// Wisdom Quest
		//*************************
		else if (item == "coin" && npc == "james")
		{
			DialogueLua.SetVariable("RIDDLER_GIVEN_COINS", true);
			DialogueManager.StartConversation("Wisdom Quest");
		}
		else if (item == "humanskull" && npc == "james")
		{
			DialogueLua.SetVariable("RIDDLER_GIVEN_SKULL", true);
			DialogueManager.StartConversation("Wisdom Quest");
		}
		else if (item == "candle" && npc == "james")
		{
			DialogueLua.SetVariable("RIDDLER_GIVEN_CANDLE", true);
			DialogueManager.StartConversation("Wisdom Quest");
		}
		else if (npc == "james")
		{
			DialogueLua.SetVariable("RIDDLER_GIVEN_NONQUEST_ITEM", true);
		}

		//*************************
		// Love Quest
		//*************************
		else if (item == "loveletter" && npc == "jordan")
		{
			DialogueLua.SetVariable("LOVER_B_GIVEN_LETTER", true);
			DialogueManager.StartConversation("Love Quest B");
		}
		else if (item == "rubyring" && npc == "dorian")
		{
			DialogueLua.SetVariable("LOVER_A_GIVEN_RING", true);
			DialogueManager.StartConversation("Love Quest A");
		}
		else if (item == "bouquet" && npc == "dorian")
		{
			DialogueLua.SetVariable("LOVER_A_GIVEN_FLOWERS", true);
			DialogueManager.StartConversation("Love Quest A");
		}
//		else if (npc == "dorian")
//		{
//			DialogueLua.SetVariable("LOVER_A_GIVEN_NONQUEST_ITEM", true);
//		}
		else if (item == "lovecontract" && npc == "jordan")
		{
			DialogueLua.SetVariable("LOVER_B_GIVEN_CONTRACT", true);
			DialogueManager.StartConversation("Love Quest B");
		}

		//*************************
		// Fetch Quest
		//*************************
		else if (npc == "giovanna")
		{
			// sets variable that is shown inside dialogue
			DialogueLua.SetVariable("fetchQuestItem", item);

			// determine if quest or non-quest item is given
			if (item == "hairtonic")
			{
				DialogueLua.SetVariable("APPRAISER_GIVEN_QUEST_ITEM", true);
				DialogueManager.StartConversation("Fetch Quest");
			}
			else
			{
				DialogueLua.SetVariable("APPRAISER_GIVEN_NONQUEST_ITEM", true);
			}
		}
		else if (item == "ash" && npc == "frank")
		{
			DialogueLua.SetVariable("BANKER_GIVEN_ASH", true);
			DialogueManager.StartConversation("Banker");
		}

	}
}
