using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An actant represents an interactable object in UGME. Interactable objects require a collider and a sprite renderer.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ClickHandler : MonoBehaviour
{
    // A description for the thing being clicked.
    public string description;

    // The interface to the command builder.
    private CommandBuilder commandBuilder;

    // The previous color of this thing.
    private Color previousColor;

    // A reference to the quest action handler script to determine if the player is in conversation.
    private QuestActionHandler questActionHandler;

    // Whether the mouse has hovered over this clickable thing.
    private bool mouseHasEntered;



    // Use this for initialization
    void Awake( )
    {
        // Find the command builder to which we are going to delegate the click information.
        commandBuilder = GameObject.Find("Command").GetComponent<CommandBuilder>();

        // Find the quest action handler script.
        questActionHandler = GameObject.Find("Dialogue Manager Custom").GetComponent<QuestActionHandler>();

        // By default this clickable thing has not had the mouse enter it.
        mouseHasEntered = false;

        // Store the original color of this thing.
        previousColor = GetComponent<SpriteRenderer>().color;

    }

    // Runs every frame
    void Update( )
    {
        if (questActionHandler.playerInConversation)
            ResetColor();
    }

    // Called when thing is clicked.
    void OnMouseDown( )
    {
        if (!questActionHandler.playerInConversation)
        {
            commandBuilder.SetEntity(gameObject.name);
        }
            
    }

    // Called when cursor is hovering.
    void OnMouseEnter( )
    {
        if (!questActionHandler.playerInConversation)
        {
            mouseHasEntered = true;
            previousColor = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = Color.cyan;
        }
    }

    // Called when cursor ceases to hover.
    void OnMouseExit( )
    {
        if (mouseHasEntered)
        {
            mouseHasEntered = false;
            GetComponent<SpriteRenderer>().color = previousColor;
        }

    }

    public void SetColor(Color color)
    {
        previousColor = color;
    }

    public void ResetColor( )
    {
        GetComponent<SpriteRenderer>().color = previousColor;
    }
}
