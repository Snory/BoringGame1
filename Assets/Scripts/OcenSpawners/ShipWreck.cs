using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWreck : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Triggered");

        if(other.tag == "Player")
        {
            Destroy(this.gameObject);
        }
    }
}
