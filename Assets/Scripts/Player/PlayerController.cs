using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 4f;
    [SerializeField] private float _sprintSpeed = 6f;
    [SerializeField] private float _dashSpeed = 7f;
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _dashSmoothTime = 0.05f;
    [SerializeField] private float _rotationSmoothTime = 0.2f;
    [SerializeField] private CharacterController _controller;

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
    }

    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_inputVector != Vector2.zero && !_animator.GetBool(_animatorDashParamId))
        {
            float rotationAngle = Mathf.Atan2(_inputVector.x, _inputVector.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        if (PlayerInputHandler.Instance.Dash && !_animator.GetBool(_animatorDashParamId))
        {
            StartCoroutine(PlayDashAnim());
        }

        if (_animator.GetBool(_animatorDashParamId))
        {
            Vector2 dashDirection = new Vector2(transform.forward.x, transform.forward.z);
            _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, dashDirection * _dashSpeed, ref _currentVelocity, _dashSmoothTime);
        }
        else
        {
            _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed), ref _currentVelocity, _smoothTime);
        }

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        _animator.SetFloat(_animatorSpeedParamId, _controller.velocity.magnitude);

        //Debug.Log(_controller.velocity.magnitude);
        //Debug.Log(_controller.velocity);
    }

    private IEnumerator PlayDashAnim()
    {
        float waitVal = _animator.GetNextAnimatorStateInfo(1).length;
        _animator.SetBool(_animatorDashParamId, true);
        yield return new WaitForSeconds(waitVal - 0.3f);
        _animator.SetBool(_animatorDashParamId, false);
    }
}
