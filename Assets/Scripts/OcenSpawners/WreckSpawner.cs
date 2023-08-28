
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class WreckSpawner : MonoBehaviour
{
    private Transform _environmentTransform;

    [SerializeField]
    private float _spawnTime;

    [SerializeField]
    private GameObject _wreckPrefab;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private float _distanceFromOthers;

    [SerializeField]
    private LayerMask _wreckMask;

    private CancellationTokenSource _cancellationTokenSource;


    [SerializeField]
    private bool _spawning;

    public async void StartSpawning()
    {
        _spawning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        await SpawnWreckRoutine(_cancellationTokenSource.Token);
    }


    public async Task SpawnWreckRoutine(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            SpawnWreck();

            try
            {
                await Task.Delay(System.TimeSpan.FromSeconds(_spawnTime), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void SpawnWreck()
    {
        Vector3 position = GetWreckPosition();

        if (position == Vector3.zero)
        {
            return;
        }

        Instantiate(_wreckPrefab, position, Quaternion.identity, _environmentTransform);
    }

    private Vector3 GetWreckPosition()
    {
        //20 is generate test
        for (int i = 0; i < 20; i++)
        {
            float randomAngle = Mathf.PI * 2 * Random.Range(0f, 1f);
            float x = Mathf.Cos(randomAngle) * Random.Range(0f, _distance);
            float z = Mathf.Sin(randomAngle) * Random.Range(0f, _distance);

            Vector3 positionCandidate = new Vector3(x, this.transform.position.y, z) + this.transform.position;

            Collider[] overlaps = Physics.OverlapSphere(new Vector3(x, this.transform.position.y, z), _distanceFromOthers, _wreckMask);

            if (overlaps.Length > 0)
            {
                continue;
            }

            return positionCandidate;
        }

        return Vector3.zero;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        if (_spawning)
        {
            return;
        }

        StartSpawning();
    }

    private void OnTriggerExit(Collider other)
    {
        StopSpawning();
    }

    private void StopSpawning()
    {
        _spawning = false;
        if(_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    private void OnDestroy()
    {
        StopSpawning();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, _distance);
    }
}
