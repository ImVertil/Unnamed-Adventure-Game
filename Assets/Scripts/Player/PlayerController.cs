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

    private Vector2 _inputVector;
    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;
    private Vector2 _currentCombatParamVelocity;
    private Vector2 _currentCombatParamVector;
    private float _currentRotationVelocity;

    // ==================== Animations ==================== //
    [Header("Animations")]
    private Animator _animator;
    private Dictionary<string, int> _animationTriggerMap; // temp, replace string with WeaponType enum or think of something else
    public int AnimatorSpeedParamId { get; private set; }
    public int AnimatorDashParamId { get; private set; }
    public int AnimatorPosXParamId { get; private set; }
    public int AnimatorPosYParamId { get; private set; }

    public bool IsDashAnimPlaying { get; private set; }

    [SerializeField] private AttackChain _attackChain; // temp 
    private int _attackAnimIndex = 0;
    public bool CanAttack = true;

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

        AnimatorSpeedParamId = Animator.StringToHash("Speed");
        AnimatorDashParamId = Animator.StringToHash("Dash");
        AnimatorPosXParamId = Animator.StringToHash("PosX");
        AnimatorPosYParamId = Animator.StringToHash("PosY");

        // temp, for testing purposes
        _animationTriggerMap = new Dictionary<string, int>
        {
            { "Sword", Animator.StringToHash("SwordEquip") },
            { "Bow", Animator.StringToHash("BowEquip") }
        };
        PlayerInputHandler.Instance.EquipAction.performed += context => _animator.SetTrigger(_animationTriggerMap["Sword"]);
    }

    private void Update()
    {
        _inputVector = PlayerInputHandler.Instance.Move;
        _currentMovementState.UpdateState(this);
        _currentCombatState.UpdateState(this);
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

        // Values used for movement
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, _inputVector * _combatSpeed, ref _currentVelocity, _smoothTime);
        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        // Values used for animator parameters
        _currentCombatParamVector = Vector2.SmoothDamp(_currentCombatParamVector, _inputVector, ref _currentCombatParamVelocity, _smoothTime);
        Vector3 combatMovementVector = new Vector3(_currentCombatParamVector.x, 0f, _currentCombatParamVector.y);
        Vector3 cross = Vector3.Cross(Vector3.up, lookDirection);
        float dotX = Vector3.Dot(cross, combatMovementVector);
        float dotY = Vector3.Dot(lookDirection, combatMovementVector);

        SetAnimatorPosParam(dotX, dotY);
    }

    public void DashMove()
    {
        Vector2 dashDirection = new Vector2(transform.forward.x, transform.forward.z);
        _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, dashDirection * _dashSpeed, ref _currentVelocity, _dashSmoothTime);

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        SetAnimatorSpeedParam(_controller.velocity.magnitude);
    }
    public void Attack()
    {
        if (!CanAttack)
            return;

        for(int i=0; i<10000; i++)
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
        _animator.SetFloat(AnimatorSpeedParamId, val);
    }

    public void SetAnimatorPosParam(float x, float y)
    {
        _animator.SetFloat(AnimatorPosXParamId, x);
        _animator.SetFloat(AnimatorPosYParamId, y);
    }

    public void SetAnimationTrigger(int triggerParamId)
    {
        _animator.SetTrigger(triggerParamId);
    }

    private IEnumerator HandleDash(AnimationEvent animationEvent)
    {
        IsDashAnimPlaying = true;
        // Float parameter - the value of animation speed (it's increased to 1.33 for dash)
        float waitTime = animationEvent.animatorClipInfo.clip.length / animationEvent.floatParameter - 0.33f;
        yield return new WaitForSeconds(waitTime);
        IsDashAnimPlaying = false;

        StartCoroutine(PutDashOnCooldown());
    }

    private IEnumerator PlayAttackAnim()
    {
        int index = _attackAnimIndex++;
        if (index >= _attackChain.AttacksAmount - 1)
            _attackAnimIndex = 0;

        CanAttack = false;
        AnimationClip clip = _attackChain.AnimationClips[index];
        _animator.Play(clip.name, 2);
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