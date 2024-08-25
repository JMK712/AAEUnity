using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barmetler.RoadSystem;
using UnityEditor.UI;
using UnityEngine.Networking;

public class AAEOnlineParadigm : MonoBehaviour
{
    [SerializeField] private List<Road> roads; // all possibly road objects
    public UIBCIPlugin ui;
    public GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        NetService.Instance.AddDistributer(HandleCmd);
        Player = GameObject.Find("PlayerArmature");
        ui = GameObject.FindWithTag("BCIPlugin").GetComponent<UIBCIPlugin>();
        // HandleCmd("StartTrial_0");  // TODO for debug!!!!!
        Trial.EnableUI();
    }

    //#######################

    private Trial currentTrial = null;

    public void HandleCmd(string msg)
    {
        Debug.Log(msg);
        
        string[] messages = msg.Split('_');

        if (messages[0] == "StartTrial")
        {
            Debug.Log("new a trail: Road "+ int.Parse(messages[1]));
            currentTrial = new Trial(roads[int.Parse(messages[1])]);
            Debug.Log("trial started");
            //start coroutine to set volume by interval
            StartCoroutine(SetVolume(Trial.volume));  //use a static var in trial to update volume
        }
        else if (messages[0] == "TrialCmd")
        {
            if (currentTrial != null)
            {
                var trialCmd = new string[messages.Length - 1];
                messages.CopyTo(trialCmd, 1);  //take down the first phrase of original message , copy the rest as a new array
                currentTrial.HandleCmd(trialCmd);  //then give it to HandleCmd method of current trial
                string result = string.Join(" ", trialCmd);
                Debug.Log("TrialCmd: " + result);
            }
            else
            {
                Debug.Log("Skipping trial command: " + messages);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (currentTrial == null)
        {
            ui.textMain.text = "Click to Start New Trial";
        }
        else if (!currentTrial.Update())
        {
            
            NetService.Instance.SendMessage("TrialEnd");  //tell python that a trial has end
            currentTrial.Report();
            Trial.EnableUI();
            currentTrial = null;
            if (PlayDataCollector.IsOnEndPoint)
            {
                Player.transform.position = new Vector3(490.26f,2.045f,13.58f);
                Player.transform.rotation = new Quaternion(0,270,0,1);
            }
        }
    }

    private IEnumerator SetVolume(float volume)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (volume == 0)
            {
                Debug.Log("Volume is zero or None");    
            }
            else if (volume != 0)
            {
                Player.GetComponent<AudioSource>().volume = volume;
                Debug.Log("Volume is set to: " + volume);
            }
        }
    }
    
    public void SetTrack(int trackCode)
    {
        var audioSource = Player.GetComponent<AudioSource>();
        if (trackCode == -1)
        {
            audioSource.mute = true;
            return;  // exit method
        }
        string fileName = trackCode.ToString();  //n.wav
        AudioClip clip = Resources.Load<AudioClip>(fileName);
        audioSource.clip = clip;
        audioSource.Play();
    }
    //#########################
}
