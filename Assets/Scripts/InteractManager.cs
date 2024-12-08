using UnityEngine;
using TMPro;
public class InteractManager  : MonoBehaviour
{
    #region Variable
    #region public
    [HideInInspector]//Inspector창에 띄우지 않음.
    public bool isTalk = false;

    [Header("UI 설정")]
    public GameObject talkPanel;
    public TMP_Text TextUI;

    [Header("대사 매니저")]
    public DialogueManager dialogueManager;

    [Header("퀘스트 매니저")]
    public QuestManager questManager;

    #endregion

    #region private
    private GameObject scanObj;
    private int talkIndex;
    #endregion
    #endregion

    void Start() {
        Debug.Log(questManager.CheckQuest());
    }

    public void HandleInteraction(GameObject _scanObj){//_scanObj는 not null, player script에서 null인경우 호출 안함
        scanObj = _scanObj;
        ObjectData objData = scanObj.GetComponent<ObjectData>();
        
        Talk(objData.id, objData.isNpc);
        talkPanel.SetActive(isTalk);
    }
    void Talk(int id, bool isNpc){
        int questDialogueIdx = questManager.GetQuestDialogueIdx(npcId:id);
        string dialogueData = dialogueManager.GetDialogue(id + questDialogueIdx, talkIndex++);
        isTalk = dialogueData != null ? true : false;
        
        if(!isTalk){//대화가 끝난경우
            talkIndex = 0;
            Debug.Log(questManager.CheckQuest(id));
            return;
        }

        if(isNpc){
            TextUI.text = dialogueData;
        }else{
            TextUI.text = dialogueData;
        }
    }
}
