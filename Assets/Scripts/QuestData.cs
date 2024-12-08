using UnityEngine;

public class QuestData
{
    public string questName;
    public int[] npcIds;

    public QuestData(string _questName, int[] _npcIds){
        questName = _questName;
        npcIds = _npcIds;
    }
}
