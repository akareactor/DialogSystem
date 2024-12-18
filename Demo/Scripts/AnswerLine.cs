using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KulibinSpace.DialogSystem {

    public class AnswerLine : MonoBehaviour {

        public TextMeshProUGUI answerText;
        public Button answerButton;

        public void SetAnswer (string answer) {
            answerText.text = answer;
        }

    }

}
