using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Player player;

    private const float Step = 0.05f;

    private const float positionUpdateRate = 0.1f;
    
   
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            this.player.Z += Step;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            this.player.Z -= Step;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            this.player.X -= Step;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            this.player.X += Step;
        }       
    } 
}