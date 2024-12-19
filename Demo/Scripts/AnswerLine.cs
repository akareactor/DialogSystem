using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

namespace KulibinSpace.DialogSystem {

    public class AnswerLine : MonoBehaviour {

        public TextMeshProUGUI answerText;
        public Button answerButton;

        public void SetAnswer (string answer, SentenceNode snode, UnityAction<SentenceNode> action) {
            answerText.text = answer;
            answerButton.onClick.RemoveAllListeners();
            answerButton.onClick.AddListener(() => action.Invoke(snode));
        }

    }

}
