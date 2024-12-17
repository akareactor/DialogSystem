using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KulibinSpace.DialogSystem {
    
    public class Node : ScriptableObject {
        [HideInInspector] public DialogNodeGraph nodeGraph;
        [HideInInspector] public Rect rect = new Rect();
        [HideInInspector] public bool isDragging;
        [HideInInspector] public bool isSelected;
        protected float standartHeight;

#if UNITY_EDITOR

        /// <summary>
        /// Base initialisation method
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeGraph"></param>
        public virtual void Initialize (Rect rect, string nodeName, DialogNodeGraph par) {
            Debug.Log("Initialise " + (par != null));
            name = nodeName;
            standartHeight = rect.height;
            this.rect = rect;
            this.nodeGraph = par;
        }

        /// <summary>
        /// Base draw method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="lableStyle"></param>
        public virtual void Draw (GUIStyle nodeStyle, GUIStyle lableStyle) { }

        public virtual bool AddToParentConnectedNode (Node nodeToAdd) { return true; }
        public virtual bool AddToChildConnectedNode (Node nodeToAdd) { return true; }
        public virtual bool CanAddAsChildren (Node par) { return true; }
        public virtual bool CanAddAsParent (Node par) { return true; }
        public virtual void NotifyConnectedToRemove () {}
        public virtual void RemoveFromParents (Node par) {}
        public virtual void RemoveFromChildren (Node par) {}

        /// <summary>
        /// Drag node
        /// </summary>
        /// <param name="delta"></param>
        public void DragNode (Vector2 delta) {
            rect.position += delta;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
