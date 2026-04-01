using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HallwaySelfTalk : MonoBehaviour
{
    [SerializeField] private string[] genericHallwayLines;
    [SerializeField] private TMP_Text selfTalkText;
    [SerializeField] private float minFirstTimer = 1f;
    [SerializeField] private float maxFirstTimer = 4f;
    [SerializeField] private float minSecondTimer = 4f;
    [SerializeField] private float maxSecondTimer = 10f;
    [SerializeField] private float thoughtTimer = 3f;
    private Coroutine hallwayRoutine;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    void Start()
    {
        hallwayRoutine = StartCoroutine(HallwayTimer());
    }
    private IEnumerator HallwayTimer()
    {
        yield return new WaitForSeconds(Random.Range(minFirstTimer, maxFirstTimer));
        selfTalkText.text = genericHallwayLines[Random.Range(0, genericHallwayLines.Length)];
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
        yield return new WaitForSeconds(Random.Range(minSecondTimer, maxSecondTimer));
        selfTalkText.text = genericHallwayLines[Random.Range(0, genericHallwayLines.Length)];
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
    }

    public void EndHallwayTimer()
    {
        if (hallwayRoutine != null)
            StopCoroutine(hallwayRoutine);
        selfTalkText.text = "";
    }
    
}
