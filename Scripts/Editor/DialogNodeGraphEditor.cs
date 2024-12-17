using UnityEditor;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    [CustomEditor(typeof(DialogNodeGraph))]
    public class DialogNodeGraphEditor : Editor {

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            DialogNodeGraph nodeGraph = (DialogNodeGraph)target;
            if (GUILayout.Button("Open Editor Window")) {
                NodeEditor.SetGraph(nodeGraph);
                NodeEditor.OpenWindow();
            }
        }
    }

}
