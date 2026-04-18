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
    
    private int _charm;
    public int peakCharm;
    public int charm
    {
        get => _charm;
        set
        {
            _charm = Mathf.Clamp(value, MinCharm, MaxCharm);
            if (_charm > peakCharm) peakCharm = _charm;
        }
    }

    void Start()
    {
        charm = startingCharm;
    }

    void Update()
    {
        if (label != null)
            label.text = $"{charm}";
        
        // you die if charm hits 0
        // todo: build branches/logic for this
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