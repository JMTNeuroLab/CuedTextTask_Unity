using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TextCue_TaskInfo : TaskInfo
{
    // Here we can add new task parameters
    [Header("Text Cue")]
    public string BaseCueString = "Find the {object}, and bring it to the {target} house.";
    public float TextCueDuration = 3.0f;
    public Text cueTextBox;

    private void OnValidate()
    {
        // Call the validate function of base-class TaskInfo.
        // Note that we do not use base.OnValidate() as this function
        // is not "virtual" so we are not "overridding" it. 
        BaseValidate();
    }
}
