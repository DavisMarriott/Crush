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
    [SerializeField] private ConfidenceHeartMeter heartMeter;
    

    [Header("Conversation State")]
    public bool introMade = false;
    public bool inConversation = false;
    

    // confidence is in thirds, 45 = 15 full hearts
    const int MinConfidenceFullGame = 0;
    const int MaxConfidenceFullGame = 45;

    [Header("Starting Confidence")]
    [SerializeField] public int startingConfidence = 3;

    private int _confidence;
    public int peakConfidence;
    public int confidence
    {
        get => _confidence;
        set
        {
            int oldConfidence = _confidence;
            _confidence = Mathf.Clamp(value, MinConfidenceFullGame, MaxConfidenceFullGame);
            if (_confidence > peakConfidence) peakConfidence = _confidence;

            int delta = _confidence - oldConfidence;
            if (delta > 0) heartMeter.AddHearts(delta);
            else if (delta < 0) heartMeter.RemoveHearts(-delta);
        }
    }


    void Start()
    {
        _confidence = startingConfidence;
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
