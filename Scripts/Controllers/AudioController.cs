using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance = null;
    AudioSource audioSource;
    public AudioClip[] audioClips;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.PlayOneShot(audioClips[3]);
    }


    public void playSound(string type)
    {
        /*AudioClip clip = audioClips[0];
        switch (type)
        {
            case "Point":
                clip = audioClips[0];
                break;
            case "Die":
                clip = audioClips[1];
                break;
            case "Hit":
                clip = audioClips[2];
                break;
            case "BGM":
                clip = audioClips[3];
                break;
            default:
                clip = audioClips[3];
                break;


        }
        audioSource.PlayOneShot(audioClips[3]);*/

    }
}
