using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KulibinSpace.DialogSystem;

public class TestRunner : MonoBehaviour {

    public DialogNodeGraph dialog;

    void Start () {
        DialogNodeRunner runner = new();
        runner.Init(dialog);
        while (runner.node != null) {
            if (runner.node is SentenceNode snode) {
                print("Sentence: " + snode.GetSentenceText());
            } else if (runner.node is AnswerNode anode) {
                print("Answers: " + string.Join(", ", anode.Answers));
            }
            runner.Next();
        }
    }

}
