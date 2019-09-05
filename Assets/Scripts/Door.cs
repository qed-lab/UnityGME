using UnityEngine;
using System.Collections;

// Represents a door within the world.
public class Door : MonoBehaviour
{
    // The interface to the command builder.
    private CommandBuilder commandBuilder;

    // Where this door connects to.
    private string connectsTo;

    // The previous color of this door.
    private Color prevColor;

    // The direction of this door.
    private Direction direction;

    // Whether this door is locked.
    private bool locked;

    // A reference to the quest action handler script to determine if the player is in conversation.
    private QuestActionHandler questActionHandler;

    // Whether the mouse has hovered over this clickable thing.
    private bool mouseHasEntered;
	
    // Use this for initialization
    void Awake( )
    {
        // Default to being unlocked.
        locked = false;

        // Get the interface to the command builder.
        commandBuilder = GameObject.Find("Command").GetComponent<CommandBuilder>();

        // Find the quest action handler script.
        questActionHandler = GameObject.Find("Dialogue Manager Custom").GetComponent<QuestActionHandler>();
    }
	
    // Update is called once per frame
    void Update( )
    {
        Animator animator = gameObject.GetComponent<Animator>();

        // If the object has an animator, and it has changed its locked status,
        if (animator != null && animator.GetBool("locked") != locked)
        {
            // Update the locked variable.
            locked = animator.GetBool("locked");

            // Update the description if necessary. (TODO; e.g. "It is locked.")
        }
    }

    // Called when door is clicked.
    void OnMouseDown( )
    {
        // If locked, the entity to set is the door itself
        if (locked)
            commandBuilder.SetEntity(gameObject.name);

		// Otherwise, the entity is the room behind the door
		else
            commandBuilder.SetEntity(connectsTo);

    }

    // Called when cursor is hovering
    void OnMouseEnter( )
    {
        if (!questActionHandler.playerInConversation)
        {
            mouseHasEntered = true;
            prevColor = this.GetComponent<SpriteRenderer>().color;
            this.GetComponent<SpriteRenderer>().color = Color.cyan;
        }

    }

    // Called when cursor ceases to hover
    void OnMouseExit( )
    {
        if (mouseHasEntered)
        {
            this.GetComponent<SpriteRenderer>().color = prevColor;
            mouseHasEntered = false;
        }
    }

    // Sets up this door's direction and room it connects to.
    public void Setup(Direction direction, string connectsTo)
    {
        // Save the fields.
        this.direction = direction;
        this.connectsTo = connectsTo;

        // TODO: Setup the tile's description.
		
    }

    // The name of the room this door connects to.
    public string ConnectsTo {
        get { return connectsTo; }
        set { connectsTo = value; }
    }

    // The direction of this door.
    public Direction Direction {
        get { return direction; }
        set { direction = value; }
    }

    public void ResetColor( )
    {
        GetComponent<SpriteRenderer>().color = prevColor;
    }
}
