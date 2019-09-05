using UnityEngine;
using System.Collections;

// Handles camera movements.
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // Audio to play when the player executes a "go to" command.
    public AudioClip go_to;

    // The top left world coordinate of the camera.
    private Vector3 topLeft;

    // The bottom right world coordinate of the camera.
    private Vector3 bottomRight;

    // A room's height in world coordinates.
    private float roomHeight;

    // A room's width in world coordinates.
    private float roomWidth;

    #region Getters/Setters

    public Vector3 TopLeftWorldPoint {
        get { return topLeft; }
    }

    public Vector3 BottomRightWorldPoint {
        get { return bottomRight; }
    }

    #endregion

    // Use this for initialization
    public void Start( )
    {	    
        UpdateBoundingCoordinates();
        roomWidth = bottomRight.x - topLeft.x;
        roomHeight = topLeft.y - bottomRight.y;
        iTween.CameraFadeAdd();
    }

    // Fades the camera to the given position.
    public void FadeOutCamera(Vector3 position)
    {
        // Play go to sound.
        GetComponent<AudioSource>().PlayOneShot(go_to);        

        // Fade out.
        iTween.CameraFadeTo(iTween.Hash(
            "amount", 1.0f, 
            "time", 0.75f, 
            "easetype", iTween.EaseType.easeOutSine, 
            "oncomplete", "SnapCameraAndFadeIn",
            "oncompletetarget", this.gameObject,
            "oncompleteparams", position
        ));
    }

    // Moves the camera to the given position, and fades the camera in.
    private void SnapCameraAndFadeIn(Vector3 position)
    {
        // Set the position.
        gameObject.transform.position = position;
        UpdateBoundingCoordinates();

        // Fade in.
        iTween.CameraFadeTo(iTween.Hash(
            "amount", 0.0f, 
            "time", 0.75f, 
            "easetype", iTween.EaseType.easeOutSine, 
            "oncomplete", "UpdateBoundingCoordinates",
            "oncompletetarget", this.gameObject
        ));
    }

    // Lerps the camera in the given direction.
    public void LerpCamera(Direction direction)
    {
        // Calculate the target position as an offset from the current one.
        Vector3 targetPosition;
        float newX = transform.position.x;
        float newY = transform.position.y;
        float newZ = transform.position.z;

        switch (direction)
        {
            case Direction.North:
                newY += roomHeight;
                break;

            case Direction.South:
                newY -= roomHeight;
                break;

            case Direction.East:
                newX += roomWidth;
                break;

            case Direction.West:
                newX -= roomWidth;
                break;
        }

        // Create the target position.
        targetPosition = new Vector3(newX, newY, newZ);

        // Play go to sound.
        GetComponent<AudioSource>().PlayOneShot(go_to);

        // Tween!
        iTween.MoveTo(this.gameObject, iTween.Hash(
            "position", targetPosition, 
            "time", 1.5f, 
            "easetype", "linear", 
            "onComplete", "UpdateBoundingCoordinates"));
    }

    // Updates the coordinates that demarcate the screen (top left and bottom right).
    private void UpdateBoundingCoordinates( )
    {
        topLeft = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0, GetComponent<Camera>().pixelHeight, GetComponent<Camera>().farClipPlane));
        bottomRight = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(GetComponent<Camera>().pixelWidth, 0, GetComponent<Camera>().farClipPlane));
    }
}
