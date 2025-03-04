using System.Collections.Generic;
using UnityEngine;

namespace KulibinSpace.DialogSystem {

    public class DialogNodeRunner {

        DialogNodeGraph graph;
        public Node node;

        /// <summary>
        /// Prepare dialog tree for reading, set up a first sentences
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        public void Init (DialogNodeGraph dialogNodeGraph) {
            if (dialogNodeGraph == null || dialogNodeGraph.nodes == null || dialogNodeGraph.nodes.Count == 0) {
                Debug.LogWarning("Dialog Graph is empty");
            } else {
                graph = dialogNodeGraph;
                ReadFirst();
            }
        }

        // First read node must be Sentence type only
        void ReadFirst () {
            List<SentenceNode> sentences = new();
            // select all Sentence Nodes with no parent nodes and none or some child nodes
            foreach (Node node in graph.nodes) {
                if (node is SentenceNode snode) {
                    if (snode.parentNodes == null || snode.parentNodes.Count == 0) {
                        sentences.Add(snode);
                    }
                }
            }
            if (sentences.Count > 0) {
                node = sentences[Random.Range(0, sentences.Count)];
            }
        }

        public Node Next () {
            if (node != null) {
                if (node is SentenceNode snode) {
                    if (snode.childNodes.Count > 0) {
                        node = snode.childNodes[Random.Range(0, snode.childNodes.Count)];
                    } else {
                        node = null;
                    }
                } else if (node is AnswerNode anode) {
                    if (anode.childSentenceNodes.Count > 0) {
                        node = anode.childSentenceNodes[Random.Range(0, anode.childSentenceNodes.Count)];
                    } else {
                        node = null;
                    }
                }
            }
            return node;
        }

    }

}
