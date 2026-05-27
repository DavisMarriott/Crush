using UnityEngine;

public class DisableObject : MonoBehaviour
{
    public void DisableThisObject()
    {
        gameObject.SetActive(false);
    }
}
