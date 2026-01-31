using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Object")]
public class DialogueObject : ScriptableObject
{
   [SerializeField] [TextArea] private string[] dialogue;
   
   //get only
   public string[] Dialogue => dialogue;
}
