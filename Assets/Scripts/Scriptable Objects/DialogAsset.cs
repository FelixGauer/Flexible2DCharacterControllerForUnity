using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogAsset", menuName = "Scriptable Objects/DialogAsset")]
public class DialogAsset : ScriptableObject
{

    public string[] text_string;
    public DialogAsset nextAsset;

}
