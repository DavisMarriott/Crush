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
        
        confidenceHeartMeter.SpawnHeartMeter();
    }

    public void TransitionTo(GamePhase newPhase)
    {
        if (newPhase == currentPhase)  return;
        GamePhase oldPhase = currentPhase;
        currentPhase = newPhase;
        Debug.Log($"[phaseManager] {oldPhase} -> {newPhase}");
        OnPhaseChanged?.Invoke(oldPhase, newPhase);

        if (currentPhase == GamePhase.Hallway)
        {
            animationTriggerCrush.Begin();
            animationTriggerCrush.ParticlesCharmedStateTurnOff();
        }
            

    }
}
