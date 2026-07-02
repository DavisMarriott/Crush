using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// Test mode - press T from anywhere (hallway, reflect, draft, mid-convo) to hard-restart
// straight into the conversation with EVERY card in the deck. One-way: once it's on, the
// run's progression is toast (full deck), so there's no toggle off - restart play mode.
//
// While active, death skips reflect/draft/hallway entirely: death screen -> right back
// to the convo trigger, Daisy intro and all. [1st]/[2nd] slot continuity still works
// because those counters live in GameProgression and never reset on death.
//
// Also spawns simple conf/charm adjuster buttons on the screen edges (left = confidence,
// right = charm), volume-remote style. All UI is generated in code - nothing to wire.
//
// Same setup as SkipReflect - lives on the Systems prefab (TestModeManager object).
public class TestMode : MonoBehaviour
{
    public static bool Active { get; private set; }

    private DeathRespawn deathRespawn;
    private DeckManager deckManager;
    private ConfidenceState confidenceState;
    private CharmState charmState;
    private GameProgression gameProgression;
    private PlayerMovement playerMovement;
    private AnimationTriggerCrush animationTriggerCrush;

    // runtime-generated UI
    private TMP_Text confValue;
    private TMP_Text charmValue;

    void Awake()
    {
        // found at runtime so no prefab re-wiring needed (same pattern as DeckManager)
        deathRespawn = FindFirstObjectByType<DeathRespawn>();
        deckManager = FindFirstObjectByType<DeckManager>();
        confidenceState = FindFirstObjectByType<ConfidenceState>();
        charmState = FindFirstObjectByType<CharmState>();
        gameProgression = FindFirstObjectByType<GameProgression>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        animationTriggerCrush = FindFirstObjectByType<AnimationTriggerCrush>();

        // statics survive play sessions when domain reload is off - start clean
        Active = false;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (!Active && kb != null && kb.tKey.wasPressedThisFrame)
            TryActivate();

        // keep the edge labels live (buttons aren't the only thing that moves these values)
        if (Active)
        {
            if (confValue != null) confValue.text = confidenceState.confidence.ToString();
            if (charmValue != null) charmValue.text = charmState.charm.ToString();
        }
    }

    private void TryActivate()
    {
        // works from any phase (it's a hard restart) - only blocked while the game's paused
        // (main menu, timeScale 0) so T can't fire under the start screen
        if (deathRespawn == null || Time.timeScale <= 0f) return;

        Active = true;
        Debug.Log("[TestMode] ON - full deck, convo-only loop. One-way; restart play mode to exit.");

        // UI first so it shows even if something below hiccups
        BuildUi();
        Debug.Log("[TestMode] UI built");

        // skip past loop 1 - the convo has special first-loop logic we don't want to test against.
        // Bump BEFORE dealing so the opening-hand rules (no DANCE in hand loop 2+) apply.
        if (gameProgression.loopCount < 2)
            gameProgression.NextLoop();

        // full deck (progress-gated included, upgrades not applied)
        deckManager.GiveAllCards();
        Debug.Log($"[TestMode] deck filled - {deckManager.Deck.Count + deckManager.Hand.Count} cards owned, loop {gameProgression.loopCount}");

        // hard restart into the conversation (kills whatever phase is mid-flight)
        deathRespawn.TestModeRestartToConversation();
    }

    // ---------- generated UI ----------

    private void BuildUi()
    {
        var canvasGO = new GameObject("TestModeUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;   // above everything, incl. death screen
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // pose refreshes ride along so the characters visibly react to the new values -
        // same calls the real convo entry makes (PlayerMovement.InConversation)
        confValue = BuildAdjuster(canvas.transform, "CONF", new Vector2(0f, 0.5f), new Vector2(70, 0),
            () => { confidenceState.confidence += 1; playerMovement.GetConfidencePose(); },
            () => { confidenceState.confidence -= 1; playerMovement.GetConfidencePose(); });
        charmValue = BuildAdjuster(canvas.transform, "CHARM", new Vector2(1f, 0.5f), new Vector2(-70, 0),
            () => { charmState.charm += 1; animationTriggerCrush.GetCharmPose(); },
            () => { charmState.charm -= 1; animationTriggerCrush.GetCharmPose(); });

        var modeLabel = MakeText(canvas.transform, "TEST MODE", 26);
        var modeRt = modeLabel.rectTransform;
        modeRt.anchorMin = modeRt.anchorMax = new Vector2(0.5f, 1f);
        modeRt.anchoredPosition = new Vector2(0, -30);
        modeLabel.color = new Color(1f, 0.55f, 0.55f);
    }

    // one edge column: [+] value [-] with a small title under it
    private TMP_Text BuildAdjuster(Transform parent, string title, Vector2 anchor, Vector2 offset,
        System.Action up, System.Action down)
    {
        var root = new GameObject(title + "Adjuster", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rt = (RectTransform)root.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(80, 300);

        MakeButton(rt, "+", new Vector2(0, 100), up);
        var value = MakeText(rt, "0", 38);
        MakeButton(rt, "-", new Vector2(0, -100), down);

        var label = MakeText(rt, title, 18);
        label.rectTransform.anchoredPosition = new Vector2(0, -160);
        label.color = new Color(1f, 1f, 1f, 0.7f);
        return value;
    }

    private void MakeButton(Transform parent, string glyph, Vector2 pos, System.Action onClick)
    {
        var go = new GameObject("Btn" + glyph, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(70, 70);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        go.GetComponent<Button>().onClick.AddListener(() => onClick());
        MakeText(go.transform, glyph, 42);
    }

    private TMP_Text MakeText(Transform parent, string content, float size)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;   // don't eat the button clicks
        ((RectTransform)go.transform).sizeDelta = new Vector2(200, 60);
        return t;
    }
}
