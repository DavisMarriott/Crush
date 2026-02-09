using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ConfidenceState confidenceState;
    public Rigidbody2D rb;
    public float speed;
    private Vector2 _moveDirection;
    public InputActionReference move;
    
    // Update is called once per frame
    void Update()
    {
        if (confidenceState.Dead == false)
        {
            _moveDirection = move.action.ReadValue<Vector2>();
        }
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void FixedUpdate()
    {
        if (confidenceState.Dead == false)
        {
            rb.linearVelocity = new Vector2(_moveDirection.x * speed,0);
        }
    }
}
