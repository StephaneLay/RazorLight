using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChargerComponent : EnemyBehaviour
{
    
    
    private void Update()
    {
        if (doDestroy)
            Destroy(this.gameObject);
        else if(isFollow)
            nav.SetDestination(player.position);
    }

}
