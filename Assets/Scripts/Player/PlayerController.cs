using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private CharacterController _controller;

    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;


    private void Start()
    {

    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = PlayerInputHandler.Instance.Move;
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, inputVector, ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);

        _controller.Move(movementVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed) * Time.deltaTime);
    }
}
