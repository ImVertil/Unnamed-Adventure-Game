using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{    
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _rotationSmoothTime = 0.2f;
    [SerializeField] private CharacterController _controller;

    private Vector2 _inputVector;
    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;

    private float _currentRotationVelocity;

    [SerializeField] private Animator _animator;
    private int _animatorSpeedParamId;
    private string _currentState;


    private void Start()
    {
        _animatorSpeedParamId = Animator.StringToHash("Speed");
        ChangeAnimationState(_currentState);
    }

    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        HandleMovement();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        if(_inputVector != Vector2.zero)
        {
            float rotationAngle = Mathf.Atan2(_inputVector.x, _inputVector.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed), ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);

        _controller.Move(movementVector * Time.deltaTime);
    }

    private void HandleAnimation()
    {
        // figure out animation smoothing/blend trees
        float playerVelocity = Mathf.Max(Mathf.Abs(_controller.velocity.x), Mathf.Abs(_controller.velocity.z));
        _animator.SetFloat(_animatorSpeedParamId, playerVelocity);

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
