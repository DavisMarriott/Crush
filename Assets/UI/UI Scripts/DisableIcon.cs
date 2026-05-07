using UnityEngine;

public class DisableIcon : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Disable()
    {
        transform.Find("Icon_Art").gameObject.SetActive(false);
    }
}
