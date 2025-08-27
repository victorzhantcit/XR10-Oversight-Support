using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeywordConfig", menuName = "Settings/Keyword Config")]
public class KeywordConfig : ScriptableObject
{
    public string keyword = "DefaultKeyword";
    public List<string> matchedObjects = new List<string>();
}