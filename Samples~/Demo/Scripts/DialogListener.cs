using KulibinSpace.DialogSystem;
using UnityEngine;

public class DialogListener : MonoBehaviour {

    public DialogController controller;

    void OnEnable () => controller.dialogStopped += OnDialogStopped;
    void OnDisable () => controller.dialogStopped -= OnDialogStopped;

    void OnDialogStopped () {
        print("Dialog was stopped!");
    }

}
