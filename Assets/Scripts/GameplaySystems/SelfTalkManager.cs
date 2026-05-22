using UnityEngine;

public class SelfTalkManager : MonoBehaviour
{
    public GameObject reflectSelfTalk;
    public GameObject reflectText;


    public void EnableSelfTalk()
    {
        reflectSelfTalk.SetActive(true);
        reflectText.SetActive(true);
    }
    
    public void DisableSelfTalk()
    {
        reflectText.SetActive(false);
        reflectSelfTalk.SetActive(false);
    }
    
}
