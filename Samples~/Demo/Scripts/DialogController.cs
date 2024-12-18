using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KulibinSpace.DialogSystem {

    public class DialogController : MonoBehaviour {

        public DialogNodeGraph dialog;

        public GameObject sentencePanel;
        public TextMeshProUGUI characterName;
        public TextMeshProUGUI sentenceText;
        public Transform answersPanel;
        public GameObject answerLinePrefab;

        DialogNodeRunner runner;

        void Start () {
            runner = new();
            runner.Init(dialog);
            NextSentence();
        }

        public void NextSentence () {
            if (runner.node != null) {
                if (runner.node is SentenceNode snode) {
                    sentenceText.text = snode.GetSentenceText();
                    characterName.text = snode.GetSentenceCharacterName();
                    while (answersPanel.childCount > 0) DestroyImmediate(answersPanel.GetChild(0).gameObject);
                } else if (runner.node is AnswerNode anode) {
                    foreach (string answer in anode.answers) {
                        GameObject clone = Instantiate(answerLinePrefab, answersPanel);
                        clone.GetComponent<AnswerLine>().SetAnswer(answer);
                    }
                    // to select first answer
                    if (answersPanel.childCount > 0) {
                        //answersPanel.GetChild(0).GetComponent<AnswerLine>().answerButton.GetComponent<Image>().enabled = true;
                    }
                }
                runner.Next();
            }
        }

    }

}

