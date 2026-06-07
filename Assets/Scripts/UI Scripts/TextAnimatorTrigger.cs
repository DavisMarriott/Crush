using UnityEngine;
using TMPro;
using Febucci.TextAnimatorForUnity.TextMeshPro;

public class TextAnimatorTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text textMeshPro;
    [SerializeField] Febucci.TextAnimatorForUnity.TypewriterComponent textAnimator;
    
    public void GenerateText()
    {
        // textAnimator.SetText($"{textMeshPro.text}");
        string textValue = textMeshPro.text;
        textAnimator.ShowText(textValue);    }
}
