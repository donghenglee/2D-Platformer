using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventHolder : MonoBehaviour
{
    public delegate void DeathDelegate();
    public event DeathDelegate OnPlayerDeath;

    public event Action JumpEvent;

    public int i = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Die();
            print(OnPlayerDeath?.Method.Name);
            Jump();
            print(i.ToString());
        }
    }

    void Die()
    {
        /*
         * if(OnPlayerDeath!=null){
         * OnPlayerDeath.Invoke();
         * }
         */
        OnPlayerDeath?.Invoke();
    }

    void Jump()
    {
        JumpEvent?.Invoke();
    }

}
