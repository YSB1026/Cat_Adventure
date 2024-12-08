using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{   
    Dictionary<int,string[]>dialogueData;
    void Awake() {
        dialogueData= new Dictionary<int,string[]>();
        GenerateData();
    }
    void GenerateData(){
        //Talk Dialogue
        //이웃집 할아버지 : 1000
        //(sign) 집사네 집 표지판 : 100, 이웃집 할아버지 표지판 : 200, 보스 표지판 : 300
        //나비 시체 : 5000, 쥐 시체 : 6000, 뱀 시체 : 7000
       
        dialogueData.Add(1000, new string[] {"구름아 안녕?"});
        dialogueData.Add(100, new string[] {"(집사의 집)"});
        dialogueData.Add(200, new string[] {"(이웃 할아버지 집)"});

        //Quest Dialogue
        //quest id : 10번부터시작, 10씩 증가
        //quest sub idx 1씩 증가
        //quest idx + quest sub idx + npc id
  
        //QUEST1
        dialogueData.Add(10 + 0 + 100, new string[] {"(집사는 오늘도 일하러 갔네)","(오늘도 할아버지에게 츄르를 얻어먹으러 가야겠다.)"});

        dialogueData.Add(10 + 1 + 1000, new string[] {"안녕 구름아, 오늘도 왔구나!",
                                                "츄르 먹고 싶니?","허허, 잘 먹는구나",
                                                "(할아버지에게 츄르를 얻어먹었으니 선물을 드리자)",
                                                "[Q. 오른쪽 길을 따라가 파란색 나비를 사냥하여 할아버지에게 선물로 주기]"});

        //QUEST2
        dialogueData.Add(20 + 0 + 1000, new string[] {"(할아버지에게 츄르를 얻어먹었으니 선물을 드리자)",
                                                    "[Q. 오른쪽 길을 따라가 파란색 나비를 사냥하여 할아버지에게 선물로 주기]"});

        dialogueData.Add(20 + 0 + 5000, new string[] {"(할아버지에게 파란 나비를 선물로 드리자)"});

        dialogueData.Add(20 + 1 + 1000, new string[] {
            "오호, 구름아, 이걸 나에게 주는 거니?",
            "정말 예쁜 파란 나비구나! 고맙다, 구름아.",
            "(할아버지는 웃으며 구름이를 쓰다듬었다.)",
            "[Q. 아래쪽 길을 따라가 집사에게 줄 선물을 사냥해오기]"
        });
        //QUEST3
        dialogueData.Add(30 + 0 + 6000, new string[] {
            "(할아버지에게 선물도 드리고 정말 뿌듯하다.)",
            "(집사 선물도 챙겼으니, 이제 조금 더 숲을 돌아다니며 \n놀다가 집으로 돌아가야지.)",
            "[Q. 왼쪽 길을 따라 숲속을 더 탐험하기]"
        });

        //QUEST4
        dialogueData.Add(40 + 0 + 300, new string[] {
            "<주의! 위험 지역: 뱀이 출몰합니다>",
            "긴장하고 뱀의 투사체와 꼬리 공격에 대비하세요."
        });
        dialogueData.Add(40 + 1 + 7000, new string[] {
            "(뱀을 물리치느라 정말 힘들었지만, 해냈다!)",
            "(이제는 집사에게 돌아갈 시간이다. 집으로 가서 푹 쉬어야겠다.)",
            "[Q. 맵의 오른쪽으로 이동해 집으로 돌아가기]"
        });
        //완료
        dialogueData.Add(50 + 0 + 101, new string[] {
            "(집으로 무사히 돌아왔다.)",
            "(뱀과의 전투는 정말 힘들었지만, 덕분에 오늘 하루 큰 모험을 했다.)",
            "(이제 푹 쉬면서 집사를 기다려야지. 집사가 돌아오면 자랑해야겠다!)"
        });

    }
    public string GetDialogue(int id, int talkIndex){
        if(!dialogueData.ContainsKey(id)){
            if(!dialogueData.ContainsKey(id-id%10)){
               return GetDialogue(id-id%100, talkIndex);//퀘스트 아닐때 첫 대화
            }
            else{
                return GetDialogue(id-id%10, talkIndex);//퀘스트 첫 대화
            }
        }

        if(talkIndex==dialogueData[id].Length) return null;
        return dialogueData[id][talkIndex];
    }
}
