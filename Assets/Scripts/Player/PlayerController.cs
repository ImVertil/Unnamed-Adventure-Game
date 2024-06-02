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

    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationSmoothTime = 0.2f;
    private string _currentState;


    private void Start()
    {
        ChangeAnimationState(_currentState);
    }

    private void Update()
    {
        HandleMovement();
        HandleCameraMovement();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = PlayerInputHandler.Instance.Move;
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed), ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);

        //_controller.Move(movementVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed) * Time.deltaTime);
        _controller.Move(movementVector * Time.deltaTime);

    }

    private void HandleCameraMovement()
    {

    }

    private void HandleAnimation()
    {
        // figure out animation smoothing/blend trees

        Vector2 inputVector = PlayerInputHandler.Instance.Move;
        _animator.SetFloat("Speed", _controller.velocity.z);
        //Debug.Log(_controller.velocity.z);

        /*if(inputVector == Vector2.zero)
        {
            ChangeAnimationState("Idle");
        }
        else
        {
            if(_controller.velocity.z <= _speed)
            {
                ChangeAnimationState("RunForward");
            }
            else
            {
                ChangeAnimationState("Sprint");
            }
        }*/

        /*if (inputVector == Vector2.up)
        {
            ChangeAnimationState("RunForward");
        }
        else if (inputVector == Vector2.down)
        {
            ChangeAnimationState("RunBackward");
        }
        else if (inputVector == Vector2.left)
        {
            ChangeAnimationState("RunLeft");
        }
        else if (inputVector == Vector2.right)
        {
            ChangeAnimationState("RunRight");
        }
        else
        {
            ChangeAnimationState("Idle");
        }*/
    }

    private void ChangeAnimationState(string nextState)
    {
        if (_currentState == nextState)
            return;

        _animator.Play(nextState);
        _currentState = nextState;
    }
}
