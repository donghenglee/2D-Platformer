using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenWallController : MonoBehaviour
{

    public GameObject inviWall;
    private bool isInvisible=false;


    // Start is called before the first frame update
    void Start()
    {

        //inviWall = GetComponent<GameObject>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.GetComponent<Player>() != null && !isInvisible )
        {
            inviWall.SetActive(false);
            isInvisible = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && isInvisible)
        {
            inviWall.SetActive(true);
            isInvisible = false;

        }
        
    }

    
}
