using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TextCue_ExpCtrl : ExperimentController
{
    // The taskInfo base class is defined as "dynamic" in the experiment
    // controller class, this means that the class is implemented at runtime
    // we therefore need to define it now. 
    // We use a specific sub-class of task info and set it as "taskInfo" so
    // all the base scripts will still work. 
    public TextCue_TaskInfo textcue_taskinfo;
    
    // This function allows the base-class Experiment Controller to 
    // access values defined here (e.g. the IsNorth boolean). The returned value 
    // needs to be converted into the proper format with (format) before: 
    // bool ReturnedIsNorth = (bool)returnValue("IsNorth");
    public override object ReturnValue(string name)
    {
        return GetType().GetProperty(name).GetValue(this);
    }

    // OnEnable and OnDisable function calls are important because they connect the
    // experiment controller with the events controller for communication across classes. 
    private new void OnEnable()
    {
        // Important, do not remove ---
        base.OnEnable();
        taskInfo = textcue_taskinfo;
        // ----------------------------
    }

    private void Start()
    {
        // Base function call. Do not remove. 
        base.Initialize();
    }

    // Overriding base experiment controller functions
    // You need a special function to prepare all trials because you won't be using most
    // of the default settings. 
    public override void PrepareAllTrials()
    {
        // set the seed
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        // Clear trial list
        _allTrials = new List<TrialData>();

        // To run this task with the current framework we'll try using the CueObjects as the objects to 
        // pick-up. And the TargetObjects as the actual targets to navigate to (i.e. houses). 
        // We will define the current trials Targets as a combination of Object-House and set the trial as
        // having "MultipleTargets". 
        // The Objects will be made visible/interactable first, when the subject hits one of them it can either: 
        //  - Be correct: trigger the onset of the House triggers (i.e. TargetOnset)
        //  - Be Incorrect: trigger the end of the trial (i.e. all other objects and houses will be set as distractors). 

        // To generate the list of all trials, we will take each object and iterate through all possible houses. Since there will
        // always be only one object and one house per trial, we don't need a lot of options from the TaskInfo, such as: 
        //      - Number of targets/distractors
        //      - Possible target positions (i.e. house positions are static)
        //      - Conditions
        //      - Materials
        //      - ...

        // All objects are distractors
        List<GameObject> distractors = new List<GameObject>();
        List<Vector3> distractors_positions = new List<Vector3>();
        foreach (GameObject go in taskInfo.CueObjects)
        {
            distractors.Add(go);
            distractors_positions.Add(go.transform.position);
        }
        foreach (GameObject go in taskInfo.TargetObjects)
        {
            distractors.Add(go);
            distractors_positions.Add(go.transform.position);
        }

        // Loop for start positions (only 1 per trial?)
        for (int start_index = 0; start_index < taskInfo.StartPositions.Length; start_index++)
        {
            for (int object_index = 0; object_index < taskInfo.CueObjects.Length; object_index++)
            {
                for (int house_index = 0; house_index < taskInfo.TargetObjects.Length; house_index++)
                {
                    // At this point we have everything. Add to trial list N times:
                    for (int ii = 0; ii < taskInfo.NumberOfSets; ii++)
                    {

                        //Remove targets from Distractors, they'll be added back at the end
                        distractors.Remove(taskInfo.CueObjects[object_index]);
                        distractors.Remove(taskInfo.TargetObjects[house_index]);
                        distractors_positions.Remove(taskInfo.CueObjects[object_index].transform.position);
                        distractors_positions.Remove(taskInfo.TargetObjects[house_index].transform.position);

                        _allTrials.Add(
                            new TrialData
                            {
                                Trial_Number = 0,
                                Start_Position = taskInfo.StartPositions[start_index].transform.position,
                                Start_Rotation = taskInfo.StartPositions[start_index].transform.rotation.eulerAngles,
                                // Fix point
                                Fix_Object = null,
                                // Subracting the camera position to get the fixation point local space (i.e. child of camera)
                                Fix_Position_World = Vector3.zero,
                                Fix_Position_Screen = Vector3.zero,
                                Fix_Size = 0,
                                Fix_Window = taskInfo.FixationWindow,

                                // Current Cue
                                Cue_Objects = new GameObject[] { taskInfo.CueObjects[object_index] },
                                Cue_Material = null,

                                // Targets
                                Target_Objects = new GameObject[] { taskInfo.TargetObjects[house_index] },
                                Target_Materials = null,
                                Target_Positions = new Vector3[] { taskInfo.TargetObjects[house_index].transform.position },
                                MultipleTargets = taskInfo.MultipleTargets,
                                MultipleRewardsScale = taskInfo.MultipleRewardsScale,

                                Distractor_Objects = distractors.ToArray(),
                                Distractor_Materials = null,
                                Distractor_Positions = distractors_positions.ToArray()
                            });
                        distractors.Add(taskInfo.CueObjects[object_index]);
                        distractors.Add(taskInfo.TargetObjects[house_index]);
                        distractors_positions.Add(taskInfo.CueObjects[object_index].transform.position);
                        distractors_positions.Add(taskInfo.TargetObjects[house_index].transform.position);
                    }
                }
            }
        }
        // shuffle trials
        _allTrials = _allTrials.OrderBy(x => UnityEngine.Random.value).ToList();
        Debug.Log("Generated :" + _allTrials.Count + " trials. " + (_allTrials.Count / taskInfo.NumberOfSets) + " of which are different.");
    }

    public override void HideCues()
    {
        // Clear text box and disable cue objects
        foreach (GameObject go in taskInfo.CueObjects)
        {
            go.GetComponent<Collider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }

    }

    // Cue don't move nor change color so nothing to do. 
    public override void PrepareCues()
    {
    }


    public override void PrepareTargets()
    {
        // targets are static in terms of location, material,... 

        // create array to keep track of hit targets
        _trialTargetHits = new bool[_currentTrial.Target_Objects.Length];
    }

    public override void PrepareDistractors()
    {
        // Same as cues and targets, nothing to do. 
    }

    // we need to override the show cues function since we need to :
    // Display the text cue and turn on collisions on all the cue objects
    // of note, the incorrect cue objects are found in the distractors list so 
    // they should trigger an error trial.
    public override void ShowCues()
    {
        foreach (GameObject go in taskInfo.CueObjects)
        {
            go.GetComponent<Collider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
        StartCoroutine(CueTextTimer());
    }

    IEnumerator CueTextTimer()
    {
        string cueString = textcue_taskinfo.BaseCueString.Replace("{object}", _currentTrial.Cue_Objects[0].name.ToString());
        cueString = cueString.Replace("{target}", _currentTrial.Target_Objects[0].name.ToString());
        textcue_taskinfo.cueTextBox.text = cueString;

        yield return new WaitForSecondsRealtime(textcue_taskinfo.TextCueDuration);
        textcue_taskinfo.cueTextBox.text = "";
    }

}
