using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

namespace KulibinSpace.DialogSystem {

    public class DialogController : MonoBehaviour {

        public DialogNodeGraph dialog;

        public GameObject sentencePanel;
        public TextMeshProUGUI characterName;
        public TextMeshProUGUI sentenceText;
        public Transform answersPanel;
        public GameObject answerLinePrefab;
        public float sentenceCharsOutputDelay = 0.02f;

        DialogNodeRunner runner;

        void Start () {
            runner = new DialogNodeRunner();
            runner.Init(dialog);
            NextSentence();
        }

        // skip works in three issues:
        // 1) to stop output current sentence, then show answers and wait answer selection
        // 2) to stop output current sentence and wait next skip action
        // 3) go to next Sentence Node and start output its text
        public void OnSkipSentence (InputAction.CallbackContext context) {
            if (context.performed) {
                StopAllCoroutines();
                sentenceText.maxVisibleCharacters = sentenceText.text.Length;
                if (runner.node != null && runner.node is SentenceNode) {
                    StartCoroutine(FillAnswer(runner.Next()));
                }
            }
        }

        void FillSentence (SentenceNode snode) {
            sentenceText.text = snode.GetSentenceText();
            sentenceText.maxVisibleCharacters = 0;
            characterName.text = snode.GetSentenceCharacterName();
            StopAllCoroutines();
            StartCoroutine(SentenceOutputLikeTerminal());
        }

        IEnumerator SentenceOutputLikeTerminal () {
            while (sentenceText.maxVisibleCharacters < sentenceText.text.Length) {
                sentenceText.maxVisibleCharacters += 1;
                yield return new WaitForSeconds(sentenceCharsOutputDelay);
            }
            if (runner.node != null && runner.node is SentenceNode) {
                yield return new WaitForSeconds(0.2f); // some natural like delay
                StartCoroutine(FillAnswer(runner.Next()));
            }
        }

        void ClearAnswer () {
            while (answersPanel.childCount > 0) DestroyImmediate(answersPanel.GetChild(0).gameObject);
        }

        IEnumerator FillAnswer (Node par) {
            if (par != null && par is AnswerNode anode) {
                int si = 0;
                foreach (string answer in anode.answers) {
                    GameObject clone = Instantiate(answerLinePrefab, answersPanel);
                    clone.GetComponent<AnswerLine>().SetAnswer(answer, anode.childSentenceNodes[si], GoToSentence);
                    yield return new WaitForSeconds(0.05f); // presentation like delay
                    si++;
                }
            }
        }

        // The sentence node is ALWAYS after the answer node
        // After the sentence node there is either an answer node or a sentence
        // We need to know two consequent nodes at a time, to get a pair sentence-answer
        public void NextSentence () {
            ClearAnswer();
            if (runner.node != null && runner.node is SentenceNode snode) {
                FillSentence(snode);
            }
        }

        public void GoToSentence (SentenceNode par) {
            runner.node = par;
            NextSentence();
        }

    }

}

