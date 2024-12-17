using System;
using System.Collections.Generic;
using Codice.CM.Client.Differences.Merge;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    public class NodeEditor : EditorWindow {

        private static DialogNodeGraph graph;
        private Node currentNode;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;

        private GUIStyle labelStyle;

        private Rect selectionRect;
        private Vector2 mouseClickPosition;

        private Vector2 graphOffset;
        private Vector2 graphDrag;

        private const float nodeWidth = 190f;
        private const float nodeHeight = 135f;

        private const float connectingLineWidth = 2f;
        private const float connectingLineArrowSize = 8f;

        private const int lableFontSize = 12;

        private const int nodePadding = 20;
        private const int nodeBorder = 10;

        private const float gridLargeLineSpacing = 100f;
        private const float gridSmallLineSpacing = 25;

        private bool isSelecting = false;

        /// <summary>
        /// Define nodes and lable style parameters on enable
        /// </summary>
        private void OnEnable () {
            Selection.selectionChanged += ChangeEditorWindowOnSelection;

            nodeStyle = new GUIStyle {
                padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding),
                border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder)
            };
            nodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.Node) as Texture2D;

            selectedNodeStyle = new GUIStyle {
                border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder),
                padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding)
            };
            selectedNodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.SelectedNode) as Texture2D;

            labelStyle = new GUIStyle {
                alignment = TextAnchor.MiddleLeft,
                fontSize = lableFontSize
            };
            labelStyle.normal.textColor = Color.white;
        }

        /// <summary>
        /// Saving all changes and unsubscribing from events
        /// </summary>
        private void OnDisable () {
            Selection.selectionChanged -= ChangeEditorWindowOnSelection;
            AssetDatabase.SaveAssets();
            SaveChanges();
        }

        /// <summary>
        /// Open Node Editor Window when Node Graph asset is double clicked in the inspector
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(0)]
        public static bool OnDoubleClickAsset (int instanceID, int line) {
            DialogNodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as DialogNodeGraph;
            if (graph != null) {
                SetUpNodes();
            }
            if (nodeGraph != null) {
                OpenWindow();
                graph = nodeGraph;
                SetUpNodes();
                return true;
            }
            return false;
        }

        public static void SetGraph (DialogNodeGraph nodeGraph) {
            graph = nodeGraph;
        }

        /// <summary>
        /// Open Node Editor window
        /// </summary>
        [MenuItem("Kulibin Space Dialog System Editor", menuItem = "Window/Kulibin Space Dialog Graph Editor")]
        public static void OpenWindow () {
            NodeEditor window = (NodeEditor)GetWindow(typeof(NodeEditor));
            window.titleContent = new GUIContent("Dialog Graph Editor");
            window.Show();
        }

        /// <summary>
        /// Rendering and handling GUI events
        /// </summary>
        private void OnGUI () {
            if (graph != null) {
                DrawDraggedLine();
                DrawNodeConnection();
                DrawGridBackground(gridSmallLineSpacing, 0.2f, Color.gray);
                DrawGridBackground(gridLargeLineSpacing, 0.2f, Color.gray);
                ProcessEvents(Event.current);
                DrawNodes();
            }
            if (GUI.changed) Repaint();
        }

        /// <summary>
        /// Setting up nodes when opening the editor
        /// </summary>
        private static void SetUpNodes () {
            foreach (Node node in graph.nodes) {
                if (node.GetType() == typeof(AnswerNode)) {
                    AnswerNode answerNode = (AnswerNode)node;
                    answerNode.CalculateAmountOfAnswers();
                    answerNode.CalculateAnswerNodeHeight();
                }
                if (node.GetType() == typeof(SentenceNode)) {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    sentenceNode.CheckNodeSize(nodeWidth, nodeHeight);
                }
            }
        }

        /// <summary>
        /// Draw connection line during dragging
        /// </summary>
        private void DrawDraggedLine () {
            if (graph.linePosition != Vector2.zero) {
                Handles.DrawBezier(graph.nodeToDrawLineFrom.rect.center, graph.linePosition,
                   graph.nodeToDrawLineFrom.rect.center, graph.linePosition,
                   Color.white, null, connectingLineWidth);
            }
        }

        /// <summary>
        /// Draw connections between nodes
        /// </summary>
        private void DrawNodeConnection () {
            if (graph.nodes != null) {
                foreach (Node node in graph.nodes) {
                    if (node is AnswerNode answerNode) {
                        DrawAnswerNode(answerNode);
                    } else if (node is SentenceNode sentenceNode) {
                        DrawSentenceNode(sentenceNode);
                    }
                }
            }

            void DrawAnswerNode (AnswerNode answerNode) {
                for (int i = 0; i < answerNode.childSentenceNodes.Count; i++) {
                    if (answerNode.childSentenceNodes[i] != null) {
                        DrawConnectionLine(answerNode, answerNode.childSentenceNodes[i]);
                    }
                }
            }

            void DrawSentenceNode (SentenceNode sentenceNode) {
                foreach (Node child in sentenceNode.childNodes) {
                    DrawConnectionLine(sentenceNode, child);
                }
            }
        }

        /// <summary>
        /// Draw connection line from parent to child node
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        private void DrawConnectionLine (Node parentNode, Node childNode) {
            Vector2 startPosition = parentNode.rect.center;
            Vector2 endPosition = childNode.rect.center;
            Vector2 midPosition = (startPosition + endPosition) / 2;
            Vector2 direction = endPosition - startPosition;
            Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
            Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
            Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;
            // arrow connector
            Handles.color = Color.blue;
            Handles.DrawLine(arrowHeadPoint, arrowTailPoint1);
            Handles.DrawLine(arrowHeadPoint, arrowTailPoint2);
            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.blue, null, connectingLineWidth);
            GUI.changed = true;
        }

        /// <summary>
        /// Draw grid background lines for node editor window
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="gridOpacity"></param>
        /// <param name="color"></param>
        private void DrawGridBackground (float gridSize, float gridOpacity, Color color) {
            int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
            int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);
            Handles.color = new Color(color.r, color.g, color.b, gridOpacity);
            graphOffset += graphDrag * 0.5f;
            Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);
            for (int i = 0; i < verticalLineCount; i++) {
                Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
            }
            for (int j = 0; j < horizontalLineCount; j++) {
                Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
            }
            Handles.color = Color.white;
        }

        /// <summary>
        /// Call Draw method from all existing nodes in nodes list
        /// </summary>
        private void DrawNodes () {
            if (graph.nodes != null) {
                foreach (Node node in graph.nodes) {
                    if (!node.isSelected) {
                        node.Draw(nodeStyle, labelStyle);
                    } else {
                        node.Draw(selectedNodeStyle, labelStyle);
                    }
                }
                GUI.changed = true;
            }
        }

        void DeselectAll () {
            foreach (Node node in graph.nodes) { if (node.isSelected) node.isSelected = false; }
        }

        /// <summary>
        /// Process events
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessEvents (Event currentEvent) {
            graphDrag = Vector2.zero;
            switch (currentEvent.type) {
                case EventType.MouseDown: ProcessMouseDownEvent(currentEvent); break;
                case EventType.MouseUp: ProcessMouseUpEvent(currentEvent); break;
                case EventType.MouseDrag: ProcessMouseDragEvent(currentEvent); break;
                case EventType.Repaint: SelectNodesBySelectionRect(currentEvent.mousePosition); break;
                default: break;
            }
        }

        /// <summary>
        /// Process mouse down event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseDownEvent (Event currentEvent) {
            if (currentEvent.button == 0) { // LMB
                if (graph.nodes != null) currentNode = GetHoveredNode(currentEvent.mousePosition);
                mouseClickPosition = currentEvent.mousePosition;
                if (currentNode == null) isSelecting = true; // starting a multiselection
            } else if (currentEvent.button == 1) { // RMB
                Node node = GetHoveredNode(currentEvent.mousePosition);
                if (node != null) {
                    graph.SetNodeToDrawLineFromAndLinePosition(node, currentEvent.mousePosition);
                } else {
                    ShowContextMenu(currentEvent.mousePosition);
                }
            } else if (currentEvent.button == 2) { // Wheel
            }
        }

        /// <summary>
        /// Process mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseUpEvent (Event currentEvent) {
            if (currentEvent.button == 0) {
                // clear selection, leave one selected node
                if (graph.nodes != null) {
                    if (!isSelecting) {
                        // don't know how mush nodes are selected, so just apply common deselect
                        currentNode = GetHoveredNode(currentEvent.mousePosition);
                        if (currentNode == null) {
                            DeselectAll();
                        } else {
                            if (!currentEvent.shift && !draggingSelectedNodes) DeselectAll();
                            currentNode.isSelected = true;
                        }
                    }
                }
                draggingSelectedNodes = false;
                selectionRect = new Rect(0, 0, 0, 0);
                isSelecting = false;
            } else if (currentEvent.button == 1) { // RMB
                if (graph.nodeToDrawLineFrom != null) {
                    CheckLineConnection(currentEvent);
                    ClearDraggedLine();
                }
            } else if (currentEvent.button == 2) { // Wheel
            }
        }

        bool draggingSelectedNodes = false;

        /// <summary>
        /// Process mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessMouseDragEvent (Event currentEvent) {
            if (currentEvent.button == 0) { // LMB
                if (isSelecting) {
                    SelectNodesBySelectionRect(currentEvent.mousePosition);
                } else if (currentNode != null && currentNode.isSelected) {
                    draggingSelectedNodes = true;
                    graph.DragAllSelectedNodes(currentEvent.delta);
                }
                GUI.changed = true;
            } else if (currentEvent.button == 1) { // RMB
                if (graph.nodeToDrawLineFrom != null) {
                    graph.linePosition += currentEvent.delta;
                    GUI.changed = true;
                }
            } else if (currentEvent.button == 2) { // Wheel
                graphDrag = currentEvent.delta;
                foreach (var node in graph.nodes) node.DragNode(graphDrag); // drag all nodes to imitate canvas movement
            }
        }

        /// <summary>
        /// Check line connect when right mouse up
        /// </summary>
        /// <param name="currentEvent"></param>
        private void CheckLineConnection (Event currentEvent) {
            if (graph.nodeToDrawLineFrom != null) {
                Node node = GetHoveredNode(currentEvent.mousePosition);
                if (node != null) {
                    if (graph.nodeToDrawLineFrom.CanAddAsChildren(node) && node.CanAddAsParent(graph.nodeToDrawLineFrom)) {
                        Debug.Log("Node Editor: добавляю к потомкам");
                        graph.nodeToDrawLineFrom.AddToChildConnectedNode(node);
                        Debug.Log("Node Editor: добавляю к родителям");
                        node.AddToParentConnectedNode(graph.nodeToDrawLineFrom);
                    }
                }
            }
        }

        /// <summary>
        /// Clear dragged line
        /// </summary>
        private void ClearDraggedLine () {
            graph.nodeToDrawLineFrom = null;
            graph.linePosition = Vector2.zero;
            GUI.changed = true;
        }

        /// <summary>
        /// Draw selection rect and select all node in it
        /// </summary>
        /// <param name="mousePosition"></param>
        private void SelectNodesBySelectionRect (Vector2 mousePosition) {
            if (isSelecting) {
                selectionRect = new Rect(mouseClickPosition.x, mouseClickPosition.y, mousePosition.x - mouseClickPosition.x, mousePosition.y - mouseClickPosition.y);
                //EditorGUI.DrawRect(selectionRect, new Color(0, 0, 0, 0.7f));
                Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0, 0, 0, 0.05f), Color.gray / 2);
                foreach (Node node in graph.nodes) {
                    node.isSelected = selectionRect.Contains(node.rect.position);
                }
            }
        }

        /// <summary>
        /// Return the node that is at the mouse position
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private Node GetHoveredNode (Vector2 mousePosition) {
            foreach (Node node in graph.nodes) {
                if (node.rect.Contains(mousePosition)) {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Show the context menu
        /// </summary>
        /// <param name="mousePosition"></param>
        private void ShowContextMenu (Vector2 mousePosition) {
            Debug.Log("Show context menu");
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Create Sentence Node"), false, CreateSentenceNode, mousePosition);
            contextMenu.AddItem(new GUIContent("Create Answer Node"), false, CreateAnswerNode, mousePosition);
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Select All Nodes"), false, SelectAllNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Selected Nodes"), false, RemoveSelectedNodes, mousePosition);
            contextMenu.ShowAsContext();
        }

        /// <summary>
        /// Create Sentence Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateSentenceNode (object mousePositionObject) {
            SentenceNode sentenceNode = ScriptableObject.CreateInstance<SentenceNode>();
            InitialiseNode(mousePositionObject, sentenceNode, "Sentence Node");
        }

        /// <summary>
        /// Create Answer Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateAnswerNode (object mousePositionObject) {
            AnswerNode answerNode = ScriptableObject.CreateInstance<AnswerNode>();
            InitialiseNode(mousePositionObject, answerNode, "Answer Node");
        }

        /// <summary>
        /// Select all nodes in node editor
        /// </summary>
        /// <param name="userData"></param>
        private void SelectAllNodes (object userData) {
            foreach (Node node in graph.nodes) {
                node.isSelected = true;
            }
            GUI.changed = true;
        }

        /// <summary>
        /// Remove all selected nodes
        /// </summary>
        /// <param name="userData"></param>
        private void RemoveSelectedNodes (object userData) {
            Queue<Node> nodeDeletionQueue = new Queue<Node>();
            foreach (Node node in graph.nodes) {
                if (node.isSelected) {
                    nodeDeletionQueue.Enqueue(node);
                }
            }
            while (nodeDeletionQueue.Count > 0) {
                Node nodeTodelete = nodeDeletionQueue.Dequeue();
                nodeTodelete.NotifyConnectedToRemove();
                graph.nodes.Remove(nodeTodelete);
                DestroyImmediate(nodeTodelete, true);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Create Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        private void InitialiseNode (object mousePositionObject, Node node, string nodeName) {
            Vector2 mousePosition = (Vector2)mousePositionObject;
            graph.nodes.Add(node);
            node.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), nodeName, graph);
            AssetDatabase.AddObjectToAsset(node, graph);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Chance current node graph and draw the new one
        /// </summary>
        private void ChangeEditorWindowOnSelection () {
            DialogNodeGraph nodeGraph = Selection.activeObject as DialogNodeGraph;
            if (nodeGraph != null) {
                graph = nodeGraph;
                GUI.changed = true;
            }
        }
    }
}
