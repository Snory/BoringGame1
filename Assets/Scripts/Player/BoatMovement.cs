using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BoatMovement : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Transform _boatTransform;
    private Vector3 _middlePosition;
    private Camera _main;

    private RectTransform _directionRectTransform;

    [SerializeField]
    private GameObject _directionPointerObject;

    [SerializeField]
    private Image _forcePointer;

    private bool _isDragging;

    [SerializeField]
    private float _minForwardForce;

    [SerializeField]
    private float _maxForwardForce;

    [SerializeField]
    private float _maxMagnitudeInPercent;

    [SerializeField]
    private float _degradationForce;

    [SerializeField]
    private float _minSpeed;

    [SerializeField]
    private TextMeshProUGUI _debugText;

    [SerializeField]
    private TextMeshProUGUI _debugText2;

    float _maxMagnitude;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _boatTransform = transform;
        _main = Camera.main;
        _directionRectTransform = _directionPointerObject.GetComponent<RectTransform>();
        _directionPointerObject.SetActive(false);
        _maxMagnitude = Mathf.Sqrt(Screen.width * Screen.width + Screen.height + Screen.height) * _maxMagnitudeInPercent;
    }

    // Update is called once per frame
    void Update()
    {
        string text = string.Empty;

        if (Input.GetMouseButtonDown(0))
        {
            _middlePosition = _main.WorldToScreenPoint(_boatTransform.position);
            _directionPointerObject.SetActive(true);
            _isDragging = true;
        }

        if (_isDragging)
        {
            Vector3 directionMouseToMiddle = Input.mousePosition - _middlePosition;

            float angle = (Mathf.Atan2(directionMouseToMiddle.y, directionMouseToMiddle.x) * Mathf.Rad2Deg);
            angle = 180 - (angle + 270) % 360;
            _directionRectTransform.localRotation = Quaternion.Euler(0, 0, angle - _boatTransform.rotation.eulerAngles.y);


            float magnitude = directionMouseToMiddle.magnitude;

            if (magnitude > _maxMagnitude)
            {
                magnitude = _maxMagnitude;
            }

            _forcePointer.fillAmount = magnitude / _maxMagnitude;

            if (Input.GetMouseButtonUp(0))
            {
                _rigidBody.velocity = Vector3.zero;
                _boatTransform.rotation = Quaternion.Euler(0, angle, 0);
                _directionRectTransform.localRotation = Quaternion.Euler(0, 0, 0);

                float force = Mathf.Lerp(_minForwardForce, _maxForwardForce, magnitude / _maxMagnitude);
                text += "Force: " + force;

                _debugText2.text = text;

                _rigidBody.AddForce(_boatTransform.forward * force, ForceMode.VelocityChange);
                _isDragging = false;
                _directionPointerObject.SetActive(false);
            }
        }

        text += " Velocity: " + _rigidBody.velocity.ToString();

        _debugText.text = text;

        _rigidBody.velocity = _rigidBody.velocity / (1 + (_degradationForce * Time.deltaTime));

        if (_rigidBody.velocity.magnitude < _minSpeed)
        {
            _rigidBody.velocity = Vector3.zero;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        _rigidBody.velocity = Vector3.zero;   
    }
}
