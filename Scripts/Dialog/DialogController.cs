using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using TMPro;

namespace KulibinSpace.DialogSystem {

    public class DialogController : MonoBehaviour {

        [Header("Dialog Graph"), Space(5)]
        public DialogNodeGraph dialog;
        [Header("Dialog GUI"), Space(5)]
        public GameObject dialogContainer;
        public TextMeshProUGUI characterName;
        public TextMeshProUGUI sentenceText;
        public Transform answersPanel;
        [SerializeField]
        private GameObject answerLinePrefab;
        [Header("Dialog settings"), Space(5)]
        public float sentenceCharsOutputDelay = 0.02f;
        public UnityAction dialogStopped;

        DialogNodeRunner runner;

        public void LogSentenceSignal (string par) {
            print("Sentence signal: " + par);
        }

        void Start () {
            if (dialog != null) Init(dialog);
        }

        public void Init (DialogNodeGraph par) {
            dialog = par;
            runner = new DialogNodeRunner();
            runner.Init(dialog);
            OutputSentence();
        }

        public void SetAnswerLinePrefab (GameObject par) {
            answerLinePrefab = par;
        }

        void OpenDialog () {
            dialogContainer.SetActive(true);
            // character name
            if (dialog.stringRef.IsEmpty)
                characterName.text = dialog.characterName;
            else
                characterName.text = dialog.stringRef.GetLocalizedString();
            //dialog.stringRef.GetLocalizedStringAsync().Completed += completedHandle => { if (completedHandle.Status == AsyncOperationStatus.Succeeded) результатДействия.Invoke(completedHandle.Result); };
        }

        public void OutputSentence () {
            while (answersPanel.childCount > 0) DestroyImmediate(answersPanel.GetChild(0).gameObject); // clear answer
            if (runner.node != null && runner.node is SentenceNode snode) {
                OpenDialog();
                snode.sentenceSignal?.Invoke();
                sentenceText.text = snode.GetSentenceText();
                sentenceText.maxVisibleCharacters = 0;
                StopAllCoroutines();
                StartCoroutine(SentenceOutputLikeTerminal());
            } else {
                CloseDialog();
            }
        }

        IEnumerator SentenceOutputLikeTerminal () {
            while (sentenceText.maxVisibleCharacters < sentenceText.text.Length) {
                sentenceText.maxVisibleCharacters += 1;
                yield return new WaitForSeconds(sentenceCharsOutputDelay);
            }
            yield return new WaitForSeconds(0.2f); // some natural-like delay
            StartCoroutine(FillAnswer(runner.Next())); // next should be an answer node
        }

        IEnumerator FillAnswer (Node par) {
            if (par != null && par is AnswerNode anode) {
                int si = 0;
                foreach (string answer in anode.Answers) {
                    GameObject clone = Instantiate(answerLinePrefab, answersPanel); // let empty button alive
                    clone.GetComponent<AnswerLine>().SetAnswer(answer, anode.GetChild(si), GoToSentence);
                    yield return new WaitForSeconds(0.05f); // presentation like delay
                    si++;
                }
            }
        }

        public void GoToSentence (SentenceNode par) {
            if (par == null) print("Empty sentence node!");
            runner.node = par;
            OutputSentence();
        }

        // skip works in three issues:
        // 1) to stop output current sentence, then show answers and wait answer selection
        // 2) to stop output current sentence and wait next skip action
        // 3) go to next Sentence Node and start output its text
        public void OnSkipSentence (InputAction.CallbackContext context) {
            if (context.performed) SkipSentence();
        }

        public void SkipSentence () {
            StopAllCoroutines();
            if (runner.node != null) {
                if (runner.node is SentenceNode) {
                    if (sentenceText.maxVisibleCharacters < sentenceText.text.Length) {
                        sentenceText.maxVisibleCharacters = sentenceText.text.Length;
                        StartCoroutine(FillAnswer(runner.Next()));
                    } else {
                        OutputSentence();
                    }
                }
            } else CloseDialog();
        }

        void CloseDialog () {
            dialogContainer.SetActive(false);
            dialogStopped?.Invoke();
        }

    }

}

