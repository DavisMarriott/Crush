using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private Animator animator;
    [SerializeField] private ConfidenceState confidenceState;
    public Rigidbody2D rb;
    public float speed;
    private Vector2 _moveDirection;
    public InputActionReference move;
    [SerializeField] private AnimationTriggerPlayer animTrigger;
    private bool _wasMoving = false;
    private string _lastPose = "";

    
    void Update()
    {
        if (confidenceState.Dead == false)
        {
            //moves player on x axis
            _moveDirection = move.action.ReadValue<Vector2>();
            bool isMoving = _moveDirection.x != 0;
        
            //animation control
                //if player is pressing a or d, enter walk state
            if (isMoving && !_wasMoving)
                animTrigger.Walk();
            else if (!isMoving && _wasMoving)
            {
                //if player is in conversation mode, enter confdence pose state
                if (confidenceState.inConversation)
                    animator.Play(GetConfidencePose());
                else
                    animTrigger.Default();
            }
            else if (!isMoving && confidenceState.inConversation)
            {
                string pose = GetConfidencePose();
                if (pose != _lastPose)
                {
                    animator.Play(pose);
                    _lastPose = pose;
                }
            }
    
            _wasMoving = isMoving;
        }
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void FixedUpdate()
    {
        if (confidenceState.Dead == false)
        {
            rb.linearVelocity = new Vector2(_moveDirection.x * speed,0);
        }
    }
}
