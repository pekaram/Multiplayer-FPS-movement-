using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionUpdater : MonoBehaviour
{
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, new Vector3(player.X, player.Y, player.Z), 0.05f);
        //this.transform.Translate(position);

    }

}
