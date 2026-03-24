using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private Animator animator;
    [SerializeField] private ConfidenceState confidenceState;
    private Rigidbody2D rb;
    public float maxSpeed;
    private float accelerationTime; // Time to reach max speed
    private Vector2 moveInput;
    private Vector2 currentVelocity; // Velocity tracked by SmoothDamp
    private Vector2 smoothDampVelocity; // Reference velocity for SmoothDamp
    // private float delayTime;
    public InputActionReference move;
    [SerializeField] private AnimationTriggerPlayer animTrigger;
    private bool _wasMoving = false;
    private string _lastPose = "";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        if (confidenceState.Dead == false)
        {
            //moves player on x axis
            moveInput = move.action.ReadValue<Vector2>();

            // Block leftward movement (A key) but still read the input
            // so we can use it later for self-talk triggers
            if (moveInput.x < 0)
                moveInput.x = 0;

            bool isMoving = moveInput.x != 0;
        
            //animation control
                //if player is pressing a or d, enter walk state
            if (isMoving && !_wasMoving)
                animTrigger.Walk();
            else if (!isMoving && _wasMoving)
            {
                animTrigger.Default();
            }
    
            _wasMoving = isMoving;
        }
        
        // {
        //     horizontalInput = Input.GetAxisRaw("Horizontal"); // Get raw input (-1, 0, or 1)
        // }
    }

    //this method controls the boy's confidence poses
    private string GetConfidencePose()
    {
        int c = confidenceState.confidence;
    
        //tweak these numbers to edit confidence range/pose pairing
        //grabs first true condition
        if (c <= 0) return "PlayerConfidence_00";
        if (c <= 3) return "PlayerConfidence_01";
        if (c <= 6) return "PlayerConfidence_02";
        if (c <= 9) return "PlayerConfidence_03";
        if (c <= 12) return "PlayerConfidence_04";
        if (c <= 15) return "PlayerConfidence_05";
        return "PlayerConfidence_06";
    }
    
    void FixedUpdate()
    {
        // Set accelerationTime based on current state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Player_Start_CYCLE"))
        {
            accelerationTime = 0.40f;
            // delayTime = .25f;
        }
        if (stateInfo.IsName("Player_State01_CYCLE"))
        {
            accelerationTime = 0.30f;
            // delayTime = .15f;
        }
        else
        {
            accelerationTime = 0.12f;
            // delayTime = .25f;
        }
        
        
        // Calculate the target velocity based on input and max speed
        Vector2 targetVelocity = moveInput * maxSpeed;

        // Use Vector2.SmoothDamp to gradually change current velocity towards the target velocity
        
        currentVelocity.x = Mathf.SmoothDamp(currentVelocity.x, targetVelocity.x, ref smoothDampVelocity.x, accelerationTime);

        // Apply the smoothed velocity to the Rigidbody
        rb.linearVelocity = currentVelocity;
    }
}
