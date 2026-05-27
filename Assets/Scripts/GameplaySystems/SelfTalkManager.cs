using UnityEngine;

public class SelfTalkManager : MonoBehaviour
{
    public GameObject reflectSelfTalk;
    public GameObject reflectText;


    // True once the bubble's rise has fired EnableSelfTalk (the visual reveal), false once it closes.
    // ReflectSelfTalk waits on this before typing so reflect lines don't type before the bubble's open.
    public bool SelfTalkRevealed { get; private set; }

    public void EnableSelfTalk()
    {
        reflectSelfTalk.SetActive(true);
        reflectText.SetActive(true);
        SelfTalkRevealed = true;
    }

    public void DisableSelfTalk()
    {
        reflectText.SetActive(false);
        reflectSelfTalk.SetActive(false);
        SelfTalkRevealed = false;
    }
    
}
