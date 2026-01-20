using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    
    public Rigidbody2D rb;
    public float speed;
    private Vector2 _moveDirection;
    public InputActionReference move;
    
    // Update is called once per frame
    void Update()
    {
        _moveDirection = move.action.ReadValue<Vector2>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(_moveDirection.x * speed,0);
    }

   
}
