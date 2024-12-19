using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    [System.Serializable]
    public struct Sentence {
        public string characterName;
        public string text;
        public Sprite characterSprite;

        public Sentence (string characterName, string text) {
            characterSprite = null;
            this.characterName = characterName;
            this.text = text;
        }
    }

    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Sentence Node", fileName = "New Sentence Node")]
    public class SentenceNode : Node {

        [SerializeField] private Sentence sentence;

        [Space(10)]
        // public? Let it be for debug and visual clarity
        public List<Node> parentNodes = new();
        public List<Node> childNodes = new();

        [Space(7)]
        [SerializeField] private bool isExternalFunc;
        [SerializeField] private string externalFunctionName;

        private string externalButtonLabel;

        private const float labelFieldSpace = 45f;
        private const float textFieldWidth = 165f;

        //private const float externalNodeHeight = 80f;

        /// <summary>
        /// Returning external function name
        /// </summary>
        /// <returns></returns>
        public string GetExternalFunctionName () {
            return externalFunctionName;
        }

        /// <summary>
        /// Returning sentence character name
        /// </summary>
        /// <returns></returns>
        public string GetSentenceCharacterName () {
            return sentence.characterName;
        }

        /// <summary>
        /// Setting sentence text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void SetSentenceText (string text) {
            sentence.text = text;
        }

        /// <summary>
        /// Returning sentence text
        /// </summary>
        /// <returns></returns>
        public string GetSentenceText () {
            return sentence.text;
        }

        /// <summary>
        /// Returning sentence character sprite
        /// </summary>
        /// <returns></returns>
        public Sprite GetCharacterSprite () {
            return sentence.characterSprite;
        }

        /// <summary>
        /// Returns the value of a isExternalFunc boolean field
        /// </summary>
        /// <returns></returns>
        public bool IsExternalFunc () {
            return isExternalFunc;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draw Sentence Node method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="lableStyle"></param>
        public override void Draw (GUIStyle nodeStyle, GUIStyle labelStyle) {
            base.Draw(nodeStyle, labelStyle);
            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField("Sentence", labelStyle);
            DrawCharacterNameFieldHorizontal();
            DrawSentenceTextFieldHorizontal();
            /*
            DrawCharacterSpriteHorizontal();
            DrawExternalFunctionTextField();
            if (GUILayout.Button(externalButtonLable)) {
                isExternalFunc = !isExternalFunc;
            }
            */
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw label and text fields for char name
        /// </summary>
        private void DrawCharacterNameFieldHorizontal () {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name ", GUILayout.Width(labelFieldSpace));
            sentence.characterName = EditorGUILayout.TextField(sentence.characterName, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for sentence text
        /// </summary>
        private void DrawSentenceTextFieldHorizontal () {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Text ", GUILayout.Width(labelFieldSpace));
            sentence.text = EditorGUILayout.TextField(sentence.text, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for char sprite
        /// </summary>
        private void DrawCharacterSpriteHorizontal () {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(labelFieldSpace));
            sentence.characterSprite = (Sprite)EditorGUILayout.ObjectField(sentence.characterSprite, typeof(Sprite), false, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for external function, 
        /// depends on IsExternalFunc boolean field
        /// </summary>
        private void DrawExternalFunctionTextField () {
            if (isExternalFunc) {
                externalButtonLabel = "Remove external func";
                EditorGUILayout.BeginHorizontal();
                //rect.height = externalNodeHeight;
                EditorGUILayout.LabelField($"Func Name ", GUILayout.Width(labelFieldSpace));
                externalFunctionName = EditorGUILayout.TextField(externalFunctionName, GUILayout.Width(textFieldWidth));
                EditorGUILayout.EndHorizontal();
            } else {
                externalButtonLabel = "Add external func";
                //rect.height = standartHeight;
            }
        }

        /// <summary>
        /// Checking node size
        /// </summary>
        /// <param name="rect"></param>
        public void CheckNodeSize (float width, float height) {
            rect.width = width;
            if (heightStd == 0) heightStd = height;
            rect.height = heightStd;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the childNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode (Node par) {
            bool ret = false;
            if (par != null && par != this && !childNodes.Exists(x => x == par)) {
                childNodes.Add(par);
                if (IsConnectedToParent(par)) parentNodes.Remove(par);
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the parentNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToParentConnectedNode (Node par) {
            bool ret = false;
            if (par != null && par != this && !parentNodes.Exists(x => x == par)) {
                parentNodes.Add(par);
                if (IsConnectedToChildren(par)) childNodes.Remove(par);
                ret = true;
            }
            return ret;
        }

        public override void RemoveFromParents (Node par) {
            parentNodes.Remove(par);
        }

        public override void RemoveFromChildren (Node par) {
            childNodes.Remove(par);
        }

        public override void NotifyConnectedToRemove (bool selectedOnly) {
            // deconnect parents
            Queue<Node> queue = new Queue<Node>();
            foreach (Node node in parentNodes) {
                if ((selectedOnly && node.isSelected) || !selectedOnly) queue.Enqueue(node);
            }
            while (queue.Count > 0) {
                Node node = queue.Dequeue();
                node.RemoveFromChildren(this);
                RemoveFromParents(node);
            }
            // deconnect children nodes
            queue = new Queue<Node>();
            foreach (Node node in childNodes) {
                if ((selectedOnly && node.isSelected) || !selectedOnly) queue.Enqueue(node);
            }
            while (queue.Count > 0) {
                Node node = queue.Dequeue();
                node.RemoveFromParents(this);
                RemoveFromChildren(node);
            }
        }

        public bool IsConnectedToParent (Node par) {
            return par != null && par != this && parentNodes.Exists(x => x == par);
        }

        public bool IsConnectedToChildren (Node par) {
            return par != null && par != this && childNodes.Exists(x => x == par);
        }

        public override bool CanAddAsChildren (Node par) { 
            return par != null && par != this && !childNodes.Exists(x => x == par);
        }

        public override bool CanAddAsParent (Node par) {
            return par != null && par != this && !parentNodes.Exists(x => x == par);
        }


#endif
    }
}
