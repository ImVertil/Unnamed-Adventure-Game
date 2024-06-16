using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 4.5f;
    [SerializeField] private float _sprintSpeed = 5.75f;
    [SerializeField] private float _dashSpeed = 7f;
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private float _dashSmoothTime = 0f;
    [SerializeField] private float _rotationSmoothTime = 0.15f;
    [SerializeField] private CharacterController _controller;

    // TODO: change this later after getting dash anim, not roll anim
    private float _dashCooldownTime = 2f;
    private bool _dashOnCooldown = false;

    private Vector2 _inputVector;
    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;
    private float _currentRotationVelocity;

    [Header("Animations")]
    [SerializeField] private Animator _animator;
    private int _animatorSpeedParamId;
    private int _animatorDashParamId;
    

    private void Start()
    {
        _animatorSpeedParamId = Animator.StringToHash("Speed");
        _animatorDashParamId = Animator.StringToHash("IsDashing");
        PlayerInputHandler.Instance.dashAction.performed += Dash;
    }


    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        HandleMovement();
    }

    private void HandleMovement()
    {
        // TODO: Replace if statements readability with player states
        if (_inputVector != Vector2.zero && !_animator.GetBool(_animatorDashParamId))
        {
            float rotationAngle = Mathf.Atan2(_inputVector.x, _inputVector.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        if (_animator.GetBool(_animatorDashParamId))
        {
            Vector2 dashDirection = new Vector2(transform.forward.x, transform.forward.z);
            _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, dashDirection * _dashSpeed, ref _currentVelocity, _dashSmoothTime);
        }
        else
        {
            if (_controller.velocity.magnitude >= _dashSpeed - 0.01f)
            {
                _currentMovementVector = _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed);
            }
            else
            {
                _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed), ref _currentVelocity, _smoothTime);
            }
        }

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        _animator.SetFloat(_animatorSpeedParamId, _controller.velocity.magnitude);
    }
    private void Dash(InputAction.CallbackContext context)
    {
        if (_animator.GetBool(_animatorSpeedParamId) || _dashOnCooldown)
            return;
        
        StartCoroutine(HandleDash());
    }

    private IEnumerator HandleDash()
    {
        _animator.SetBool(_animatorDashParamId, true);
        
        yield return new WaitForEndOfFrame();
        float waitTime = _animator.GetNextAnimatorStateInfo(1).length - 0.33f;
        yield return new WaitForSeconds(waitTime);

        _animator.SetBool(_animatorDashParamId, false);

        StartCoroutine(PutDashOnCooldown());
    }
    
    private IEnumerator PutDashOnCooldown()
    {
        _dashOnCooldown = true;
        yield return new WaitForSeconds(_dashCooldownTime);
        _dashOnCooldown = false;
    }
}
