using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Barmetler.RoadSystem;
using StarterAssets;

public class AAEOnlineParadigm : MonoBehaviour
{
    public delegate void Handler(int i);
    public Handler TrialStartHandler;
    public Handler TrialEndHandler;
    public Handler RequestTrackHandler;
    
    [SerializeField] private List<Road> roads; // 所有可能的道路对象
    
    public UIBCIPlugin ui;
    // public RoadProgressTracker RoadPT;

    public Dictionary<int, AAEtype> AAEtable;
    public Dictionary<AAEtype, bool> AAEconfig;

    // Start is called before the first frame update
    void Start()
    {
        // TrialStartHandler = AAEOnlineTrialStart;
        // TrialEndHandler = AAEOnlineTrialEnd;
        RequestTrackHandler = AAEOnlineRequestHandler;

        NetService.Instance.AddDistributer(HandleCmd);

        // AAEconfig = new Dictionary<AAEtype, bool>();
        // Array allAAEtypes = Enum.GetValues(typeof(AAEtype));
        // foreach (AAEtype value in allAAEtypes)
        // {
        //     AAEconfig.Add(value, false);
        // }
        
        HandleCmd("StartTrial_0");  // TODO for debug!!!!!

    }

    //#######################

    private Trial currentTrial = null;

    public void HandleCmd(string msg)
    {
        string[] messages = msg.Split('_');
        // Debug.Log(messages[0]);

        if (messages[0] == "StartTrial")
        {
            Debug.Log("new a trail: Road "+ messages[1]);
            currentTrial = new Trial(roads[int.Parse(messages[1])]);
            Debug.Log("Created new trail: " + currentTrial); // 新增的调试输出
            //start coroutine
            StartCoroutine(SetVolume(Trial.volume));  //use a static var in trial to update volume
        }
        else if (messages[0] == "TrialCmd")
        {
            if (currentTrial != null)
            {
                var trialCmd = new string[messages.Length - 1];
                messages.CopyTo(trialCmd, 1);  //take down the first phrase of original message , copy the rest as a new array
                currentTrial.HandleCmd(trialCmd);  //then give it to HandleCmd method of current trial
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
        if (!currentTrial.Update())
        {
            //new: tell python that a trial has end
            //py TODO: set py receiver
            NetService.Instance.SendMessage("TrialEnd");
            currentTrial.Report();
            currentTrial.EnableUI();
            currentTrial = null;
            GameObject.Find("StartNewTrail").SetActive(true);
            //new: show start next trial button
        }

    }

    // void OnStartTrialButtonPressed()
    // {
    //     ExpService.Instance.SendSessionControlCode("start next trial"); //new:  figure out the button problem
    // }
    // moved to uibciplugin.cs
    
    public IEnumerator SetVolume(float volume)
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
                GetComponent<AudioSource>().volume = volume;
            }
        }
    }
    
    //################
    public void AAEOnlineRequestHandler(int i_trial)
    {
        Debug.Log("Requesting track for Trial" + (i_trial + 1));
        NetService.Instance.SendMessage("TrialCmd_RequestTrack");

    } 
    // TODO sdfsdfsdfdsfsfsdfsdfsdfsdfsdfsdfsdf
    // public void AAEOnlineTrialStart(int i_trial)
    // {
    //
    //     //isOnCheckpoint Ϊ trueʱ�ſ��Կ�ʼ
    //     if (RoadPT.isOnCheckPoint)
    //     {
    //         Debug.Log("Trial Road " + (i_trial + 1).ToString() + " : Start");
    //         ExpService.Instance.SendSessionControlCode("trial start");
    //
    //         // int i_AAEstate = ValueService.Instance.GenerateMIstate(AAEstate_size);
    //         // int AAEstate = (int)AAEtable[i_AAEstate];
    //         // ValueService.Instance.values["AAEstate"] = AAEstate;
    //         // string msg = "Value_AAEstate_" + AAEstate.ToString();
    //         // ValueService.Instance.SendValueUpdate(msg);
    //
    //         // ui.UpdateMainText(AAEtable[i_AAEstate].ToString() + " : Start!");
    //     }
    //     else
    //     {
    //         Debug.Log("ERROR ��Ҳ��ڼ���");
    //     }
    // }
    // TODO sdfsdfsdfdsfsfsdfsdfsdfsdfsdfsdfsdf
    
    
    // public void AAEOnlineTrialEnd(int i_trial)
    // {
    //     Debug.Log("Trial " + (i_trial + 1).ToString() + "End");
    //     ExpService.Instance.SendSessionControlCode("trial end");
    //     ui.UpdateMainText("Trial End");
    // }
    //
    // public IEnumerator TrialLogic(int n_session, int n_run, int n_trial)
    // {
    //
    //
    //
    //     for (int i_session = 0; i_session < n_session; i_session++)
    //     {
    //         yield return new WaitForSeconds(session_start_interval);
    //         //SessionStartHandler(i_session);
    //
    //         for (int i_run = 0; i_run < n_run; i_run++)
    //         {
    //             yield return new WaitForSeconds(trial_start_interval);
    //             //RunStartHandler(i_run);
    //
    //             for (int i_trial = 0; i_trial < n_trial; i_trial++)
    //             {
    //                 yield return new WaitForSeconds(trial_start_interval);
    //                 TrialStartHandler(i_trial); //ʵ�鿪ʼ���������
    //
    //                 yield return new WaitForSeconds(AAE_interval);
    //                 RequestTrackHandler(i_trial); //������һ������
    //
    //                 yield return new WaitForSeconds(4.0f); //Wait for receiving message
    //                 int i_AAEstate = ValueService.Instance.values["AAEstate"];
    //                 AAEtype aae_state = (AAEtype)i_AAEstate;
    //                 ui.UpdateMainText("On Line��" + RoadPT.progress +
    //                                   aae_state.ToString()); // ��ʾ��������������һ�����ߵİٷֱȺ�ע������ƽ��ֵ
    //
    //                 yield return new WaitForSeconds(2.0f); //Wait for message to transfer to server
    //                 TrialEndHandler(i_trial);
    //                 yield return new WaitForSeconds(trial_end_interval);
    //             }
    //
    //             //RunEndHandler(i_run);
    //             yield return new WaitForSeconds(run_end_interval);
    //         }
    //
    //         //SessionEndHandler(i_session);
    //         yield return new WaitForSeconds(session_end_interval);
    //     }
    // }
}
