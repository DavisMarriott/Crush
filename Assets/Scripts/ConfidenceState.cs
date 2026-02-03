using TMPro;
using UnityEngine;

public class ConfidenceState : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    //confidence is in thirds, 45 = 15 full hearts
    const int MinConfidenceFullGame = 0;
    const int MaxConfidenceFullGame = 45;
    
    [Header("Current Confidence (in thirds)")]
    public int confidence = 9;
    
    
    private void Update()
    {
       //displays confidence (in thirds)
        if (label != null)
            label.text = $"{confidence}";
        //applies clamp to confidence score. Todo: change to only clamp on score change
        ClampConfidence();
        label.text = $"{confidence}";
    }

    void ClampConfidence()
    {
        confidence = Mathf.Clamp(confidence, MinConfidenceFullGame, MaxConfidenceFullGame);
    }
}
