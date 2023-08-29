using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RobotPatternMovement : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _navmeshAgent;

    [SerializeField]
    private List<MovementPattern> _robotMovementPatterns;

    [SerializeField]
    private Transform _transform;
    private CancellationTokenSource _cancellationTokenSource;

    [SerializeField]
    private float _nextAngleIncrement;

    [SerializeField]
    private bool _debugMovementPattern;


    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        StartRobotPatternMovement(cancellationToken);
    }

    public async void StartRobotPatternMovement(CancellationToken cancellationToken)
    {
    
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                List<MovementPattern> robotMovementPattern = _robotMovementPatterns;

                float angle = 0;
                bool reachable = false;

                for (int i = 0; i < 360 / _nextAngleIncrement; i++)
                {

                    Debug.Log("Searching for next angle: " + angle.ToString());

                    reachable = RobotMovementPatternReachable(robotMovementPattern);

                    Debug.Log("Reachable: " + reachable.ToString());

                    if (reachable)
                    {
                        break;
                    }
                                
                    angle += _nextAngleIncrement;

                    robotMovementPattern = AdjustRobotMovementPattern(robotMovementPattern, angle);
                }

                //start async movement with given robotMovementPattern
                if (reachable)
                {
                    await Move(robotMovementPattern, cancellationToken);
                } else
                {
                    //tady to bude chtít nìjakej totálnì random point, ale nejsem si jistý, zda by to mìl èi nemìl být jiný pohyb
                }

            }
            catch (TaskCanceledException ex)
            {
                Debug.Log(ex.ToString());
            }
            catch
            {
                throw;
            }
        }
    }

    public async Task Move(List<MovementPattern> robotMovementPatterns, CancellationToken cancellationToken)
    {
        foreach (var pattern in robotMovementPatterns)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await MoveRoutine(pattern, cancellationToken);
            }
        }
    }

    public async Task MoveRoutine(MovementPattern pattern, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            Vector3 newPosition = _transform.position + pattern.Direction;
            _navmeshAgent.speed = pattern.Speed;
            _navmeshAgent.destination = newPosition;

            await WaitTillReachDestination(cancellationToken, newPosition); //dojdi na pozici
            await Task.Delay(System.TimeSpan.FromSeconds(pattern.NextPositionWaitTime), cancellationToken); //poèkej tam jak dlouho uznáš za vhodné
        }
    }

    public async Task WaitTillReachDestination(CancellationToken cancellationToken, Vector3 position)
    {
        while (Vector3.Distance(_transform.position, position) > 0.1 && !cancellationToken.IsCancellationRequested)
        {
            await Task.Yield();
        }
    }

    private List<MovementPattern> AdjustRobotMovementPattern(List<MovementPattern> robotMovementPattern, float rotateAngle)
    {
        List<MovementPattern> newRobotMovementPattern = new List<MovementPattern>();

        foreach (MovementPattern pattern in robotMovementPattern)
        {
            Vector3 newPosition = RotateVector(pattern.Direction, rotateAngle);
            newRobotMovementPattern.Add(new MovementPattern(newPosition, pattern));
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

    private bool RobotMovementPatternReachable(List<MovementPattern> robotMovementPattern)
    {
        bool reachable = true;

        Vector3 currentPosition = _transform.position;

        foreach (MovementPattern pattern in robotMovementPattern)
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


    private void OnDrawGizmos()
    {
        if (_debugMovementPattern)
        {
            Gizmos.color = Color.green;
            Vector3 position = _transform.position;
            foreach (var pattern in _robotMovementPatterns)
            {
                Gizmos.DrawRay(position, pattern.Direction);
                Gizmos.DrawSphere(position, 0.4f);
                position = position + pattern.Direction;
            }
        }
    }


}
