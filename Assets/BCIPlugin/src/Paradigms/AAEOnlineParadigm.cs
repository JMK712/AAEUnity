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
    // private GameObject obj;
    private Queue<String> cmdQueue = new Queue<String>();
    
    // Start is called before the first frame update
    void Start()
    {
        NetService.Instance.AddDistributer(cmd =>
        {
            lock (cmdQueue)
            {
                cmdQueue.Enqueue(cmd);
            }
        });
        Player = GameObject.Find("PlayerArmature");
        ui = GameObject.FindWithTag("BCIPlugin").GetComponent<UIBCIPlugin>();
        // obj = Resources.Load<GameObject>("Trial");
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
            // var trialGameObj = Instantiate(obj, new Vector3(0, 0, 0), Quaternion.identity);
            // currentTrial = trialGameObj.GetComponent<Trial>();
            currentTrial = new Trial(roads[int.Parse(messages[1])]);
            Debug.Log("trial started");
            //start coroutine to set volume by interval
            StartCoroutine(SetVolume());  //use a static var in trial to update volume
        }
        else if (messages[0] == "TrialCmd")
        {
            if (currentTrial != null)
            {
                currentTrial.HandleCmd(messages);  //then give it to HandleCmd method of current trial
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
        lock (cmdQueue)
        {
            while (cmdQueue.Count > 0)
            {
                HandleCmd(cmdQueue.Dequeue());
            }
        }

        if (currentTrial == null)
        {
            ui.textMain.text = "Click to Start New Trial";
        }
        else if (!currentTrial.Update())
        {
            Debug.Log("Trial End");
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

    private IEnumerator SetVolume()
    {
        while (true)
        {
            var volume = Trial.volume;
            yield return new WaitForSeconds(1f);
            if (volume == 0)
            {
                Debug.Log("Volume is zero or None");    
            }
            else
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
