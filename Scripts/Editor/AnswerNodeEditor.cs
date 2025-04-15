using UnityEditor;

namespace KulibinSpace.DialogSystem {

    // Указываем, что этот редактор предназначен для класса AnswerNode
    [CustomEditor(typeof(AnswerNode))]
    public class AnswerNodeEditor : Editor {
        public override void OnInspectorGUI () {
            AnswerNode answerNode = (AnswerNode)target;
            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Child Nodes", EditorStyles.boldLabel);
            if (answerNode.childSentenceNodes != null && answerNode.childSentenceNodes.Count > 0) {
                for (int i = 0; i < answerNode.childSentenceNodes.Count; i++) {
                    Node childNode = answerNode.childSentenceNodes[i];
                    if (childNode != null) {
                        if (childNode is SentenceNode snode) { // Sentence only!
                            EditorGUILayout.LabelField($"Sentence {i}: {snode.GetSentenceText()}", EditorStyles.helpBox);
                        }
                    } else {
                        EditorGUILayout.LabelField($"Child {i + 1}: NULL", EditorStyles.helpBox);
                    }
                }
            } else {
                EditorGUILayout.LabelField("No child nodes available.", EditorStyles.helpBox);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

}
