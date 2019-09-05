using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialScreen : MonoBehaviour
{
    //Outside your functions where you want to set all your declarations...
    public Sprite[] gallery;
    //store all your images in here at design time
    public Image displayImage;
    //The current image thats visible
    public Button nextImg;
    //Button to view next image
    public Button prevImg;
    //Button to view previous image
    public int i = 0;
    //Will control where in the array you are

    // A reference to the audio souce.
    private AudioSource cameraAudio;

    // Sound to play when clicked.
    public AudioClip click;

    public void BtnNext( )
    {
        // Play the click sound.
        cameraAudio.PlayOneShot(click);

        if (i + 1 < gallery.Length)
        {
            i++;
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    public void BtnPrev( )
    {
        // Play the click sound.
        cameraAudio.PlayOneShot(click);

        if (i - 1 >= 0)
        {
            i--;
        }
    }

    void Awake( )
    {
        cameraAudio = gameObject.GetComponent<AudioSource>();
    }

    void Update( )
    {
        displayImage.sprite = gallery[i];
        prevImg.interactable = (i == 0) ? false : true;

    }
}
