#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace KulibinSpace.DialogSystem {

    [CustomEditor(typeof(DialogNodeGraph))]
    public class DialogNodeGraphEditor : Editor {

        public override void OnInspectorGUI () {

            base.OnInspectorGUI();

            DialogNodeGraph nodeGraph = (DialogNodeGraph)target;

            // Отображаем поле для LocalizedString
            //nodeGraph.stringRef = (LocalizedString)EditorGUILayout.ObjectField("Localized Greeting", nodeGraph.stringRef, typeof(LocalizedString), false);

            // Если LocalizedString задан, отображаем его значение
            if (!nodeGraph.stringRef.IsEmpty) {
                string currentValue = nodeGraph.stringRef.GetLocalizedString();
                EditorGUILayout.LabelField("Character name:", currentValue);
            } else {
                EditorGUILayout.HelpBox("No LocalizedString selected.", MessageType.Info);
            }

            if (GUILayout.Button("Open Editor Window")) {
                NodeEditor.SetGraph(nodeGraph);
                NodeEditor.OpenWindow();
            }

            // Сохраняем изменения
            if (GUI.changed) {
                EditorUtility.SetDirty(nodeGraph);
            }

        }

    }

}

#endif



