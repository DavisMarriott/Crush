using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConfidenceIncrementer : MonoBehaviour

{

    public int confidence = 4;
    
    private TextMeshProUGUI textMeshPro;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = $"Confidence: {confidence.ToString()} ";
    }

    public void IncrementConfidenceUp()
    {
        if (confidence < 6)
        {
            confidence++;
        }
    }
    
    public void IncrementConfidenceDown()
    {
        if (confidence > 0)
        {
            confidence--;
        }
        
    }
    
}
