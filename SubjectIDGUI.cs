using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubjectIDGUI : MonoBehaviour
{
    public Text txt_Label;
    public Button bt_Go;
    public InputField txt_ID;
    private Canvas cv;
    private string _participantID;

    // Start is called before the first frame update
    void Start()
    {
        cv = gameObject.GetComponent<Canvas>();
        bt_Go.onClick.AddListener(OnGoClicked);
    }
    public void DisplayCompletionCode()
    {
        cv.enabled = true;
        txt_Label.text = "Your completion code is: ";
        txt_ID.text = _participantID;
        txt_ID.enabled = false;
        bt_Go.GetComponentInChildren<Text>().text = "Quit";
        bt_Go.onClick.RemoveAllListeners();
        bt_Go.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        Application.Quit();
    }

    private void OnGoClicked()
    {
        string tmp_txt = txt_ID.text;
        char[] reversed = new char[tmp_txt.Length];
        for (int i = 0; i < tmp_txt.Length; i++)
        {
            reversed[i] = tmp_txt[tmp_txt.Length - 1 - i];
        }
        tmp_txt = Hash128.Compute(new string(reversed)).ToString();
        _participantID = tmp_txt.Substring(tmp_txt.Length - 10, 10);
        cv.enabled = false;
        EventsController.instance.SendBegin();

    }

}
