using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TriggerManager : MonoBehaviour
{
    [SerializeField] private GameObject interactIcon;
    
    

    private bool playerIsInRange=false;
    

    public TextAsset inkJSON;
    private static TriggerManager instance;


    private void Awake()
    {
        instance = this;
        interactIcon.SetActive(false);
    }

    public static TriggerManager GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        if (playerIsInRange)
        {
            interactIcon.SetActive(true);
            if (!DialogueManager.GetInstance().dialogueIsPlaying)
            {
                
                if (Input.GetKeyDown(KeyCode.I))
                {
                    DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
                }
            }
            else
            {
                interactIcon.SetActive(false);
            }
            
            
        }
        else
        {
            interactIcon.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerIsInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerIsInRange = false;
        }
    }

    
}
