using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Crush/Dialogue Card")]
public class DialogueCard : ScriptableObject
{
    public bool isDance = false;
    public bool revealed = false;
    [Header("Display text on bubble")]
    public string previewText;

    [Header("Card Cost")]
    public int cost = 1;

    [Header("Button BG Color")]
    public Color buttonColor = Color.black;

    [Header("Luke's Branches (confidence-based)")]
    [SerializeField] private DialogueBranch[] lukeBranches;

    public DialogueBranch[] LukeBranches => lukeBranches;
    public DraftLine[] draftLines;

    public DialogueCardUpgrade[] availableUpgrades;
    public int upgradeThreshold = 3;

    public enum DialogueCharacter
    {
        Boy,
        Girl,
        BoyInternal
    }

    public enum CharmState
    {
        Death,      // 0
        Low,        // 1-2
        Neutral,    // 3-5
        Positive,   // 6-8
        High        // 9-10
    }

    public enum ConfidenceLevel
    {
        Low,        // 1-3
        Neutral,    // 4-6
        Positive,   // 7-9
        High        // 10+
    }


    public static void GetCharmRange(CharmState state, out int min, out int max)
    {
        switch (state)
        {
            case CharmState.Death:    min = 0;  max = 0;  break;
            case CharmState.Low:      min = 1;  max = 2;  break;
            case CharmState.Neutral:  min = 3;  max = 5;  break;
            case CharmState.Positive: min = 6;  max = 8;  break;
            case CharmState.High:     min = 9;  max = 10; break;
            default:                  min = 0;  max = 10; break;
        }
    }

    public static void GetConfidenceRange(ConfidenceLevel level, out int min, out int max)
    {
        switch (level)
        {
            case ConfidenceLevel.Low:      min = 1;  max = 3;  break;
            case ConfidenceLevel.Neutral:   min = 4;  max = 6;  break;
            case ConfidenceLevel.Positive:  min = 7;  max = 9;  break;
            case ConfidenceLevel.High:      min = 10; max = 45; break;
            default:                        min = 0;  max = 45; break;
        }
    }

    public static ConfidenceLevel GetConfidenceLevel(int confidence)
    {
        if (confidence <= 0) return ConfidenceLevel.Low;
        if (confidence <= 3) return ConfidenceLevel.Low;
        if (confidence <= 6) return ConfidenceLevel.Neutral;
        if (confidence <= 9) return ConfidenceLevel.Positive;
        return ConfidenceLevel.High;
    }


    [System.Serializable]
    public class CharmImpactEntry
    {
        public CharmState state;
        public int impact;
    }

    // separate class from DialogueBranch so Unity's serializer doesn't hit a
    // recursive depth limit (DialogueBranch used to contain DialogueBranch[])
    [System.Serializable]
    public class DaisyBranch
    {
        public CharmState charmState;
        public DialogueLine[] dialogue = new DialogueLine[0];

        [Header("Tags fired when this Daisy branch plays")]
        public DialogueTag[] tags;
    }
    
    [System.Serializable]
    public class DialogueBranch
    {
        public string branchName;
        public DialogueLine[] dialogue = new DialogueLine[0];

        [Header("Tags fired when this Luke branch plays")]
        public DialogueTag[] tags;

        [Header("Charm Impact Per State (for this branch)")]
        public CharmImpactEntry[] charmImpacts;

        [Header("Daisy's Response (charm-based, for this branch)")]
        public DaisyBranch[] daisyBranches;

        public int GetCharmImpact(CharmState currentState)
        {
            if (charmImpacts == null) return 0;
            for (int i = 0; i < charmImpacts.Length; i++)
            {
                if (charmImpacts[i].state == currentState)
                    return charmImpacts[i].impact;
            }
            return 0;
        }

        // find the daisy branch that matches the current charm score
        public DaisyBranch GetDaisyBranch(int charm)
        {
            if (daisyBranches == null || daisyBranches.Length == 0)
                return null;

            for (int i = 0; i < daisyBranches.Length; i++)
            {
                var b = daisyBranches[i];
                GetCharmRange(b.charmState, out int min, out int max);

                if (charm >= min && charm <= max)
                    return b;
            }
            return null;
        }
    }

    // confidence here is AFTER cost has been deducted by ThoughtSpawner.
    // Convenience overload — no applied upgrade, no loop context.
    public DialogueBranch GetLukeBranch(int confidence)
    {
        return GetLukeBranch(confidence, null, 0);
    }

    // Main branch selector. `upgrade` (if any) folds branch overrides into the card's branches.
    // `loopCount` is unused by the base card but lets subclasses (e.g. DanceCard) special-case by loop.
    // virtual so subclasses can override selection while reusing the effective-branch helpers below.
    public virtual DialogueBranch GetLukeBranch(int confidence, DialogueCardUpgrade upgrade, int loopCount)
    {
        DialogueBranch[] branches = GetEffectiveLukeBranches(upgrade);
        if (branches == null || branches.Length == 0)
            return null;

        // dead
        if (confidence <= 0)
            return FindBranchByName(branches, "Death");

        ConfidenceLevel level = GetConfidenceLevel(confidence);

        // awkward (only if the card has one)
        if (level == ConfidenceLevel.Low)
        {
            DialogueBranch awkward = FindBranchByName(branches, "Awkward");
            if (awkward != null)
                return awkward;
        }

        // normal (or fallback if no awkward)
        DialogueBranch normal = FindBranchByName(branches, "Normal");
        if (normal != null)
            return normal;

        // last resort - first available branch
        return branches[0];
    }

    // Effective Luke-branch set for an applied upgrade: the card's branches with any same-named
    // branch replaced by the upgrade's override. branchAdditions are intentionally ignored for now.
    protected DialogueBranch[] GetEffectiveLukeBranches(DialogueCardUpgrade upgrade)
    {
        if (upgrade == null || upgrade.branchOverrides == null || upgrade.branchOverrides.Length == 0)
            return lukeBranches;

        List<DialogueBranch> result = new List<DialogueBranch>(lukeBranches);
        foreach (DialogueBranch ov in upgrade.branchOverrides)
        {
            if (ov == null) continue;
            int idx = result.FindIndex(b => b.branchName == ov.branchName);
            if (idx >= 0) result[idx] = ov;   // override the same-named branch
            // (no matching name would be an "addition" — on ice for now, so skip)
        }
        return result.ToArray();
    }

    protected DialogueBranch FindBranchByName(DialogueBranch[] branches, string name)
    {
        if (branches == null) return null;
        for (int i = 0; i < branches.Length; i++)
        {
            if (branches[i].branchName == name)
                return branches[i];
        }
        return null;
    }

    
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Character")]
        public DialogueCharacter character;

        [Header("Confidence Impact")]
        public int confidenceImpact;

        [Header("Charm Impact")]
        public int charmImpact;

        [Header("Dialogue Line")]
        [TextArea(3,5)]
        public string line;
    }
    

    [System.Serializable]
    public class DraftLine
    {
        [Header("Dialogue Line")]
        [TextArea(3,5)]
        public string line;
    }
}
