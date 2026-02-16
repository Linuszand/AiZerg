using UnityEngine;

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public InputAction playerControls;
    Vector3 moveDirection = Vector3.zero;
    private Rigidbody rb;
    private int moveSpeed = 5;
    
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
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }
}