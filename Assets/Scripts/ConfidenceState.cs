using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ConfidenceState : MonoBehaviour
{
    [SerializeField] private Transform boyTransform;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private TMP_Text label;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    
    //confidence is in thirds, 45 = 15 full hearts
    const int MinConfidenceFullGame = 0;
    const int MaxConfidenceFullGame = 45;

    
    [Header("Starting Confidence (in thirds)")]
    [SerializeField] private int startingConfidence = 3;

    public int confidence;

    void Start()
    {
    confidence = startingConfidence;
    }
    
    
    
    
    private void Update()
    {
       //displays confidence (in thirds)
        if (label != null)
            label.text = $"{confidence}";
        //applies clamp to confidence score. Todo: change to only clamp on score change
        ClampConfidence();
        label.text = $"{confidence}";

        if (confidence <= 0)
        {
            boyTransform.position = spawnPoint.position;
            confidence = startingConfidence;
            dialogueBox.CloseDialogueBox();
            thoughtSpawner.SpawnButtons();
        }
    }

    void ClampConfidence()
    {
        confidence = Mathf.Clamp(confidence, MinConfidenceFullGame, MaxConfidenceFullGame);
    }
}
