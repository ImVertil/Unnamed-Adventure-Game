using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public sealed class PlayerController : MonoBehaviour
{
    // ==================== Movement ==================== //
    [Header("Movement")]
    [SerializeField] private float _speed = 4f;
    [SerializeField] private float _combatSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 5.5f;
    [SerializeField] private float _dashSpeed = 6.5f;
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private float _dashSmoothTime = 0f;
    [SerializeField] private float _rotationSmoothTime = 0.15f;
    private CharacterController _controller;

    private float _dashCooldownTime = 2f; // temp
    public bool IsDashOnCooldown { get; private set; } = false; // temp
    public bool IsInDashingState => _currentMovementState == DashingState;

    public float CurrentSpeed => _controller.velocity.magnitude;

    private Vector3 _lookDirection;
    private Vector2 _dashDirection;
    private Vector2 _inputVector;
    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;
    private Vector2 _currentCombatParamVelocity;
    private Vector2 _currentCombatParamVector;
    private float _currentRotationVelocity;

    // ==================== Animations ==================== //
    [Header("Animations")]
    private Animator _animator;
    private Dictionary<WeaponType, int> _animationTriggerMap; // temp
    private int _animatorSpeedParamId;
    private int _animatorPosXParamId;
    private int _animatorPosYParamId;
    private int _animatorDashParamId;

    public bool IsDashAnimPlaying { get; private set; }

    [SerializeField] private AttackChain _attackChain; // temp 
    private int _attackAnimIndex = 0;
    public bool CanAttack { get; private set; } = true;

    // ==================== States ==================== //
    [Header("States")]
    private PlayerMovementState _currentMovementState;
    private PlayerCombatState _currentCombatState;

    public PlayerIdleState IdleState = new PlayerIdleState();
    public PlayerMovingState MovingState = new PlayerMovingState();
    public PlayerDashingState DashingState = new PlayerDashingState();

    public PlayerOutOfCombatState OutOfCombatState = new PlayerOutOfCombatState();
    public PlayerInCombatState InCombatState = new PlayerInCombatState();

    public bool IsInCombat => _currentCombatState == InCombatState;


    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _currentMovementState = IdleState;
        _currentMovementState.EnterState(this);

        _currentCombatState = OutOfCombatState;
        _currentCombatState.EnterState(this);

        _animatorSpeedParamId = Animator.StringToHash("Speed");
        _animatorDashParamId = Animator.StringToHash("Dash");
        _animatorPosXParamId = Animator.StringToHash("PosX");
        _animatorPosYParamId = Animator.StringToHash("PosY");

        // temp
        _animationTriggerMap = new Dictionary<WeaponType, int>
        {
            { WeaponType.BOW, Animator.StringToHash("BowEquip") },
            { WeaponType.STAFF, Animator.StringToHash("StaffEquip") },
            { WeaponType.SWORD, Animator.StringToHash("SwordEquip") }
        };
        PlayerInputHandler.Instance.EquipAction.performed += context => _animator.SetTrigger(_animationTriggerMap[WeaponType.SWORD]);
        PlayerInputHandler.Instance.TestAction.performed += context => _animator.SetTrigger("Unequip");
    }

    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        _currentMovementState.UpdateState(this);
        _currentCombatState.UpdateState(this);
    }

    public void Idle()
    {
        Rotate();
        _currentMovementVector = Vector2.zero;
    }

    public void Move()
    {
        Rotate();

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
        Rotate();

        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * _combatSpeed, ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        _currentCombatParamVector = Vector2.SmoothDamp(_currentCombatParamVector, _inputVector, ref _currentCombatParamVelocity, _smoothTime);
        Vector3 combatMovementVector = new Vector3(_currentCombatParamVector.x, 0f, _currentCombatParamVector.y);
        Vector3 cross = Vector3.Cross(Vector3.up, _lookDirection);
        float dotX = Vector3.Dot(cross, combatMovementVector);
        float dotY = Vector3.Dot(_lookDirection, combatMovementVector);

        SetAnimatorPosParam(dotX, dotY);
        SetAnimatorSpeedParam(_controller.velocity.magnitude);
    }

    public void DashMove()
    {
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _dashDirection * _dashSpeed, ref _currentVelocity, _dashSmoothTime);

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        SetAnimatorSpeedParam(_controller.velocity.magnitude);
    }

    public void Rotate()
    {
        if (IsInCombat)
        {
            Ray ray = Camera.main.ScreenPointToRay(PlayerInputHandler.Instance.Mouse);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (groundPlane.Raycast(ray, out float distanceToGround))
            {
                Vector3 targetPos = ray.GetPoint(distanceToGround);
                _lookDirection = (targetPos - transform.position).normalized;
                float rotationAngle = Mathf.Atan2(_lookDirection.x, _lookDirection.z) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                Debug.DrawLine(transform.position, transform.position + _lookDirection, Color.red);
            }
        }
        else
        {
            if (_inputVector != Vector2.zero)
            {
                float rotationAngle = Mathf.Atan2(_inputVector.x, _inputVector.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
        }
    }

    public void Attack()
    {
        if (!CanAttack)
            return;

        StartCoroutine(PlayAttackAnim());
    }

    public void ChangeMovementState(PlayerMovementState nextState)
    {
        //_currentMovementState.ExitState(this);
        nextState.EnterState(this);
        _currentMovementState = nextState;
    }

    public void ChangeCombatState(PlayerCombatState nextState)
    {
        //_currentCombatState.ExitState(this);
        nextState.EnterState(this);
        _currentCombatState = nextState;
    }

    public void SetAnimatorSpeedParam(float val)
    {
        _animator.SetFloat(_animatorSpeedParamId, val);
    }

    public void SetAnimatorPosParam(float x, float y)
    {
        _animator.SetFloat(_animatorPosXParamId, x);
        _animator.SetFloat(_animatorPosYParamId, y);
    }

    public void SetDashTrigger()
    {
        _animator.SetTrigger(_animatorDashParamId);
    }

    private IEnumerator HandleDash(AnimationEvent animationEvent)
    {
        _dashDirection = _inputVector == Vector2.zero ? new Vector2(transform.forward.x, transform.forward.z) : _inputVector;
        float rotationAngle = Mathf.Atan2(_dashDirection.x, _dashDirection.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);

        IsDashAnimPlaying = true;
        // floatParameter - the value of animation speed (it's increased to 1.33 for dash)
        float waitTime = animationEvent.animatorClipInfo.clip.length / animationEvent.floatParameter - 0.33f;
        yield return new WaitForSeconds(waitTime);
        IsDashAnimPlaying = false;
        _dashDirection = Vector2.zero;

        StartCoroutine(PutDashOnCooldown());
    }

    private IEnumerator PlayAttackAnim()
    {
        int index = _attackAnimIndex++;
        if (index >= _attackChain.AttacksAmount - 1)
            _attackAnimIndex = 0;
        AnimationClip clip = _attackChain.AnimationClips[index];

        CanAttack = false;
        _animator.SetTrigger("Attack");
        yield return new WaitForSeconds(clip.length - _attackChain.NextAttackWindowTime[index]);
        CanAttack = true;
        yield return new WaitForSeconds(_attackChain.NextAttackWindowTime[index]);

        // if player didn't chain another attack after fully completing the animation, set the index back to 0
        if (CanAttack)
            _attackAnimIndex = 0;
    }
    
    // temp
    private IEnumerator PutDashOnCooldown()
    {
        IsDashOnCooldown = true;
        yield return new WaitForSeconds(_dashCooldownTime);
        IsDashOnCooldown = false;
    }
}