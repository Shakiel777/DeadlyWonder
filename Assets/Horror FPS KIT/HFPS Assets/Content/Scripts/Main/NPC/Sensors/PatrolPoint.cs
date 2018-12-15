using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPoint : MonoBehaviour {

    public float radius;
    public LayerMask PlayerMask;
    public LayerMask ZombieMask;

    [HideInInspector] public bool InTrigger;
    [HideInInspector] public bool zombieInTrigger;


    void Update()
    {
        if (Physics.CheckSphere(transform.position, radius, PlayerMask))
        {
            InTrigger = true;
        }
        else
        {
            InTrigger = false;
        }

        if (Physics.CheckSphere(transform.position, radius, ZombieMask))
        {
            zombieInTrigger = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
