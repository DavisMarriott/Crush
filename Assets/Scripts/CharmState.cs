using UnityEngine;
using TMPro;

public class CharmState : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private ConfidenceState confidenceState;
    
    [Header("Charm Settings")]
    [SerializeField] private int startingCharm = 4;
    
    const int MinCharm = 0;
    const int MaxCharm = 10;
    
    public int charm;

    void Start()
    {
        charm = startingCharm;
    }

    void Update()
    {
        charm = Mathf.Clamp(charm, MinCharm, MaxCharm);
        
        if (label != null)
            label.text = $"{charm}";
        
        // you die if death hits 0
        if (charm <= 0)
        {
            confidenceState.confidence = 0;
        }
    }

    public void ResetCharm()
    {
        charm = startingCharm;
    }
}