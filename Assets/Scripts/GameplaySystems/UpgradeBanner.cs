using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeBanner : MonoBehaviour
{
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private float holdTime = 2f;

    private void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
    }

    public void Show(string text)
    {
        if (bannerText != null) bannerText.text = text;
        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (canvasGroup == null) yield break;

        // fade in
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdTime);

        // fade out
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
