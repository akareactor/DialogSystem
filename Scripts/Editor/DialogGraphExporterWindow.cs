// Editor script for exporting/importing DialogNodeGraph to JSON with localized strings
// Put this script into an Editor folder

using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;


namespace KulibinSpace.DialogSystem {

    public class DialogGraphExporterWindow : EditorWindow {
        private DialogNodeGraph graph;
        private string fileName = "dialog_graph_export.json";
        private Locale selectedLocale;
        private StringTableCollection selectedTable;
        private bool generateLocalization = true;
        private TextAsset jsonFile; // JSON-файл с диалогом

        [MenuItem("Tools/Kulibin.Space/Dialog Graph Exporter")]
        public static void OpenWindow () {
            GetWindow<DialogGraphExporterWindow>("Dialog Graph Exporter");
        }

        private void OnGUI () {
            GUILayout.Label("Export / Import DialogNodeGraph", EditorStyles.boldLabel);
            graph = (DialogNodeGraph)EditorGUILayout.ObjectField("Graph Asset", graph, typeof(DialogNodeGraph), false);
            if (graph == null)
                return;
            fileName = EditorGUILayout.TextField("JSON File Name", graph.name + ".json");
            generateLocalization = EditorGUILayout.Toggle("Генерировать локализацию", generateLocalization);
            var locales = LocalizationEditorSettings.GetLocales();
            if (locales.Count == 0) {
                EditorGUILayout.HelpBox("No locales found in Localization Settings.", MessageType.Warning);
                return;
            }
            var tableCollections = LocalizationEditorSettings.GetStringTableCollections();
            var tableNames = tableCollections.Select(c => c.TableCollectionName).ToList();
            int selectedIndex = selectedTable != null ? tableNames.IndexOf(selectedTable.TableCollectionName) : 0;
            selectedIndex = EditorGUILayout.Popup("Table", selectedIndex, tableNames.ToArray());
            selectedTable = tableCollections[selectedIndex];
            int localeIndex = selectedLocale != null ? locales.IndexOf(selectedLocale) : 0;
            localeIndex = EditorGUILayout.Popup("Primary Locale", localeIndex, locales.Select(l => l.Identifier.Code).ToArray());
            selectedLocale = locales[localeIndex];
            GUILayout.Space(10);
            if (GUILayout.Button("Export JSON")) {
                ExportGraphToJson();
            }
            // Поле для JSON-файла
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON-файл для импорта", jsonFile, typeof(TextAsset), false);
            if (GUILayout.Button("Import JSON")) {
                ImportGraphFromJson(jsonFile);
            }
        }

        [Serializable]
        private class LocalizedEntry {
            public string locale;
            public string text;
        }

        [Serializable]
        private class SentenceNodeData {
            public string guid;
            public string text;
            public string key;
            public List<LocalizedEntry> localized = new();
        }

        [Serializable]
        private class AnswerData {
            public string text;
            public string key;
            public List<LocalizedEntry> localized = new();
        }

        [Serializable]
        private class AnswerNodeData {
            public string guid;
            public List<AnswerData> answers = new();
        }

        [Serializable]
        private class GraphExportData {
            public string characterName;
            public List<SentenceNodeData> sentences = new();
            public List<AnswerNodeData> answers = new();
        }

        // human-readable key!
        // Not checks if key existing
        string Key (LocalizedString stringRef) {
            if (!stringRef.IsEmpty) {
                return stringRef.TableEntryReference.ResolveKeyName(selectedTable.SharedData);
            } else {
                return "";
            }
        }

        string GetKeyName (LocalizedString stringRef) {
            if (stringRef == null || stringRef.IsEmpty)
                return "";

            // Получаем коллекцию таблиц по ссылке
            var collection = LocalizationEditorSettings.GetStringTableCollection(stringRef.TableReference);
            if (collection == null)
                return "";

            var sharedData = collection.SharedData;
            if (sharedData == null)
                return "";

            // Пытаемся получить ключ по ID
            var entry = sharedData.GetEntry(stringRef.TableEntryReference.KeyId);
            return entry != null ? entry.Key : "";
        }

        private void ExportGraphToJson () {
            var data = new GraphExportData();
            data.characterName = GetKeyName(graph.stringRef);
            if (data.characterName == "") data.characterName = string.IsNullOrWhiteSpace(graph.characterName) ? "character" : graph.characterName.Replace(" ", "");
            var tableCollection = selectedTable;
            foreach (var node in graph.nodes) {
                if (node is SentenceNode sentenceNode) {
                    var key = Key(sentenceNode.stringRef);
                    if (key == "" && generateLocalization) key = $"@{data.characterName}#{sentenceNode.Guid}";
                    var sentenceData = new SentenceNodeData {
                        guid = sentenceNode.Guid,
                        text = sentenceNode.RawText,
                        key = key,
                    };
                    if (generateLocalization) {
                        EnsureEntryInTable(tableCollection, selectedLocale, key, sentenceNode.RawText);
                        foreach (var locale in LocalizationEditorSettings.GetLocales()) {
                            var entry = GetOrCreateEntry(tableCollection, locale, key);
                            sentenceData.localized.Add(new LocalizedEntry {
                                locale = locale.Identifier.Code,
                                text = entry?.LocalizedValue ?? ""
                            });
                        }
                    }
                    data.sentences.Add(sentenceData);
                } else if (node is AnswerNode answerNode) {
                    var answerData = new AnswerNodeData {
                        guid = answerNode.Guid,
                    };
                    var answers = answerNode.RawAnswers;
                    for (int i = 0; i < answers.Count; i++) {
                        var key = GetKeyName(answerNode.answers[i].stringRef);
                        if (key == "" && generateLocalization) key = $"@{data.characterName}_answer_{i}_#{answerNode.Guid}";
                        var answer = answers[i];
                        var localized = new AnswerData {
                            text = answer,
                            key = key,
                            localized = new List<LocalizedEntry>()
                        };
                        if (generateLocalization) {
                            EnsureEntryInTable(tableCollection, selectedLocale, key, answer);
                            foreach (var locale in LocalizationEditorSettings.GetLocales()) {
                                var entry = GetOrCreateEntry(tableCollection, locale, key);
                                localized.localized.Add(new LocalizedEntry {
                                    locale = locale.Identifier.Code,
                                    text = entry?.LocalizedValue ?? ""
                                });
                            }
                        }
                        answerData.answers.Add(localized);
                    }
                    data.answers.Add(answerData);
                }
            }
            EditorUtility.SetDirty(tableCollection);
            AssetDatabase.SaveAssets();
            var json = JsonUtility.ToJson(data, true);
            // write json to a same folder
            string assetPath = AssetDatabase.GetAssetPath(graph);
            string dir = Path.GetDirectoryName(assetPath);
            string filePath = Path.Combine(dir, fileName);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            AssetDatabase.Refresh();
            // Автоматически найдём и подставим JSON в поле
            jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            Debug.Log($"Exported to {fileName}");
            if (generateLocalization) {
                Debug.Log($"Localization keys generated at {selectedTable.TableCollectionName}");
            }
        }

        private void ImportGraphFromJson (TextAsset file) {
            if (file == null) {
                Debug.LogWarning("JSON-файл не выбран.");
                return;
            }

            var data = JsonUtility.FromJson<GraphExportData>(file.text);
            int addedSentences = 0;
            int updatedSentences = 0;
            int addedAnswers = 0;
            int updatedAnswers = 0;

            foreach (var sentence in data.sentences) {
                var node = graph.nodes.OfType<SentenceNode>().FirstOrDefault(n => n.Guid == sentence.guid);
                if (node != null) {
                    // 1) Всегда обновляем обычный текст
                    node.SetSentenceText(sentence.text);
                    // 2) Обновляем ключ в stringRef, если он указан
                    if (!string.IsNullOrEmpty(sentence.key)) {
                        node.stringRef.TableReference = selectedTable.TableCollectionName;
                        node.stringRef.TableEntryReference = sentence.key;
                        // 3) Обновляем локализации по ключу
                        foreach (var loc in sentence.localized) {
                            var table = selectedTable.GetTable(new LocaleIdentifier(loc.locale)) as StringTable;
                            if (table == null) continue;
                            var entry = table.GetEntry(sentence.key);
                            if (entry == null) {
                                entry = table.AddEntry(sentence.key, loc.text); addedSentences += 1;
                            } else {
                                entry.Value = loc.text; updatedSentences += 1;
                            }
                            EditorUtility.SetDirty(table);
                        }
                    }
                }
            }

            foreach (var answer in data.answers) {
                var node = graph.nodes.OfType<AnswerNode>().FirstOrDefault(n => n.Guid == answer.guid);
                if (node != null) {
                    for (int i = 0; i < answer.answers.Count; i++) {
                        if (i >= node.answers.Count)
                            continue;
                        var ans = answer.answers[i];
                        // 1) Обновляем обычный текст
                        var a = node.answers[i];
                        a.answer = ans.text;
                        // 2) Обновляем stringRef, если указан ключ
                        if (!string.IsNullOrEmpty(ans.key)) {
                            a.stringRef = new LocalizedString {
                                TableReference = selectedTable.TableCollectionName,
                                TableEntryReference = ans.key
                            };
                            // 3) Обновляем локализации по ключу
                            foreach (var loc in ans.localized) {
                                var table = selectedTable.GetTable(new LocaleIdentifier(loc.locale)) as StringTable;
                                if (table == null) continue;
                                var entry = table.GetEntry(ans.key);
                                if (entry == null) {
                                    entry = table.AddEntry(ans.key, loc.text); addedAnswers += 1;
                                } else {
                                    entry.Value = loc.text; updatedAnswers += 1;
                                }
                                EditorUtility.SetDirty(table);
                            }
                        }
                        node.answers[i] = a;
                    }
                }
            }
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported from {fileName}");
            Debug.Log($"Localized sentences added {addedSentences}, updated {updatedSentences}");
            Debug.Log($"Localized answers added {addedAnswers}, updated {updatedAnswers}");
        }

        private void EnsureEntryInTable (StringTableCollection collection, Locale locale, string key, string defaultValue) {
            var table = collection.GetTable(locale.Identifier) as StringTable;
            if (table == null) return;
            var entry = table.GetEntry(key);
            if (entry == null) {
                table.AddEntry(key, defaultValue);
            }
        }

        private StringTableEntry GetOrCreateEntry (StringTableCollection collection, Locale locale, string key) {
            var table = collection.GetTable(locale.Identifier) as StringTable;
            return table.GetEntry(key);
        }
    }

}
