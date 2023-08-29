using System;
using UnityEngine;

[Serializable]
public struct MovementPattern
{
    public Vector3 Direction;
    public float Speed;
    public float NextPositionWaitTime;
    public MovementPattern(Vector3 direction, float speed, float nextPositionWaitTime)
    {
        Direction = direction;
        Speed = speed;
        NextPositionWaitTime = nextPositionWaitTime;
    }

    public MovementPattern(Vector3 direction, MovementPattern oldPattern)
    {
        Direction = direction;
        Speed = oldPattern.Speed;
        NextPositionWaitTime = oldPattern.NextPositionWaitTime;
    }
}
