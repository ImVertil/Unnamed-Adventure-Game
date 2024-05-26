using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private CharacterController _controller;


    private void Start()
    {
        _controller = GetComponent<CharacterController>();    
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = PlayerInputHandler.Instance.Move;
        Vector3 movementVector = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        _controller.Move(movementVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed) * Time.deltaTime);
    }
}
