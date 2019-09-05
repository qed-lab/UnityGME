using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandBuilder : MonoBehaviour
{
    // String to display when the user has selected a command they cannot do.
    public static readonly string ACTION_ERROR = "I don't think I'm able to do that.";

    // String to display when the user can do an action but is too far from the item.
    public static readonly string PROXIMITY_ERROR = "I think I need to be closer to do that.";

    // String to display when there has been an error in the mediation system.
    public static readonly string MEDIATION_ERROR = "INTERNAL ERROR";

    // String to display when the user can do an action but is carrying too much to move.
    public static readonly string OVERENCUMBERED_WALK_ERROR = "You are carrying too much to be able to walk.";

    // String to display when the user can do an action but is carrying too much to pick something up.
    public static readonly string OVERENCUMBERED_PICKUP_ERROR = "You are carrying too much to be able to pick this up.";

    // String to display when the user tries to open the door that exits the bar during the tutorial.
    public static readonly string TUTORIAL_ERROR = "I shouldn't ignore Mel.";

    // The command that is being built through the UI.
    public Text command;

    // The audio that will play when a command parameter is set.
    public AudioClip click;

    // The state manager.
    private StateManager stateManager;

    // The verb of the command.
    private string verb;

    // The object/character of the command.
    private string firstEntity;

    // The second object/character of the command.  Only applies when the verb is "use"
    private string secondEntity;

    // Whether the command is complext or not (it's complex if the verb is "use").
    private bool isComplexCommand;

    // Other colors.
    private Color orange = new Color(1.0f, 0.5f, 0.0f);
    private Color lightBlue = new Color(0.0f, 0.58f, 1.0f);

    // Use this for initialization.
    void Start( )
    {
        verb = "";
        firstEntity = "";
        secondEntity = "";
        isComplexCommand = false;
        command.text = "";
        stateManager = GameObject.Find("Level").GetComponent<StateManager>();
    }

    // Update is called once per frame.
    void Update( )
    {
        if (Input.GetKeyDown(KeyCode.Escape) && stateManager.Player.CanAct)
        {
            CommandBackspace();
        }

        if (Input.GetKeyDown(KeyCode.Space) && stateManager.Player.CanAct)
        {
            // Check that we have a well-formed command.
            if (!IsIncompleteCommand(verb, firstEntity, secondEntity))
            {
                stateManager.Player.ExecuteCommand(verb, firstEntity, secondEntity);
                command.color = Color.white;
                ClearCommand();
            }

			// If you do not have a complete command, let the player know.
			else if (!verb.Equals(""))
            {
                command.text = "Incomplete command.";
                ClearCommand();
            }

			// If you don't have anything, just act as a clear.
			else
            {
                command.text = "";
                ClearCommand();
            }
                
        }
    }

    // Sets the verb of the command.
    public void SetVerb(string verb)
    {
        ClearCommand();
		
        if (verb.Equals("use") || verb.Equals("give"))
        {
            isComplexCommand = true;
        }
        else
        {
            isComplexCommand = false; 
        }

        this.verb = verb;
        command.text = verb;
        GetComponent<AudioSource>().PlayOneShot(click);

        if (IsIncompleteCommand(verb, firstEntity, secondEntity))
            command.color = orange;
        else
            command.color = lightBlue;
    }

    // Sets the object/character of the command.
    public void SetEntity(string entity)
    {
        // If no verb has been set, default to "look at"
        if (verb.Equals(""))
        {
            verb = "look at";
        }

	
        // If it is a complex command and the first entity is set,
        if (isComplexCommand && !firstEntity.Equals(""))
        {
            secondEntity = entity;

            if (verb.Equals("use"))
                command.text = verb + " " + firstEntity + " with " + secondEntity;
            else
                command.text = verb + " " + firstEntity + " to " + secondEntity;
				
        }

		// Otherwise, set / override the first entity.
		else
        {
            firstEntity = entity;
            command.text = verb + " " + firstEntity;
        }

        GetComponent<AudioSource>().PlayOneShot(click); 


        // FIXME: For color blind folks, this isn't too good.
        if (IsIncompleteCommand(verb, firstEntity, secondEntity))
            command.color = orange;
        else
            command.color = lightBlue;
    }

    // Clears the command being built and resets the entity colors that were clicked.
    public void ClearCommand( )
    {
        if (!firstEntity.Equals("") && GameObject.Find(firstEntity).GetComponent<ClickHandler>() != null)
            GameObject.Find(firstEntity).GetComponent<ClickHandler>().ResetColor();

        if (!secondEntity.Equals("") && GameObject.Find(secondEntity).GetComponent<ClickHandler>() != null)
            GameObject.Find(secondEntity).GetComponent<ClickHandler>().ResetColor();
        
        verb = "";
        firstEntity = "";
        secondEntity = "";
        isComplexCommand = false;
    }

    // Checks whether the given strings are an incomplete command or not.
    private bool IsIncompleteCommand(string verb, string firstEntity, string secondEntity)
    {
        if (string.IsNullOrEmpty(verb))
            return true;
        else if (string.IsNullOrEmpty(firstEntity))
            return true;
        else if (isComplexCommand && string.IsNullOrEmpty(secondEntity))
            return true;

        return false;
    }

    // Acts as a backspace for the command being built.
    private void CommandBackspace( )
    {
        // if the second entity is set, backspace removes it
        if (!secondEntity.Equals(""))
            secondEntity = "";

		// if the first entity is set, backspace removes it
		else if (!firstEntity.Equals(""))
            firstEntity = "";

		// if the verb is set, backspace removes it and resests the isComplexCommand variable.
		else if (!verb.Equals(""))
        {
            verb = "";
            isComplexCommand = false;
        }

        command.text = verb + " " + firstEntity;
    }


}
