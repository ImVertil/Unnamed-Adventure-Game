using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(PlayerInputHandler.Instance.Dash)
        {
            Test();
        }
    }

    private void Test()
    {
        Debug.Log("A");
    }
}
