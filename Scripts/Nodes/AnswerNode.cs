using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace KulibinSpace.DialogSystem {

    [System.Serializable]
    public struct Answer {
        public string answer;
        public LocalizedString stringRef;
    }

    public class AnswerNode : Node {
        private int amountOfAnswers = 1;
        public List<Answer> answers = new();
        // typed nodes!
        public List<SentenceNode> parentSentenceNodes = new();
        public List<SentenceNode> childSentenceNodes = new();
        private const float lableFieldSpace = 18f;
        private const float textFieldWidth = 220f;
        private const float textFieldHeight = 20;
        private const float answerNodeWidth = 265f;
        private const float answerNodeHeight = 80f;
        private float currentAnswerNodeHeight = 80f;
        private const float additionalAnswerNodeHeight = 20f;

        public List<string> Answers { get { return GetAnswers(); } }

        List<string> GetAnswers () {
            List<string> ret = new();
            foreach (var answer in answers) {
                if (answer.stringRef.IsEmpty) ret.Add(answer.answer); else ret.Add(answer.stringRef.GetLocalizedString());
            }
            return ret;
        }

        public string GetAnswer (int index) {
            string ret = "";
            if (answers != null && answers.Count > index) {
                Answer answer = answers[index];
                if (answer.stringRef.IsEmpty) ret = answer.answer; else ret = answer.stringRef.GetLocalizedString();
            }
            return ret;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Answer node initialisation method
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeGraph"></param>
        public override void Initialize (Rect rect, string nodeName, DialogNodeGraph nodeGraph) {
            base.Initialize(rect, nodeName, nodeGraph);
            CalculateAmountOfAnswers();
            childSentenceNodes = new List<SentenceNode>(amountOfAnswers);
        }

        /// <summary>
        /// Draw Answer Node method
        /// </summary>
        /// <param name = "nodeStyle" ></ param >
        /// < param name="labelStyle"></param>
        public override void Draw (GUIStyle nodeStyle, GUIStyle labelStyle) {
            base.Draw(nodeStyle, labelStyle);
            childSentenceNodes.RemoveAll(item => item == null);
            rect.size = new Vector2(answerNodeWidth, currentAnswerNodeHeight);
            GUILayout.BeginArea(rect, nodeStyle);

            // label
            GUIStyle answerLabelStyle = new GUIStyle(labelStyle);
            answerLabelStyle.normal.textColor = new Color(255, 165, 0, 1); // orange
            // answers
            EditorGUILayout.LabelField("Answer", answerLabelStyle);
            for (int i = 0; i < amountOfAnswers; i++) {
                if (i < childSentenceNodes.Count)
                    DrawAnswerLine(i + 1, StringConstants.GreenDot);
                else
                    DrawAnswerLine(i + 1, StringConstants.EmptyDot);
            }
            DrawAnswerNodeButtons();

            GUILayout.EndArea();
        }

        Answer NewAnswerItem () {
            return new Answer {
                answer = string.Empty,
                stringRef = new LocalizedString("DialogSystemDemo", "")
            };
        }

        /// <summary>
        /// Determines the number of answers depending on answers list count
        /// </summary>
        public void CalculateAmountOfAnswers () {
            if (answers.Count == 0) {
                amountOfAnswers = 1;
                //answers = new List<string>() { string.Empty };
                answers = new List<Answer>() { NewAnswerItem() };
            } else {
                amountOfAnswers = answers.Count;
            }
        }

        /// <summary>
        /// Draw answer line
        /// </summary>
        /// <param name="answerNumber"></param>
        /// <param name="iconPathOrName"></param>
        private void DrawAnswerLine (int answerNumber, string iconPathOrName) {
            EditorGUILayout.BeginHorizontal();
            Answer a = answers[answerNumber - 1];
            string currentValue = ""; if (!a.stringRef.IsEmpty) currentValue = a.stringRef.GetLocalizedString();
            if (currentValue == "") {
                // editable sentence
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                a.answer = EditorGUILayout.TextArea(a.answer, textAreaStyle, GUILayout.Width(textFieldWidth));
                answers[answerNumber - 1] = a;
            } else {
                // non-editable localized content
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) {
                    wordWrap = true,
                    normal = { textColor = Color.black }
                };
                GUI.color = new Color(0.8f, 0.8f, 0.8f);
                EditorGUILayout.SelectableLabel(currentValue, textAreaStyle, GUILayout.Width(textFieldWidth), GUILayout.Height(textFieldHeight));
                GUI.color = Color.white;
            }
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(iconPathOrName), GUILayout.Width(lableFieldSpace));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnswerNodeButtons () {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(-10);
            if (GUILayout.Button("Add", GUILayout.Width(82))) {
                IncreaseAmountOfAnswers();
            }
            if (GUILayout.Button("Remove", GUILayout.Width(82))) {
                DecreaseAmountOfAnswers();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Increase amount of answers and node height
        /// </summary>
        private void IncreaseAmountOfAnswers () {
            amountOfAnswers++;
            answers.Add(NewAnswerItem());
            currentAnswerNodeHeight += additionalAnswerNodeHeight;
        }

        /// <summary>
        /// Decrease amount of answers and node height 
        /// </summary>
        private void DecreaseAmountOfAnswers () {
            if (answers.Count == 1) {
                return;
            }
            answers.RemoveAt(amountOfAnswers - 1);
            if (childSentenceNodes.Count == amountOfAnswers) {
                childSentenceNodes[amountOfAnswers - 1].RemoveFromParents(this);
                childSentenceNodes.RemoveAt(amountOfAnswers - 1);
            }
            amountOfAnswers--;
            currentAnswerNodeHeight -= additionalAnswerNodeHeight;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the parentSentenceNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToParentConnectedNode (Node par) {
            if (par != null && par != this && par is SentenceNode snode && !parentSentenceNodes.Exists(x => x == par)) {
                parentSentenceNodes.Add(snode);
                if (childSentenceNodes.Exists(x => x == par)) childSentenceNodes.Remove(snode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the childSentenceNodes array
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode (Node par) {
            if (par is SentenceNode snode) {
                if (CanAddToChildConnectedNode(snode)) {
                    childSentenceNodes.Add(snode);
                    snode.AddToParentConnectedNode(this);
                    return true;
                }
            } else Debug.Log("Answer tries to add NOT a Sentence!");
            return false;
        }

        /// <summary>
        /// Calculate answer node height based on amount of answers
        /// </summary>
        public void CalculateAnswerNodeHeight () {
            currentAnswerNodeHeight = answerNodeHeight;
            for (int i = 0; i < amountOfAnswers - 1; i++) {
                currentAnswerNodeHeight += additionalAnswerNodeHeight;
            }
        }

        private bool CanAddToChildConnectedNode (SentenceNode par) {
            bool ret = childSentenceNodes.Count < amountOfAnswers && !par.IsConnectedToChildren(this) && !par.IsConnectedToParent(this);
            return ret;
        }

        public override bool CanAddAsChildren (Node par) {
            bool ret = false;
            if (par != null && par != this && par is SentenceNode snode) {
                ret = CanAddToChildConnectedNode(snode);
            }
            return ret;
        }

        public override bool CanAddAsParent (Node par) {
            return par != null && par != this && !parentSentenceNodes.Exists(x => x == par);
        }

        // becouse every connection is duplicated on its visavi
        public override void NotifyConnectedToRemove (bool selectedOnly) {
            // deconnect parents
            Queue<Node> queue = new Queue<Node>();
            foreach (Node node in parentSentenceNodes) {
                if ((selectedOnly && node.isSelected) || !selectedOnly) queue.Enqueue(node);
            }
            while (queue.Count > 0) {
                Node node = queue.Dequeue();
                node.RemoveFromChildren(this);
                RemoveFromParents(node);
            }
            // deconnect children nodes
            queue = new Queue<Node>();
            foreach (Node node in childSentenceNodes) {
                if ((selectedOnly && node.isSelected) || !selectedOnly) queue.Enqueue(node);
            }
            while (queue.Count > 0) {
                Node node = queue.Dequeue();
                node.RemoveFromParents(this);
                RemoveFromChildren(node);
            }

        }

        public override void RemoveFromParents (Node par) {
            if (par != null && par != this && par is SentenceNode snode) parentSentenceNodes.Remove(snode);
        }

        public override void RemoveFromChildren (Node par) {
            if (par != null && par != this && par is SentenceNode snode) childSentenceNodes.Remove(snode);
        }

#endif

        public SentenceNode GetChild (int index) {
            SentenceNode ret = null;
            if (index >= 0 && index < childSentenceNodes.Count) ret = childSentenceNodes[index];
            return ret;
        }

    }
}
