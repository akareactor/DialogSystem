using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Answer Node", fileName = "New Answer Node")]
    public class AnswerNode : Node {
        private int amountOfAnswers = 1;
        public List<string> answers = new();
        public List<SentenceNode> parentSentenceNodes = new();
        public List<SentenceNode> childSentenceNodes = new();
        private const float lableFieldSpace = 18f;
        private const float textFieldWidth = 120f;
        private const float answerNodeWidth = 190f;
        private const float answerNodeHeight = 115f;
        private float currentAnswerNodeHeight = 115f;
        private float additionalAnswerNodeHeight = 20f;

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
        /// < param name="lableStyle"></param>
        public override void Draw (GUIStyle nodeStyle, GUIStyle lableStyle) {
            base.Draw(nodeStyle, lableStyle);
            childSentenceNodes.RemoveAll(item => item == null);
            rect.size = new Vector2(answerNodeWidth, currentAnswerNodeHeight);
            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField("Answer Node", lableStyle);
            for (int i = 0; i < amountOfAnswers; i++) {
                DrawAnswerLine(i + 1, StringConstants.GreenDot);
            }
            DrawAnswerNodeButtons();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Determines the number of answers depending on answers list count
        /// </summary>
        public void CalculateAmountOfAnswers () {
            if (answers.Count == 0) {
                amountOfAnswers = 1;
                answers = new List<string>() { string.Empty };
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
            EditorGUILayout.LabelField($"{answerNumber}. ", GUILayout.Width(lableFieldSpace));
            answers[answerNumber - 1] = EditorGUILayout.TextField(answers[answerNumber - 1], GUILayout.Width(textFieldWidth));
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(iconPathOrName), GUILayout.Width(lableFieldSpace));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnswerNodeButtons () {
            if (GUILayout.Button("Add answer")) {
                IncreaseAmountOfAnswers();
            }

            if (GUILayout.Button("Remove answer")) {
                DecreaseAmountOfAnswers();
            }
        }

        /// <summary>
        /// Increase amount of answers and node height
        /// </summary>
        private void IncreaseAmountOfAnswers () {
            amountOfAnswers++;
            answers.Add(string.Empty);
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
            } else Debug.Log("Ответ пытается добавить в потомки не Сентенцию ");
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
        public override void NotifyConnectedToRemove () {
            foreach(SentenceNode par in parentSentenceNodes) {
                par.RemoveFromChildren(this);
            }
            foreach(SentenceNode par in childSentenceNodes) {
                par.RemoveFromParents(this);
            }
        }

        public override void RemoveFromParents (Node par) {
            if (par != null && par != this && par is SentenceNode snode) parentSentenceNodes.Remove(snode);
        }

        public override void RemoveFromChildren (Node par) {
            if (par != null && par != this && par is SentenceNode snode) childSentenceNodes.Remove(snode);
        }



#endif
    }
}
