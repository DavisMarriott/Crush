using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private Animator animator;
    public GameObject cardContainer;
    [SerializeField] private ConfidenceState confidenceState;
    private Rigidbody2D rb;
    public float maxSpeed;
    private float accelerationTime;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private Vector2 smoothDampVelocity;
    // private float delayTime;
    public InputActionReference move;
    [SerializeField] private AnimationTriggerPlayer animTrigger;
    [SerializeField] private AnimationTriggerCrush animationTriggerCrush;
    private bool _wasMoving = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (confidenceState.Dead == false)
        {
            moveInput = move.action.ReadValue<Vector2>();

            // block left movement but keep reading the input for future self-talk triggers
            if (moveInput.x < 0)
                moveInput.x = 0;
            
            
            if (confidenceState.inConversation)
                moveInput.x = 0;

            bool isMoving = moveInput.x != 0;

            // start walking
            if (isMoving && !_wasMoving)
                animTrigger.Walk();
            // stop walking
            else if (!isMoving && _wasMoving)
            {
                if(!confidenceState.inConversation)
                    animTrigger.Default();
            }

            _wasMoving = isMoving;
        }

        // {
        //     horizontalInput = Input.GetAxisRaw("Horizontal"); // Get raw input (-1, 0, or 1)
        // }
    }

    // Set InConversation to true
    public void InConversation()
    {
        confidenceState.inConversation = true;
        cardContainer.SetActive(true);
        GetConfidencePose();
        animationTriggerCrush.GetCharmPose();
    }


    // this method controls the boy's confidence poses - calls methods define in AnimationTriggerPlayer.
    // Not currently working correctly.
     public void GetConfidencePose()
     {
         //tweak these numbers to edit confidence range/pose pairing
         //grabs first true condition
         if (confidenceState.confidence <= 0)
             animTrigger.EnterDeathOne();
         else if (confidenceState.confidence <= 3)
         {
             animTrigger.EnterStateThree();
         }
             
         else if (confidenceState.confidence <= 9)
             animTrigger.EnterStateTwo();
         else
             animTrigger.EnterStateOne();
     }

    void FixedUpdate()
    {
        // acceleration feels different depending on what anim state we're in
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


        Vector2 targetVelocity = moveInput * maxSpeed;
        currentVelocity.x = Mathf.SmoothDamp(currentVelocity.x, targetVelocity.x, ref smoothDampVelocity.x, accelerationTime);
        rb.linearVelocity = currentVelocity;
    }
}
