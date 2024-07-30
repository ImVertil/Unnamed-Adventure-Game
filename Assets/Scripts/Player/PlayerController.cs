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

    public float Speed => _speed;
    public float CombatSpeed => _combatSpeed;
    public float SprintSpeed => _sprintSpeed;
    public float DashSpeed => _dashSpeed;
    public float SmoothTime => _smoothTime;

    private float _dashCooldownTime = 2f; // temp
    public bool IsDashOnCooldown { get; private set; } = false; // temp
    public bool IsInDashingState => _currentMovementState == DashingState;

    public float CurrentSpeed => _controller.velocity.magnitude;
    public float CurrentSqrSpeed => _controller.velocity.sqrMagnitude;

    private Vector2 _currentVelocity;
    private Vector2 _currentMovementVector;
    private float _currentRotationVelocity;

    // ==================== Animations ==================== //
    [Header("Animations")]
    [SerializeField] private float _attackTransitionDuration = 0.25f;
    private Animator _animator;
    private Dictionary<WeaponType, int> _animationTriggerMap;
    private int _animatorSpeedParamId = Animator.StringToHash("Speed");
    private int _animatorPosXParamId = Animator.StringToHash("PosX");
    private int _animatorPosYParamId = Animator.StringToHash("PosY");
    private int _animatorDashParamId = Animator.StringToHash("Dash");
    private int _animatorAttackLayerId;

    public bool IsDashAnimPlaying { get; private set; }

    // ==================== Attacks ==================== //
    [Header("Attacks")]
    [SerializeField] private AttackChain _attackChain; // temp 
    private BoxCollider _attackCollider;
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
        _attackCollider = GetComponent<BoxCollider>();
        _animator = GetComponent<Animator>();

        _currentMovementState = IdleState;
        _currentMovementState.EnterState(this);

        _currentCombatState = OutOfCombatState;
        _currentCombatState.EnterState(this);

        _animatorAttackLayerId = _animator.GetLayerIndex("AttackLayer");
        
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
        _currentMovementState.UpdateState(this);
        _currentCombatState.UpdateState(this);
    }

    public void Idle()
    {
        _currentMovementVector = Vector2.zero;
    }

    public void Move(Vector2 direction, float speed)
    {
        //if (CurrentSpeed >= _dashSpeed - 0.01f)
        if (CurrentSqrSpeed >= _dashSpeed * _dashSpeed - 0.01f)
        {
            _currentMovementVector = direction * speed;
        }
        else
        {
            _currentMovementVector = Vector2.SmoothDamp(_currentMovementVector, direction * speed, ref _currentVelocity, _smoothTime);
        }

        Vector3 movementVector = new Vector3(_currentMovementVector.x, 0f, _currentMovementVector.y);
        _controller.Move(movementVector * Time.deltaTime);

        SetAnimatorSpeedParam(CurrentSpeed);
    }

    public void Rotate(Vector2 direction)
    {
        float rotationAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _currentRotationVelocity, _rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    public void Attack()
    {
        StartCoroutine(HandleAttack());
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

    public void SetAttackCollider(float width, float height, bool isReset = false)
    {
        if (isReset)
        {
            _attackCollider.center = new Vector3(0f, _attackCollider.center.y, 0f);
            _attackCollider.size = new Vector3(width, _attackCollider.size.y, height);
        }
        else
        {
            _attackCollider.center = new Vector3(0f, _attackCollider.center.y, height / 2);
            _attackCollider.size = new Vector3(width, _attackCollider.size.y, height);
        }
    }

    public void SetDashTrigger()
    {
        _animator.SetTrigger(_animatorDashParamId);
    }

    public Vector3 GetDirectionTowardsMouseCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(PlayerInputHandler.Instance.Mouse);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        Vector3 lookDirection = Vector3.zero;

        if (groundPlane.Raycast(ray, out float distanceToGround))
        {
            Vector3 targetPos = ray.GetPoint(distanceToGround);
            Vector3 heading = targetPos - transform.position;
            //lookDirection = (targetPos - transform.position).normalized;
            lookDirection = heading / heading.magnitude;
        }

        return lookDirection;
    }

    private IEnumerator HandleDash(AnimationEvent animationEvent)
    {
        IsDashAnimPlaying = true;
        // floatParameter - the value of animation speed (it's increased to 1.33 for dash)
        float waitTime = animationEvent.animatorClipInfo.clip.length / animationEvent.floatParameter - 0.33f;
        yield return new WaitForSeconds(waitTime);
        IsDashAnimPlaying = false;

        StartCoroutine(PutDashOnCooldown());
    }

    private IEnumerator HandleAttack()
    {
        int index = _attackAnimIndex++;
        if (index >= _attackChain.AttacksAmount - 1)
            _attackAnimIndex = 0;
        WeaponAttack attack = _attackChain.Attacks[index];

        CanAttack = false;
        _animator.CrossFade(attack.Clip.name, _attackTransitionDuration, _animatorAttackLayerId);
        SetAttackCollider(attack.HitWidth, attack.HitHeight);
        yield return new WaitForSeconds(attack.Clip.length - attack.NextAttackWindowTime);
        CanAttack = true;
        yield return new WaitForSeconds(attack.NextAttackWindowTime);
        // if player didn't chain another attack after fully completing the animation, set the index back to 0
        if (CanAttack)
        {
            _attackAnimIndex = 0;
            SetAttackCollider(1f, 1f, true);
        }      
    }
    
    // temp
    private IEnumerator PutDashOnCooldown()
    {
        IsDashOnCooldown = true;
        yield return new WaitForSeconds(_dashCooldownTime);
        IsDashOnCooldown = false;
    }
}
