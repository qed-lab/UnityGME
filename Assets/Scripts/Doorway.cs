using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents a doorway within the world. Doorways are special kinds of tiles.
[RequireComponent(typeof(ClickHandler))]
[RequireComponent(typeof(SpriteRenderer))]
public class Doorway : MonoBehaviour
{
    // The name of the room this doorway connects to.
    private string connectsTo;

    // The direction of this doorway.
    private Direction direction;

    // Called upon loading.
    void Awake( )
    {
        // Update the color of this tile.
        Color teal = new Color(1.0f, 1.0f, 0.86f);
        gameObject.GetComponent<SpriteRenderer>().color = teal;
        gameObject.GetComponent<ClickHandler>().SetColor(teal);
    }

    // Sets up this doorway's direction and room it connects to.
    public void Setup(Direction direction, string connectsTo)
    {
        // Save the fields.
        this.direction = direction;
        this.connectsTo = connectsTo;

        // Update the tag.
        gameObject.tag = "Doorway";

        // When clicked, this tile's gameObject name is sent to the command builder.
        // The thing we have to send is the ultimate room we're connecting to, so override the original entity name.
        gameObject.name = "doorway toward " + connectsTo;

        // Setup this tile's description, too.
        GetComponent<ClickHandler>().description = "The " +
        direction.ToString().ToLower() + "ern doorway, which leads to the " + connectsTo + ".";
    }

    // The name of the room this doorway connects to.
    public string ConnectsTo {
        get { return connectsTo; }
    }

    // The direction of this doorway.
    public Direction Direction {
        get { return direction; }
    }
	
}
