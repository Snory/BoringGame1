using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RobotPowerSource : MonoBehaviour
{
    public UnityEvent<RobotPowerSource> PowerSourceHitted;
    public UnityEvent<RobotPowerSource> PowerSourceAdded;

    public void Start()
    {
        PowerSourceAdded?.Invoke(this);
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == MyTags.PLAYER_BOAT)
        {
            PowerSourceHitted?.Invoke(this);
        }
    }
}
