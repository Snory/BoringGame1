using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform _targetToFollowTransform;

    [SerializeField]
    private float _cameraSpeed;

    [SerializeField]
    private float _cameraSpeedAdjustmentPerDistance;

    [SerializeField]
    private Vector3 _offset;

    private Transform _cameraTransform;

    private void Awake()
    {
        _cameraTransform = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = _targetToFollowTransform.transform.position + _offset;

        float distnace = Vector3.Distance(newPosition, _cameraTransform.position);

        _cameraTransform.position = Vector3.MoveTowards(_cameraTransform.position, newPosition, (_cameraSpeed + distnace/_cameraSpeedAdjustmentPerDistance) * Time.deltaTime);
    }
}
