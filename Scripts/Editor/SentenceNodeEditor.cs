#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    [CustomEditor(typeof(SentenceNode))]
    public class SentenceNodeEditor : Editor {
        public override void OnInspectorGUI () {
            SentenceNode sentenceNode = (SentenceNode)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sentenceSignal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringRef"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sentence", EditorStyles.boldLabel);
            var sentenceProp = serializedObject.FindProperty("sentence");
            var textProp = sentenceProp.FindPropertyRelative("text");

            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

            string text = textProp.stringValue;

            float minHeight = 60f;
            float calculatedHeight = wrapStyle.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - 40f); // немного запас

            // Display TextArea
            textProp.stringValue = EditorGUILayout.TextArea(
                text,
                wrapStyle,
                GUILayout.Height(Mathf.Max(minHeight, calculatedHeight))
            );

            //textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Height(60)); // fixed field size

            // Parent nodes, visualization only
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Parent Nodes", EditorStyles.boldLabel);
            if (sentenceNode.parentNodes != null && sentenceNode.parentNodes.Count > 0) {
                for (int i = 0; i < sentenceNode.parentNodes.Count; i++) {
                    var parent = sentenceNode.parentNodes[i];
                    EditorGUILayout.LabelField($"Parent {i}: {(parent != null ? parent.name : "NULL")}", EditorStyles.helpBox);
                }
            } else {
                EditorGUILayout.LabelField("No parent nodes.", EditorStyles.helpBox);
            }

            // Child nodes
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Child Nodes", EditorStyles.boldLabel);
            if (sentenceNode.childNodes != null && sentenceNode.childNodes.Count > 0) {
                for (int i = 0; i < sentenceNode.childNodes.Count; i++) {
                    var child = sentenceNode.childNodes[i];
                    if (child != null) {
                        if (child is SentenceNode snode) {
                            EditorGUILayout.LabelField($"Sentence {i}: {snode.GetSentenceText()}", EditorStyles.helpBox);
                        } else if (child is AnswerNode anode) {
                            EditorGUILayout.LabelField($"Answer {i}: {anode.GetAnswer(0)}", EditorStyles.helpBox);
                        } else {
                            EditorGUILayout.LabelField($"Child {i}: {child.name}", EditorStyles.helpBox);
                        }
                    } else {
                        EditorGUILayout.LabelField($"Child {i}: NULL", EditorStyles.helpBox);
                    }
                }
            } else {
                EditorGUILayout.LabelField("No child nodes.", EditorStyles.helpBox);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}

#endif
