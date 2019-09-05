using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents a building that belongs to an entrance.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Building : MonoBehaviour
{
    public Sprite[] buildingSprites;

    // Sets and enables the building sprite around the entrance this building is attached to.
    public void SetBuildingSprite(string buildingName)
    {
        Sprite spriteToSet;

        // Find the corresponding texture to set in the building sprites array
        if (buildingName.Equals("bar"))
            spriteToSet = buildingSprites[0];
        else if (buildingName.Equals("forge"))
            spriteToSet = buildingSprites[1];
        else if (buildingName.Equals("fort"))
            spriteToSet = buildingSprites[2];
        else if (buildingName.Equals("hut"))
            spriteToSet = buildingSprites[3];
        else if (buildingName.Equals("bank"))
            spriteToSet = buildingSprites[4];
        else if (buildingName.Equals("mansion"))
            spriteToSet = buildingSprites[5];
        else if (buildingName.Equals("shop"))
            spriteToSet = buildingSprites[6];
        else
            spriteToSet = buildingSprites[3]; // as a default, set a house.

        // Once the texture has been found, set it and enable it.
        GetComponent<SpriteRenderer>().sprite = spriteToSet;
        GetComponent<SpriteRenderer>().enabled = true;

        // Also, enable the box collider around the image.
        GetComponent<Collider2D>().enabled = true;
    }
}
