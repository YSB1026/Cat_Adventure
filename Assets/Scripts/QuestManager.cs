using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour
{
    #region Variable
    //[HideInInspector]//Inspector창에 띄우지 않음.
    /*curQuestId : 현재 진행중인 quest ID, 10번 퀘스트부터 시작
    curQuestSubIdx : 현재 진행중인 quest의 서브 id(QuestData 클래스의 npcIds 배열의 idx.)
    */
    public int curQuestId = 10, curQuestSubIdx = 0;
    public GameObject[] questObject;//현재 진행중인 quest의 목표

    //npc 대화의 흐름이라든지 quest의 흐름을 위해 만든 변수
    //quest1에 (0,1,2,3)의 흐름이 있다면 0,1,2,3 순으로 진행. 0을 무시하고 1을 진행할수없음.
    Dictionary<int, QuestData> questList;
    #endregion

    void Awake()
    {
        questList = new Dictionary<int, QuestData>();
        GenerateData();
    }

    void GenerateData(){
        //QuestData
        //_questName : 퀘스트 이름
        //npcIds : 해당 퀘스트와 연관된 npc의 ID

        //quest id, QuestData("퀘스트 이름", 해당 퀘스트와 연관된 NPC id)
        questList.Add(10, new QuestData(_questName : "할아버지와 대화", _npcIds : new int[] { 100, 1000 }));
        questList.Add(20,new QuestData(_questName : "할아버지에게 나비 선물", _npcIds : new int[] { 5000, 1000 }));
        questList.Add(30,new QuestData(_questName : "집사에게 줄 선물 (1)", _npcIds : new int[] { 6000  }));
        questList.Add(40,new QuestData(_questName : "[BOSS] 집사에게 줄 선물 (2)", _npcIds : new int[] { 300, 7000 }));
        questList.Add(50,new QuestData(_questName : "퀘스트 완료", _npcIds : new int[] { -1, -1 }));
    }

    public int GetQuestDialogueIdx(int npcId){
        return curQuestId + curQuestSubIdx;
    }
    public string CheckQuest(int npcId){//플레이어가 퀘스트를 수행한 경우 호출
        //현재 진행중인 퀘스트 흐름.
        //subIdx를 증가시키면서 퀘스트 대화를 이어감.
        if(npcId == questList[curQuestId].npcIds[curQuestSubIdx])
            curQuestSubIdx++;

        ControlObject();

        if(curQuestSubIdx == questList[curQuestId].npcIds.Length){
            NextQuest();
        }

        return questList[curQuestId].questName;
    }

    public string CheckQuest(){//플레이어가 퀘스트를 수행한 경우 호출
        return questList[curQuestId].questName;
    }

    void NextQuest(){
        curQuestId += 10;
        curQuestSubIdx = 0;
    }

    void ControlObject(){
        /*
        0 : portal 0(main-f1), 1 : butterfly
        2 : portal 3(main-f2), 3: rat
        4: portal 5(f2-f3) 5: snake
        6 : Portal 9(boss-main), 7: portal10(main-boss)
        */
        switch(curQuestId){
        case 10://quest1
            if(curQuestSubIdx==2) questObject[0].SetActive(true);
            break;
        case 20://quest2
            Butterfly butterfly = questObject[1].GetComponent<Butterfly>();
            if(butterfly == null) return;
            else if(curQuestSubIdx==1 && butterfly.currentHealth<=0){
                butterfly.corpsePrefab.SetActive(false);
            }
            else if(curQuestSubIdx==2 && butterfly.corpsePrefab != null){
                butterfly.Remove();
                questObject[2].SetActive(true);
            }
            break;
        case 30:
            Rat rat = questObject[3].GetComponent<Rat>();
            if(rat == null) return;
            else if(curQuestSubIdx==1 && rat.currentHealth<=0){
                rat.Remove();
                questObject[4].SetActive(true);
            }
            break;
        case 40:
            Snake snake = questObject[5].GetComponent<Snake>();
            if(snake == null) return;
            if(curQuestSubIdx==2 && snake.currentHealth<=0){
                snake.Remove();
                questObject[6].SetActive(true);
                questObject[7].SetActive(true);
            }
            break;
        }
    }
}
