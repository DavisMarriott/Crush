using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeIconReveal : MonoBehaviour
{
    [SerializeField] private Transform iconParent;
    //short lock so a leftover click from the previous beat can't instantly activate it
    [SerializeField] private float clickHold = 2f;
    //time for the prefab's own activate animation to play before we despawn
    [SerializeField] private float despawnDelay = 1f;

    public IEnumerator Play(CharacterUpgrade upgrade)
    {
        if (upgrade == null || upgrade.iconPrefab == null) yield break;

        Transform parent = iconParent != null ? iconParent : transform;
        Button icon = Instantiate(upgrade.iconPrefab, parent);

        // reuse the draft card's gate (Button + EventTrigger). Locked during the hold so a stray click is ignored.
        var controller = icon.GetComponentInChildren<ButtonController>();
        if (controller != null) controller.DisableButton();
        else icon.interactable = false;

        yield return new WaitForSeconds(clickHold);

        if (controller != null) controller.EnableButton();
        else icon.interactable = true;

        // click the card itself to activate - same onClick the draft uses to pick a card
        bool activated = false;
        icon.onClick.AddListener(() => activated = true);
        yield return new WaitUntil(() => activated);

        // the prefab fires its own activate animation on click; give it a beat before despawn
        yield return new WaitForSeconds(despawnDelay);

        Destroy(icon.gameObject);
    }
}
