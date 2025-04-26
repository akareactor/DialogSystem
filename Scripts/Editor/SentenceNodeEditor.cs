#if UNITY_EDITOR

using UnityEditor;

namespace KulibinSpace.DialogSystem {

    // Указываем, что этот редактор предназначен для класса SentenceNode
    [CustomEditor(typeof(SentenceNode))]
    public class SentenceNodeEditor : Editor {
        public override void OnInspectorGUI () {
            // Получаем текущий объект (SentenceNode)
            SentenceNode sentenceNode = (SentenceNode)target;
            // Обновляем объект перед отображением
            serializedObject.Update();
            // Отображаем стандартные свойства инспектора
            DrawDefaultInspector();
            // Разделитель для наглядности
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Child Nodes", EditorStyles.boldLabel);
            // Проверяем, есть ли дочерние узлы
            if (sentenceNode.childNodes != null && sentenceNode.childNodes.Count > 0) {
                // Перебираем дочерние узлы и отображаем их текст
                for (int i = 0; i < sentenceNode.childNodes.Count; i++) {
                    Node childNode = sentenceNode.childNodes[i];
                    if (childNode != null) {
                        if (childNode is SentenceNode snode) {
                            // Отображаем текст из Node.text вместо ссылки на объект
                            EditorGUILayout.LabelField($"Sentence {i}: {snode.GetSentenceText()}", EditorStyles.helpBox);
                        } else if (childNode is AnswerNode anode) {
                            // Отображаем текст из Node.text вместо ссылки на объект
                            EditorGUILayout.LabelField($"Answer {i}: {anode.GetAnswer(0)}", EditorStyles.helpBox);
                        }
                    } else {
                        // Если узел отсутствует, выводим сообщение
                        EditorGUILayout.LabelField($"Child {i + 1}: NULL", EditorStyles.helpBox);
                    }
                }
            } else {
                // Если список пуст, выводим сообщение
                EditorGUILayout.LabelField("No child nodes available.", EditorStyles.helpBox);
            }

            // Применяем изменения к объекту
            serializedObject.ApplyModifiedProperties();
        }
    }

}

#endif
