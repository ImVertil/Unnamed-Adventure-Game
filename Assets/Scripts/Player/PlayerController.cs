using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private float _dashCooldownTime = 2f;
    public bool dashOnCooldown { get; private set; } = false;
    public bool isDashing { get; private set; } = false;

    private Vector2 _inputVector;
    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;
    private float _currentRotationVelocity;

    [Header("Animations")]
    [SerializeField] private Animator _animator;
    private int _animatorSpeedParamId;
    private int _animatorDashParamId;
    private int _animatorPosXParamId;
    private int _animatorPosYParamId;

    [Header("States")]
    private PlayerState _currentState;
    public PlayerIdleState IdleState = new PlayerIdleState();
    public PlayerMovingState MovingState = new PlayerMovingState();
    public PlayerDashingState DashingState = new PlayerDashingState();


    private void Start()
    {
        _currentState = IdleState;
        _currentState.EnterState(this);

        _animatorSpeedParamId = Animator.StringToHash("Speed");
        _animatorDashParamId = Animator.StringToHash("IsDashing");
        _animatorPosXParamId = Animator.StringToHash("PosX");
        _animatorPosYParamId = Animator.StringToHash("PosY");

        PlayerInputHandler.Instance.attackAction.performed += Test;
        PlayerInputHandler.Instance.testAction.performed += Test2;
        PlayerInputHandler.Instance.testAction2.performed += Test3;
    }


    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        _currentState.UpdateState(this);
    }

    private void Test(InputAction.CallbackContext context)
    {
        Debug.Log("attack");
        _animator.Play("MeleeAttack_OneHanded", 2);
        //_animator.SetTrigger("Attack");
    }

    private void Test2(InputAction.CallbackContext context)
    {
        Debug.Log("sword equip");
        _animator.SetTrigger("SwordEquip");
    }

    private void Test3(InputAction.CallbackContext context)
    {
        Debug.Log("unequip");
        _animator.SetTrigger("Unequip");
    }

    public void Move()
    {
        if(_inputVector != Vector2.zero)
        {
            float rotationAngle = Mathf.Atan2(_inputVector.x, _inputVector.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        if (_controller.velocity.magnitude >= _dashSpeed - 0.01f)
        {
            _currentMovementVector = _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed);
        }
        else
        {
            _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * (PlayerInputHandler.Instance.Sprint ? _sprintSpeed : _speed), ref _currentVelocity, _smoothTime);
        }

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        SetAnimatorSpeedParam(_controller.velocity.magnitude);
    }

    public void CombatMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(PlayerInputHandler.Instance.Mouse);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        Vector3 lookDirection = Vector3.zero;

        if (groundPlane.Raycast(ray, out float distanceToGround))
        {
            Vector3 targetPos = ray.GetPoint(distanceToGround);
            lookDirection = (targetPos - transform.position).normalized;
            float rotationAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            Debug.DrawLine(transform.position, transform.position + lookDirection, Color.red);
        }

        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector, ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * _speed * Time.deltaTime);

        Vector3 cross = Vector3.Cross(Vector3.up, lookDirection);
        float dotX = Vector3.Dot(cross, movementVector);
        float dotY = Vector3.Dot(lookDirection, movementVector);
        _animator.SetFloat(_animatorPosXParamId, dotX);
        _animator.SetFloat(_animatorPosYParamId, dotY);
    }

    public void Dash()
    {
        if (!GetDashingStatus() && !dashOnCooldown)
        {
            StartCoroutine(HandleDash());
        }

        Vector2 dashDirection = new Vector2(transform.forward.x, transform.forward.z);
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, dashDirection * _dashSpeed, ref _currentVelocity, _dashSmoothTime);

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        SetAnimatorSpeedParam(_controller.velocity.magnitude);
    }

    public void ChangeState(PlayerState nextState)
    {
        //_currentState.ExitState(this);
        nextState.EnterState(this);
        _currentState = nextState;
        //_currentState.EnterState(this);
    }

    public float GetPlayerSpeed()
    {
        return _controller.velocity.magnitude;
    }

    public bool GetDashingStatus()
    {
        return _animator.GetBool(_animatorDashParamId);
    }

    public void SetAnimatorSpeedParam(float val)
    {
        _animator.SetFloat(_animatorSpeedParamId, val);
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
    
    // temp
    private IEnumerator PutDashOnCooldown()
    {
        dashOnCooldown = true;
        yield return new WaitForSeconds(_dashCooldownTime);
        dashOnCooldown = false;
    }
}
