using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RobotPower : MonoBehaviour
{
    public UnityEvent PowerRunOut;
    private Dictionary<RobotPowerSource, bool> _powerSources;
    

    public void OnPowerSourceAdded(RobotPowerSource source)
    {
        if(_powerSources == null)
        {
            _powerSources = new Dictionary<RobotPowerSource, bool>();
        }

        _powerSources.Add(source, true);
    }

    public void OnPowerSourceHitted(RobotPowerSource source)
    {
        if(_powerSources.ContainsKey(source))
        {
            _powerSources[source] = false;
            CheckPower();
        }
    }

    private void CheckPower()
    {
        foreach(var powerSourceActive in _powerSources.Values)
        {
            if (!powerSourceActive)
            {
                PowerRunOut?.Invoke();
                break;
            }
        }
    }

}
