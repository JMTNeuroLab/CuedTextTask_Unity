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

        // My Slack message might not have been clear: 
        // { } brackets are for definition of lists/arrays
        // ( ) parentheses (i.e. Add ( ) ) are for execution functions from the array/list class
        // They can't be combined at once. 
        List<List<GameObject>> pairs = new List<List<GameObject>>()
        {
                new List<GameObject>{ taskInfo.StartPositions[0],taskInfo.CueObjects[0], taskInfo.TargetObjects[0]},
                new List<GameObject>{ taskInfo.StartPositions[1],taskInfo.CueObjects[1], taskInfo.TargetObjects[1]},
                new List<GameObject>{ taskInfo.StartPositions[2],taskInfo.CueObjects[2], taskInfo.TargetObjects[2]},
                new List<GameObject>{ taskInfo.StartPositions[3],taskInfo.CueObjects[3], taskInfo.TargetObjects[3]},
                new List<GameObject>{ taskInfo.StartPositions[4],taskInfo.CueObjects[4], taskInfo.TargetObjects[4]},
                new List<GameObject>{ taskInfo.StartPositions[5],taskInfo.CueObjects[5], taskInfo.TargetObjects[5]},
                new List<GameObject>{ taskInfo.StartPositions[6],taskInfo.CueObjects[6], taskInfo.TargetObjects[6]},
                new List<GameObject>{ taskInfo.StartPositions[7],taskInfo.CueObjects[7], taskInfo.TargetObjects[7]},
                new List<GameObject>{ taskInfo.StartPositions[8],taskInfo.CueObjects[8], taskInfo.TargetObjects[8]},
                new List<GameObject>{ taskInfo.StartPositions[9],taskInfo.CueObjects[9], taskInfo.TargetObjects[9]}
        };

        foreach (List<GameObject> l in pairs)
        { 
            // At this point we have everything. Add to trial list N times:
            for (int ii = 0; ii < taskInfo.NumberOfSets; ii++)
            {

                //Remove targets from Distractors, they'll be added back at the end
                distractors.Remove(l[1]);  // Cue Object
                distractors.Remove(l[2]);  // House Object
                distractors_positions.Remove(l[1].transform.position);
                distractors_positions.Remove(l[2].transform.position);

                // here both () and {} are combined because we are defining a new TrialData{ ... } and ADDing () it to the _allTrials List 
                _allTrials.Add(
                    new TrialData
                    {
                        Trial_Number = 0,
                        Start_Position = l[0].transform.position,
                        Start_Rotation = l[0].transform.rotation.eulerAngles,
                        // Fix point
                        Fix_Object = null,
                        // Subracting the camera position to get the fixation point local space (i.e. child of camera)
                        Fix_Position_World = Vector3.zero,
                        Fix_Position_Screen = Vector3.zero,
                        Fix_Size = 0,
                        Fix_Window = taskInfo.FixationWindow,

                        // Current Cue
                        Cue_Objects = new GameObject[] { l[1] },
                        Cue_Material = null,

                        // Targets
                        Target_Objects = new GameObject[] { l[2] },
                        Target_Materials = null,
                        Target_Positions = new Vector3[] { l[2].transform.position },
                        MultipleTargets = taskInfo.MultipleTargets,
                        MultipleRewardsScale = taskInfo.MultipleRewardsScale,

                        Distractor_Objects = distractors.ToArray(),
                        Distractor_Materials = null,
                        Distractor_Positions = distractors_positions.ToArray()
                    });
                // Add objects back to distractors
                distractors.Add(l[1]);
                distractors.Add(l[2]);
                distractors_positions.Add(l[1].transform.position);
                distractors_positions.Add(l[2].transform.position);
            }
                
        }
        // shuffle trials
        _allTrials = _allTrials.OrderBy(x => UnityEngine.Random.value).ToList();
        Debug.Log("Generated :" + _allTrials.Count + " trials. " + (_allTrials.Count / taskInfo.NumberOfSets) + " of which are different.");
    }

    // Since this is where the trial number gets incremented for every trial (i.e. _trialNumber++;), we can
    // use this function to control whether the subject moves ahead in the trial list or whether they have to 
    // re-do the previous trial (e.g. wrong object or house). 
    // This will refer to the trial position in the _allTrials list. We need to increment trial number to have 
    // a unique identifier for all of them but only increment the _allTrialsID if the previous trial was a 
    // success. 
    private int _allTrialsID = -1; 
    public override void PrepareTrial()
    {
        // only increment _allTrialsID if the previous trial was a success.
        // At the start of the first trial, the "previous trial" is set as a hit so 
        // we need to start with a negative ID
        if (_previousTrialError == 0)
            _allTrialsID++;

        // Stop experiment once all trials are done. 
        if (_allTrialsID == _allTrials.Count)
        {
            StopExperiment();
        }

        // get current trial
        _currentTrial = _allTrials[_allTrialsID];
        
        // increment trial counter regardless of performance
        _trialNumber++;
        _currentTrial.Trial_Number = _trialNumber;

        // Prepare cues and targets. 
        HideFixationObject();
        PrepareFixationObject();
        HideCues();
        PrepareCues();
        HideTargets();
        PrepareTargets();
        HideDistractors();
        PrepareDistractors();

        //teleport player to the start position
        if (!taskInfo.ContinuousTrials)
        {
            playerController.ToStart(_currentTrial.Start_Position, _currentTrial.Start_Rotation);
        }

        // Sanity checks
        TrialEnded = false;
        Outcome = "aborted";
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

    // Cue don't move nor change color
    public override void PrepareCues()
    {
        // Detach objects
        foreach (GameObject go in taskInfo.CueObjects)
        {
            if (go.TryGetComponent(out ObjectPickUp obpu))
            {
                obpu.Detach(false);

                // if the cue objects are incorrect, prevent pick up sound
                obpu.ToggleSound(!_currentTrial.Distractor_Objects.Contains(go));
            }
        }
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
        string cueString = textcue_taskinfo.BaseCueString.Replace("{object}", _currentTrial.Cue_Objects[0].name.ToString());
        cueString = cueString.Replace("{target}", _currentTrial.Target_Objects[0].name.ToString());
        StartCoroutine(CueTextTimer(cueString, textcue_taskinfo.TextCueDuration, Color.black));

        // here the objects appear, so we need to set the feedbackstring to be "wrong object" 
        // in case of error trials
        FeedbackString = "Wrong object";
    }

    public override void ShowTargets()
    {
        // we need to update the FeedbackString, at this point the corect object has to
        // be selected. 
        FeedbackString = "Wrong house";
        base.ShowTargets();
    }

    public override void ShowFeedback()
    {
        float duration = 1.5f;
        if (_previousTrialError == 0) // Correct
            StartCoroutine(CueTextTimer("Correct", duration, Color.green));
        else if (_previousTrialError ==1) // Incorrect
            StartCoroutine(CueTextTimer(FeedbackString, duration, Color.red));
        else // 2 time run out / ignored
            StartCoroutine(CueTextTimer("Time run out", duration, Color.red));
    }

    IEnumerator CueTextTimer(string cueString, float duration, Color c)
    {
        textcue_taskinfo.cueTextBox.text = cueString;
        textcue_taskinfo.cueTextBox.color = c;
        yield return new WaitForSecondsRealtime(duration);
        textcue_taskinfo.cueTextBox.text = "";
    }

}
