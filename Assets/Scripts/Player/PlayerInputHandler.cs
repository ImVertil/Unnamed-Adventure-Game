using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    [SerializeField] private InputActionAsset _controls;
    private InputActionMap _map;

    // ===== Movement ===== //
    public Vector2 Move { get; private set; }
    public bool Sprint { get; private set; }
    public bool Dash { get; private set; }

    private InputAction moveAction;
    private InputAction sprintAction;
    public InputAction dashAction;

    // ===== Attacks ===== //
    public InputAction attackAction;

    // ===== Test ===== //
    public InputAction testAction;
    public InputAction testAction2;

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

        moveAction = _map.FindAction("Movement");
        sprintAction = _map.FindAction("Sprint");
        dashAction = _map.FindAction("Dash");

        attackAction = _map.FindAction("Attack");

        testAction = _map.FindAction("TestAction");
        testAction2 = _map.FindAction("TestAction2");

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => Move = context.ReadValue<Vector2>();
        moveAction.canceled += context => Move = Vector2.zero;

        sprintAction.performed += context => Sprint = context.ReadValueAsButton();
        sprintAction.canceled += context => Sprint = context.ReadValueAsButton();

        dashAction.performed += context => Dash = context.ReadValueAsButton();
        dashAction.canceled += context => Dash = context.ReadValueAsButton();
    }

    private void OnEnable() => _map.Enable();

    private void OnDisable() => _map.Disable();
}
