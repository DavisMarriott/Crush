using System;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    [SerializeField] private GamePhase currentPhase = GamePhase.Hallway;
    public GamePhase CurrentPhase => currentPhase;
    public static PhaseManager Instance { get; private set; }
    public event Action<GamePhase, GamePhase> OnPhaseChanged;

    public ConfidenceHeartMeter confidenceHeartMeter;
    public AnimationTriggerCrush animationTriggerCrush;
    public AnimationTriggerPlayer animationTriggerPlayerDraft;
    public GameProgression gameProgression;
    public AnimationTriggerThoughtBubble animationTriggerThoughtBubbleHallway;
    public AnimationTriggerThoughtBubble animationTriggerThoughtBubbleDraft;
    public AudioSource thoughtBubbleHallwayAudio;
    public AudioSource postFxAudio;
    public GameObject postProcessingVolumeNegative;
    public GameObject iconDeckSize;
    public GameObject iconConfidentWalk;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        //only possible to get here IF ^^ ==false
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    public void TransitionTo(GamePhase newPhase)
    {
        if (newPhase == currentPhase)  return;
        GamePhase oldPhase = currentPhase;
        currentPhase = newPhase;
        Debug.Log($"[phaseManager] {oldPhase} -> {newPhase}");
        OnPhaseChanged?.Invoke(oldPhase, newPhase);

        int loopCount = gameProgression.loopCount;
        
        if (currentPhase == GamePhase.Hallway)
        {
            animationTriggerCrush.Begin();
            animationTriggerCrush.ParticlesCharmedStateTurnOff();
            thoughtBubbleHallwayAudio.enabled = true;
            postFxAudio.enabled = true;
            postProcessingVolumeNegative.SetActive(true);
        }
        
        
        if (currentPhase == GamePhase.Reflect && loopCount <= 1)
        {
            animationTriggerPlayerDraft.LockerOpen();
            // animationTriggerThoughtBubbleDraft.ThoughtBubbleOff();
            thoughtBubbleHallwayAudio.enabled = false;
            postFxAudio.enabled = false;
            postProcessingVolumeNegative.SetActive(false);
        }
        
        if (currentPhase == GamePhase.Reflect && loopCount > 1)
        {
            animationTriggerPlayerDraft.LockerWake();
            // animationTriggerThoughtBubbleDraft.ThoughtBubbleOff();
            thoughtBubbleHallwayAudio.enabled = false;
            postFxAudio.enabled = false;
            postProcessingVolumeNegative.SetActive(false);
        }
        
        if (currentPhase == GamePhase.UpgradeDraft)
        {
            iconDeckSize.SetActive(true);
            thoughtBubbleHallwayAudio.enabled = false;
            postFxAudio.enabled = false;
            postProcessingVolumeNegative.SetActive(false);
            if (gameProgression.loopCount > 1)
            {
                iconConfidentWalk.SetActive(true);
            }
            else{
                iconConfidentWalk.SetActive(false);}
        }

        if (currentPhase == GamePhase.Death)
        {
            iconDeckSize.SetActive(false);
            iconConfidentWalk.SetActive(false);
            // clear any leftover hearts so they don't carry into the next loop (respawn re-adds from 0)
            confidenceHeartMeter.ClearAllHearts();
        }
        
        
    }

}
