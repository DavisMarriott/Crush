using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ConfidenceState : MonoBehaviour
{
    [SerializeField] private Transform boyTransform;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private TMP_Text label;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private CharmState charmState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameProgression gameProgression;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
    

    [Header("Conversation State")]
    public bool introMade = false;
    public bool inConversation = false;
    

    // confidence is in thirds, 45 = 15 full hearts
    const int MinConfidenceFullGame = 0;
    const int MaxConfidenceFullGame = 45;

    [Header("Starting Confidence")]
    [SerializeField] public int startingConfidence = 3;

    public int confidence;

    void Start()
    {
        confidence = startingConfidence;
    }

    private void Update()
    {
        if (label != null)
            label.text = $"{confidence}";

        // todo: only clamp when score actually changes
        ClampConfidence();
        label.text = $"{confidence}";
        
        
        
    }
    
    void ClampConfidence()
    {
        confidence = Mathf.Clamp(confidence, MinConfidenceFullGame, MaxConfidenceFullGame);
    }

    public void EnterConversation()
    {
        inConversation = true;
    }

    public void ExitConversation()
    {
        inConversation = false;
    }

    
}
