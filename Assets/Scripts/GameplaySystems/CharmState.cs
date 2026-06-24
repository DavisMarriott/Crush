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

        // zero-charm fail is written into the dialogue cards now (low/zero-charm branches drain conf).
        // ripped out the old auto "charm 0 -> conf 0 -> die" hack. confidenceState kept for a possible fallback later.
    }

    public void ResetCharm()
    {
        charm = startingCharm;
    }
}