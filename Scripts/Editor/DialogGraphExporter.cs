#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace KulibinSpace.DialogSystem {

    public class DialogGraphExporter : EditorWindow {
        private DialogNodeGraph graph;
        private string exportPath = "Assets/dialog_export.json";

        [MenuItem("Tools/Dialog Graph/Export & Import")]
        private static void ShowWindow () {
            GetWindow<DialogGraphExporter>("Dialog Graph Export/Import");
        }

        private void OnGUI () {
            graph = EditorGUILayout.ObjectField("Graph", graph, typeof(DialogNodeGraph), false) as DialogNodeGraph;
            exportPath = EditorGUILayout.TextField("JSON Path", exportPath);

            if (GUILayout.Button("Export to JSON")) {
                if (graph != null)
                    ExportGraph(graph, exportPath);
            }

            if (GUILayout.Button("Import from JSON")) {
                if (graph != null)
                    ImportGraph(graph, exportPath);
            }
        }

        [System.Serializable]
        public class ExportedDialogGraph {
            public string characterName;
            public List<ExportedNode> nodes = new();
        }

        [System.Serializable]
        public class ExportedNode {
            public string guid;
            public string type;
            public string text;
            public Dictionary<string, string> localized;
            public List<ExportedAnswer> answers;
        }

        [System.Serializable]
        public class ExportedAnswer {
            public string text;
            public Dictionary<string, string> localized;
        }

        private static void ExportGraph (DialogNodeGraph graph, string path) {
            var export = new ExportedDialogGraph { characterName = graph.characterName };

            foreach (var node in graph.nodes) {
                var nodeData = new ExportedNode {
                    guid = ((Node)node).Guid,
                    type = node.GetType().Name
                };

                if (node is SentenceNode sentenceNode) {
                    nodeData.text = sentenceNode.GetSentenceText();
                    nodeData.localized = GetAllLocales(sentenceNode.stringRef);
                } else if (node is AnswerNode answerNode) {
                    var answerList = new List<ExportedAnswer>();
                    foreach (var ans in answerNode.GetType().GetField("answers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(answerNode) as List<Answer>) {
                        answerList.Add(new ExportedAnswer {
                            text = ans.answer,
                            localized = GetAllLocales(ans.stringRef)
                        });
                    }
                    nodeData.answers = answerList;
                }

                export.nodes.Add(nodeData);
            }

            File.WriteAllText(path, JsonUtility.ToJson(export, true));
            Debug.Log($"Graph exported to: {path}");
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, string> GetAllLocales (LocalizedString stringRef) {
            var result = new Dictionary<string, string>();
            var collection = LocalizationEditorSettings.GetStringTableCollection(stringRef.TableReference);
            if (collection == null) return result;

            foreach (var table in collection.StringTables) {
                if (table == null) continue;
                //var entry = table.GetEntry(stringRef.TableEntryReference); // 2025-07-14 20:20:43 замена
                var entryRef = stringRef.TableEntryReference;
                StringTableEntry entry;
                if (entryRef.ReferenceType == TableEntryReference.Type.Name) {
                    entry = table.GetEntry(entryRef.ToString()); // ← вернёт string ключ
                } else {
                    entry = table.GetEntry(entryRef.KeyId);      // ← вернёт по числовому ID
                }
                // конец замены
                if (entry != null) {
                    result[table.LocaleIdentifier.Code] = entry.LocalizedValue;
                }
            }
            return result;
        }

        private static void ImportGraph (DialogNodeGraph graph, string path) {
            var json = File.ReadAllText(path);
            var import = JsonUtility.FromJson<ExportedDialogGraph>(json);
            graph.characterName = import.characterName;

            foreach (var nodeData in import.nodes) {
                var node = graph.nodes.OfType<Node>().FirstOrDefault(n => n.Guid == nodeData.guid);
                if (node == null) {
                    Debug.LogWarning($"Node with guid {nodeData.guid} not found in graph.");
                    continue;
                }

                if (node is SentenceNode sentenceNode) {
                    sentenceNode.SetSentenceText(nodeData.text);
                    ApplyLocales(sentenceNode.stringRef, nodeData.localized, node.Guid);
                } else if (node is AnswerNode answerNode && nodeData.answers != null) {
                    var answersField = typeof(AnswerNode).GetField("answers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var answers = answersField.GetValue(answerNode) as List<Answer>;
                    for (int i = 0; i < answers.Count && i < nodeData.answers.Count; i++) {
                        var ans = answers[i];
                        ans.answer = nodeData.answers[i].text;
                        ApplyLocales(ans.stringRef, nodeData.answers[i].localized, node.Guid + "_a" + i);
                        answers[i] = ans;
                    }
                    answersField.SetValue(answerNode, answers);
                }

                EditorUtility.SetDirty(node);
            }

            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            Debug.Log("Graph imported from JSON.");
        }

        private static void ApplyLocales (LocalizedString stringRef, Dictionary<string, string> localizedValues, string uniqueKey) {
            var collection = LocalizationEditorSettings.GetStringTableCollection(stringRef.TableReference);
            if (collection == null) return;

            string entryKey = $"node_{uniqueKey}";
            stringRef.TableEntryReference = entryKey;

            foreach (var table in collection.StringTables) {
                if (localizedValues.TryGetValue(table.LocaleIdentifier.Code, out var value)) {
                    var entry = table.GetEntry(entryKey) ?? table.AddEntry(entryKey, value);
                    entry.Value = value;
                    EditorUtility.SetDirty(table);
                }
            }
        }
    }

}

#endif
