using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subscriber1 : MonoBehaviour
{
    EventHolder eventHolder;

    private void Start()
    {
        eventHolder = FindObjectOfType<EventHolder>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            eventHolder.OnPlayerDeath += PrintSomethingDead1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            eventHolder.OnPlayerDeath -= PrintSomethingDead1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            eventHolder.JumpEvent += MinusJumpCount;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            eventHolder.JumpEvent -= MinusJumpCount;
        }

    }

    void PrintSomethingDead1()
    {
        print("Subscriber 1 death event fire");
    }

    void MinusJumpCount()
    {
        eventHolder.i--;
        print("Subscriber 1 MinusJumpCount event fire\n");
        
    }


}
