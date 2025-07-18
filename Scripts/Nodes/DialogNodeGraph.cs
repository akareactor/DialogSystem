using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace KulibinSpace.DialogSystem {

    [CreateAssetMenu(menuName = "Kulibin Space/Scriptable Objects/Dialog/Node Graph", fileName = "New Kulibin Space Dialog System Graph")]

    public class DialogNodeGraph : ScriptableObject {
        
        public string characterName;
        public LocalizedString stringRef = new () { TableReference = "DialogSystemDemo", TableEntryReference = "characterName" };
        public Sprite characterSprite;
        public List<Node> nodes = new();

#if UNITY_EDITOR

        [HideInInspector] public Node nodeToDrawLineFrom = null;
        [HideInInspector] public Vector2 linePosition = Vector2.zero;

        /// <summary>
        /// Assigning values to nodeToDrawLineFrom and linePosition fields
        /// </summary>
        /// <param name="nodeToDrawLineFrom"></param>
        /// <param name="linePosition"></param>
        public void SetNodeToDrawLineFromAndLinePosition (Node nodeToDrawLineFrom, Vector2 linePosition) {
            this.nodeToDrawLineFrom = nodeToDrawLineFrom;
            this.linePosition = linePosition;
        }

        /// <summary>
        /// Draging all selected nodes
        /// </summary>
        /// <param name="delta"></param>
        public void DragAllSelectedNodes (Vector2 delta) {
            foreach (var node in nodes) {
                if (node.isSelected) {
                    node.DragNode(delta);
                }
            }
        }

        /// <summary>
        /// Returning amount of selected nodes
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfSelectedNodes () {
            int amount = 0;
            foreach (Node node in nodes) {
                if (node.isSelected) {
                    amount++;
                }
            }
            return amount;
        }

#endif
    }
}
