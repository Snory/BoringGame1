using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct RobotMovementPattern
{
    public Vector3 Direction;
    public float Speed;
    public float NextPositionWaitTime;
    public RobotMovementPattern(Vector3 direction, float speed, float nextPositionWaitTime)
    {
        Direction = direction;
        Speed = speed;
        NextPositionWaitTime = nextPositionWaitTime;
    }

    public RobotMovementPattern(Vector3 direction, RobotMovementPattern oldPattern)
    {
        Direction = direction;
        Speed = oldPattern.Speed;
        NextPositionWaitTime = oldPattern.NextPositionWaitTime;
    }
}

public class RobotMovement : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _navmeshAgent;

    [SerializeField]
    private List<RobotMovementPattern> _robotMovementData;

    [SerializeField]
    private Transform _transform;
    private CancellationTokenSource _cancellationTokenSource;

    [SerializeField]
    private float _nextAngleIncrement;


    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        try
        {
            StartRobotPatternMovement(cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }


    public async void StartRobotPatternMovement(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Debug.Log("StartRobotPatternMovement - start");

                List<RobotMovementPattern> robotMovementPattern = _robotMovementData;

                float angle = 0;
                bool reachable = false;

                for (int i = 0; i < 360 / _nextAngleIncrement; i++)
                {
                    Debug.Log("Searching for movable pattern");

                    reachable = RobotMovementPatternReachable(robotMovementPattern);

                    if (reachable)
                    {
                        Debug.Log("Pattern found");
                        break;
                    }

                    if (angle % 360 == 0)  // jestli je to uhel dìlitelný 360, tak jsme na stejné pozici a nemusim to øešit
                    {
                        continue;
                    }

                    angle += _nextAngleIncrement;
                    robotMovementPattern = AdjustRobotMovementPattern(robotMovementPattern, angle);
                }

                //start async movement with given robotMovementPattern
                if (reachable)
                {
                    await Move(robotMovementPattern, cancellationToken);
                }
            }
            catch
            {
                throw;
            }
        }
    }

    public async Task Move(List<RobotMovementPattern> robotMovementPatterns, CancellationToken cancellationToken)
    {
        foreach (var pattern in robotMovementPatterns)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Debug.Log("Move in direction: " + pattern.Direction);


                await MoveRoutine(pattern, cancellationToken);

            }
        }
    }

    public async Task MoveRoutine(RobotMovementPattern pattern, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {

            Vector3 newPosition = _transform.position + pattern.Direction;
            _navmeshAgent.speed = pattern.Speed;
            _navmeshAgent.destination = newPosition;

            await WaitTillReachDestination(cancellationToken, newPosition); //dojdi na pozici

            Debug.Log("Waiting on position for: " + pattern.NextPositionWaitTime);
            await Task.Delay(System.TimeSpan.FromSeconds(pattern.NextPositionWaitTime), cancellationToken); //poèkej tam jak dlouho uznáš za vhodné

        }
    }

    public async Task WaitTillReachDestination(CancellationToken cancellationToken, Vector3 position)
    {
        Debug.Log("Waiting till agent will reach destination");
        while (_navmeshAgent.velocity.magnitude > 0 && !cancellationToken.IsCancellationRequested)
        {
            Debug.Log("Distance: " + Vector3.Distance(_transform.position, position));

            await Task.Yield();

        }
    }

    private List<RobotMovementPattern> AdjustRobotMovementPattern(List<RobotMovementPattern> robotMovementPattern, float rotateAngle)
    {
        List<RobotMovementPattern> newRobotMovementPattern = new List<RobotMovementPattern>();

        foreach (RobotMovementPattern pattern in robotMovementPattern)
        {
            Vector3 newPosition = RotateVector(pattern.Direction, rotateAngle);
            newRobotMovementPattern.Add(new RobotMovementPattern(newPosition, pattern));
        }

        return newRobotMovementPattern;
    }

    private Vector3 RotateVector(Vector3 vector, float angleRadians)
    {
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        float x = vector.x * cos - vector.z * sin;
        float z = vector.x * sin + vector.z * cos;

        return new Vector3(x, vector.y, z);
    }


    private bool IsPositionReachable(Vector3 position)
    {
        return NavMesh.SamplePosition(position, out _, _navmeshAgent.height, NavMesh.AllAreas);
    }

    private bool RobotMovementPatternReachable(List<RobotMovementPattern> robotMovementPattern)
    {
        bool reachable = true;

        Vector3 currentPosition = _transform.position;

        foreach (RobotMovementPattern pattern in robotMovementPattern)
        {
            Vector3 nextPosition = currentPosition + pattern.Direction;

            if (!IsPositionReachable(nextPosition))
            {
                reachable = false;
                break;
            }

            currentPosition = nextPosition;
        }

        return reachable;
    }

    private void OnDestroy()
    {
        StopMovement();
    }

    private void OnEnable()
    {
        StopMovement();
    }

    private void StopMovement()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

    }


}
