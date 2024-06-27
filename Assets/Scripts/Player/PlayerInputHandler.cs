using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    [SerializeField] private InputActionAsset _controls;
    private InputActionMap _map;

    // ==================== Movement ==================== //
    public Vector2 Move { get; private set; }
    public bool Sprint { get; private set; }
    public bool Dash { get; private set; }

    private InputAction _moveAction;
    private InputAction _sprintAction;
    private InputAction _dashAction;


    // ==================== Mouse ==================== //
    public Vector2 Mouse { get; private set; }
    private InputAction _mouseAction;


    // ==================== Combat ==================== //
    public bool Attack { get; private set; }
    public bool Equip { get; private set; }

    private InputAction _attackAction;
    [HideInInspector] public InputAction EquipAction;


    // ==================== Test ==================== //
    [HideInInspector] public InputAction TestAction;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _map = _controls.FindActionMap("Player");

        _moveAction = _map.FindAction("Movement");
        _sprintAction = _map.FindAction("Sprint");
        _dashAction = _map.FindAction("Dash");

        _mouseAction = _map.FindAction("Mouse");

        _attackAction = _map.FindAction("Attack");
        EquipAction = _map.FindAction("Equip");

        TestAction = _map.FindAction("TestAction");

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        _moveAction.performed += context => Move = context.ReadValue<Vector2>();
        _moveAction.canceled += context => Move = Vector2.zero;

        _sprintAction.performed += context => Sprint = context.ReadValueAsButton();
        _sprintAction.canceled += context => Sprint = context.ReadValueAsButton();

        _dashAction.performed += context => Dash = context.ReadValueAsButton();
        _dashAction.canceled += context => Dash = context.ReadValueAsButton();

        _mouseAction.performed += context => Mouse = context.ReadValue<Vector2>();

        _attackAction.performed += context => Attack = context.ReadValueAsButton();
        _attackAction.canceled += context => Attack = context.ReadValueAsButton();

        EquipAction.performed += context => Equip = context.ReadValueAsButton();
        EquipAction.canceled += context => Equip = context.ReadValueAsButton();
    }

    private void OnEnable() => _map.Enable();

    private void OnDisable() => _map.Disable();
}
