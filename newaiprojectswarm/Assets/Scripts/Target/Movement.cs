using UnityEngine;

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private int moveSpeed = 5;
    
    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody rb;
    
    public InputAction playerControls;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }


    void Update()
    {
        moveDirection = playerControls.ReadValue<Vector3>().normalized;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + moveDirection * (Time.fixedDeltaTime * moveSpeed));
    }
}