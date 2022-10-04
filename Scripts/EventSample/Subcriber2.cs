using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subcriber2 : MonoBehaviour
{
    EventHolder eventHolder;

    private void Start()
    {
        eventHolder = FindObjectOfType<EventHolder>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            eventHolder.OnPlayerDeath += PrintSomethingDead2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            eventHolder.OnPlayerDeath -= PrintSomethingDead2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            eventHolder.JumpEvent += AddJumpCount;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            eventHolder.JumpEvent -= AddJumpCount;
        }
    }

    void PrintSomethingDead2()
    {
        print("Subscriber 2 death event fire");
    }

    void AddJumpCount()
    {
        eventHolder.i++;
        print("Subscriber 2 AddJumpCount event fire\n");
        
    }
}
