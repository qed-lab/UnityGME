using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents an entrance within the world.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ClickHandler))]
public class Entrance : MonoBehaviour
{
    // Reference to the state manager.
    private StateManager stateManager;

    // The original description of this entrance.
    private string originalDescription;

    // Where this entrance leads to.
    private string leadsTo;

    // Whether this entrance is locked.
    private bool locked;

    // Whether this entrance is closed.
    private bool closed;

    // Called upon loading.
    void Awake( )
    {
        // Store a reference to the state manager.
        stateManager = GameObject.Find("Level").GetComponent<StateManager>();

        // Store the original description.
        originalDescription = gameObject.GetComponent<ClickHandler>().description;

        // Store the status of this entrance.
        locked = gameObject.GetComponent<Animator>().GetBool("locked");
        closed = gameObject.GetComponent<Animator>().GetBool("closed");
    }

    // Called once per frame.
    void Update( )
    {
        // If it has changed its locked status,
        if (gameObject.GetComponent<Animator>().GetBool("locked") != locked)
        {
            // Update the locked variable.
            locked = GetComponent<Animator>().GetBool("locked");
        }

		// If it has changed its closed status,
		else if (gameObject.GetComponent<Animator>().GetBool("closed") != closed)
        {
            // Update the closed variable.
            closed = GetComponent<Animator>().GetBool("closed");
        }

        // Update the description if necessary.
        if (!locked && !closed)
            gameObject.GetComponent<ClickHandler>().description = originalDescription
            + " that leads to the " + stateManager.EntranceLeadsTo(gameObject.name) + ". It is open.";

        if (!locked && closed)
            gameObject.GetComponent<ClickHandler>().description = originalDescription
            + " that leads to the " + stateManager.EntranceLeadsTo(gameObject.name) + ". It is closed and unlocked.";

        if (locked)
            gameObject.GetComponent<ClickHandler>().description = originalDescription
            + " that leads to the " + stateManager.EntranceLeadsTo(gameObject.name) + ". It is closed and locked.";
    }
		
}
