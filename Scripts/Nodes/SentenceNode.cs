using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace KulibinSpace.DialogSystem {

    [System.Serializable]
    public struct Sentence {
        public string text;

        public Sentence (string characterName, string text) {
            //characterSprite = null;
            //this.characterName = characterName;
            this.text = text;
        }
    }

    public class SentenceNode : Node {
        public UnityEvent sentenceSignal; // for scriptable signals
        public LocalizedString stringRef = new() { TableReference = "DialogSystemDemo", TableEntryReference = "" };
        [SerializeField] private Sentence sentence;
        [Space(10)]
        // public? Let it be for debug and visual clarity
        public List<Node> parentNodes = new();
        public List<Node> childNodes = new();
        private const float labelFieldSpace = 45f;
        private const float textFieldWidth = 165f;
        private const float textAreaFieldWidth = 220f;
        private const float textFieldHeight = 40;
        //
        public string RawText { get { return sentence.text; } }
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
            if (stringRef.IsEmpty)
                return sentence.text;
            else
                return stringRef.GetLocalizedString();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draw Sentence Node method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="lableStyle"></param>
        public override void Draw (GUIStyle nodeStyle, GUIStyle labelStyle) {
            base.Draw(nodeStyle, labelStyle);
            // is local here
            string currentValue = ""; if (!stringRef.IsEmpty) currentValue = stringRef.GetLocalizedString();
            string sentenceTitle = "Sentence";
            // stringRef.TableEntryReference.Key --- is EMPTY!!! Do not know WHY!!!
            if (currentValue != "") sentenceTitle += " / " + LocalizationSettings.SelectedLocale.name + "";
            //
            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField(sentenceTitle, labelStyle);
            //DrawCharacterNameFieldHorizontal();
            // Draw label and text fields for sentence text
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField($"Text ", GUILayout.Width(labelFieldSpace));
            if (currentValue == "") { // editable sentence
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                sentence.text = EditorGUILayout.TextArea(sentence.text, textAreaStyle, GUILayout.Width(textAreaFieldWidth), GUILayout.Height(textFieldHeight));
            } else { // non-editable localized content
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) {
                    wordWrap = true, normal = { textColor = Color.black }
                };
                GUI.color = new Color(0.8f, 0.8f, 0.8f);
                EditorGUILayout.SelectableLabel(currentValue, textAreaStyle, GUILayout.Width(textAreaFieldWidth), GUILayout.Height(textFieldHeight));
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw label and text fields for char name
        /// </summary>
        private void DrawCharacterNameFieldHorizontal () {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField($"Name ", GUILayout.Width(labelFieldSpace));
            //sentence.characterName = EditorGUILayout.TextField(sentence.characterName, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for char sprite
        /// </summary>
        private void DrawCharacterSpriteHorizontal () {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(labelFieldSpace));
            //sentence.characterSprite = (Sprite)EditorGUILayout.ObjectField(sentence.characterSprite, typeof(Sprite), false, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
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
