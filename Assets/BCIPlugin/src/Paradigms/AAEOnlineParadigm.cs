using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AAEOnlineParadigm : MonoBehaviour
{
    public delegate void Handler(int i);

    // public Handler SessionStartHandler;
    // public Handler SessionEndHandler;
    // public Handler RunStartHandler;
    // public Handler RunEndHandler;
    public Handler TrialStartHandler;
    public Handler TrialEndHandler;
    public Handler RequestTrackHandler;

    public int n_session = 1;
    public int n_run = 3;
    public int n_trial = 4;

    public float session_start_interval = 1f;
    public float run_start_interval = 1f;
    public float trial_start_interval = 1f;

    public float session_end_interval = 1f;
    public float run_end_interval = 1f;
    public float trial_end_interval = 1f;

    public float AAE_interval = 4f;
    public int AAEstate_size = 0;
    public bool trigger = false;

    public UIBCIPlugin ui;
    public RoadProgressTracker RoadPT;

    public Dictionary<int, AAEtype> AAEtable;
    public Dictionary<AAEtype, bool> AAEconfig;

    // Start is called before the first frame update
    void Start()
    {
        // SessionStartHandler = AAEOnlineSessionStart;
        // SessionEndHandler = AAEOnlineSessionEnd;
        // RunStartHandler = AAEOnlineRunStart;
        // RunEndHandler = AAEOnlineRunEnd;
        TrialStartHandler = AAEOnlineTrialStart;
        TrialEndHandler = AAEOnlineTrialEnd;
        RequestTrackHandler = AAEOnlineRequestHandler;

        NetService.Instance.AddDistributer(HandleCmd);

        AAEconfig = new Dictionary<AAEtype, bool>();
        Array allAAEtypes = Enum.GetValues(typeof(AAEtype));
        foreach (AAEtype value in allAAEtypes)
        {
            AAEconfig.Add(value, false);
        }

    }

    void ResetAAEtable()
    {
        int AAE_i = 0;
        this.AAEtable = new Dictionary<int, AAEtype>();
        foreach (AAEtype t in AAEconfig.Keys)
        {
            if (AAEconfig[t])
            {
                this.AAEtable[AAE_i] = t;
                AAE_i++;

            }
        }

        this.AAEstate_size = AAE_i;
    }

    //#######################

    private Trial currentTrial = null;

    public void HandleCmd(string msg)
    {
        string[] messages = msg.Split('_');
        // Debug.Log(messages[0]);

        if (messages[0] == "StartTrial")
        {
            currentTrial = new Trial(RoadProgressTracker.ROADS[int.Parse(messages[1])]);
            //start coroutine
            StartCoroutine(CheckVolume(volume));
        }
        else if (messages[0] == "TrialCmd")
        {
            if (currentTrial != null)
            {
                var trialCmd = new string[messages.Length - 1];
                messages.CopyTo(trialCmd, 1);
                currentTrial.HandleCmd(trialCmd);
            }
            else
            {
                Debug.Log("Skipping trial command: " + messages);
            }
        }

        // if (messages[0] == "Cmd")
        // {
        //     if (messages[1] == "AAEOline")
        //     {
        //         if (messages[2] == "SetNSession")
        //         {
        //             n_session = int.Parse(messages[3]);
        //         }
        //
        //         if (messages[2] == "SetNRun")
        //         {
        //             n_run = int.Parse(messages[3]);
        //         }
        //
        //         if (messages[2] == "SetNTrial")
        //         {
        //             n_trial = int.Parse(messages[3]);
        //         }
        //
        //         if (messages[2] == "SetTrialLength")
        //         {
        //             AAE_interval = float.Parse(messages[3]);
        //         }
        //
        //         if (messages[2] == "Start")
        //         {
        //             Debug.Log("AAEOnline Started");
        //             trigger = true;
        //         }
        //
        //
        //         if (messages[2] == "SetRoad1On")
        //         {
        //             AAEconfig[AAEtype.Road1] = true;
        //         }
        //         if (messages[2] == "SetRoad1Off")
        //         {
        //             AAEconfig[AAEtype.Road1] = false;
        //         }
        //         if (messages[2] == "SetRoad2On")
        //         {
        //             AAEconfig[AAEtype.Road2] = true;
        //         }
        //         if (messages[2] == "SetRoad2Off")
        //         {
        //             AAEconfig[AAEtype.Road2] = false;
        //         }
        //         if (messages[2] == "SetRoad3On")
        //         {
        //             AAEconfig[AAEtype.Road3] = true;
        //         }
        //         if (messages[2] == "SetRoad3Off")
        //         {
        //             AAEconfig[AAEtype.Road3] = false;
        //         }
        //         if (messages[2] == "SetRoad4On")
        //         {
        //             AAEconfig[AAEtype.Road4] = true;
        //         }
        //         if (messages[2] == "SetRoad4Off")
        //         {
        //             AAEconfig[AAEtype.Road4] = false;
        //         }
        //         if (messages[2] == "SetPauseOn")
        //         {
        //             AAEconfig[AAEtype.Pause] = true;
        //         }
        //         if (messages[2] == "SetPauseOff")
        //         {
        //             AAEconfig[AAEtype.Pause] = false;
        //         }
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // if (trigger)
        // {
        //     startParadigm();
        //     trigger = false;
        // }

        if (!currentTrial.Update())
        {
            //TODO:tell python that a trial has begun
            currentTrial.Report();
            currentTrial = null;
            //TODO: show start next trial button
        }

    }

    void OnStartTrialButtonPressed()
    {
        ExpService.Instance.SendSessionControlCode("start next trial"); //TODO figure out the button problem
    }

    //################

    public void startParadigm()
    {
        ResetAAEtable();
        StartCoroutine(TrialLogic(n_session, n_run, n_trial)); // TODO:revive it in python
    }

    public void AAEOnlineRequestHandler(int i_trial)
    {
        Debug.Log("Requesting track for Trial" + (i_trial + 1).ToString());
        ExpService.Instance.SendSessionControlCode("request track data");

    }


    // public void AAEOnlineSessionStart(int i_session)
    // {
    //     Debug.Log("Session " + (i_session + 1).ToString() + "Start");
    //     ExpService.Instance.SendSessionControlCode("session start"); // New participant
    //     ui.UpdateMainText("New Participant Start");
    // }
    //
    // public void AAEOnlineSessionEnd(int i_session)
    // {
    //     Debug.Log("Session " + (i_session + 1).ToString() + "End");
    //     ExpService.Instance.SendSessionControlCode("session end"); // A participant is done participate
    //     ui.UpdateMainText("Participat End");
    // }
    //
    // public void AAEOnlineRunStart(int i_run)
    // {
    //     Debug.Log("Run " + (i_run + 1).ToString() + "Start");
    //     ExpService.Instance.SendSessionControlCode("run start"); //New scene
    //     ui.UpdateMainText("Run Start");
    // }
    //
    // public void AAEOnlineRunEnd(int i_run)
    // {
    //     Debug.Log("Run " + (i_run + 1).ToString() + "End");
    //     ExpService.Instance.SendSessionControlCode("run end"); //scene end
    //     ui.UpdateMainText("Run End");
    // }

    public void AAEOnlineTrialStart(int i_trial)
    {

        //isOnCheckpoint Ϊ trueʱ�ſ��Կ�ʼ
        if (RoadPT.isOnCheckPoint)
        {
            Debug.Log("Trial Road " + (i_trial + 1).ToString() + " : Start");
            // ��֪��������ʼ
            ExpService.Instance.SendSessionControlCode("trial start");

            int i_AAEstate = ValueService.Instance.GenerateMIstate(AAEstate_size);
            int AAEstate = (int)AAEtable[i_AAEstate];
            ValueService.Instance.values["AAEstate"] = AAEstate;
            string msg = "Value_AAEstate_" + AAEstate.ToString();
            ValueService.Instance.SendValueUpdate(msg);

            ui.UpdateMainText(AAEtable[i_AAEstate].ToString() + " : Start!");
        }
        else
        {
            // ������ڼ��㣬�������������һЩ��־��¼���ߴ�����
            Debug.Log("ERROR ��Ҳ��ڼ���");
        }
    }

    public void AAEOnlineTrialEnd(int i_trial)
    {
        Debug.Log("Trial " + (i_trial + 1).ToString() + "End");
        ExpService.Instance.SendSessionControlCode("trial end");
        ui.UpdateMainText("Trial End");
    }

    public IEnumerator TrialLogic(int n_session, int n_run, int n_trial)
    {



        for (int i_session = 0; i_session < n_session; i_session++)
        {
            yield return new WaitForSeconds(session_start_interval);
            //SessionStartHandler(i_session);

            for (int i_run = 0; i_run < n_run; i_run++)
            {
                yield return new WaitForSeconds(trial_start_interval);
                //RunStartHandler(i_run);

                for (int i_trial = 0; i_trial < n_trial; i_trial++)
                {
                    yield return new WaitForSeconds(trial_start_interval);
                    TrialStartHandler(i_trial); //ʵ�鿪ʼ���������

                    yield return new WaitForSeconds(AAE_interval);
                    RequestTrackHandler(i_trial); //������һ������

                    yield return new WaitForSeconds(4.0f); //Wait for receiving message
                    int i_AAEstate = ValueService.Instance.values["AAEstate"];
                    AAEtype aae_state = (AAEtype)i_AAEstate;
                    ui.UpdateMainText("On Line��" + RoadPT.progress +
                                      aae_state.ToString()); // ��ʾ��������������һ�����ߵİٷֱȺ�ע������ƽ��ֵ

                    yield return new WaitForSeconds(2.0f); //Wait for message to transfer to server
                    TrialEndHandler(i_trial);
                    yield return new WaitForSeconds(trial_end_interval);
                }

                //RunEndHandler(i_run);
                yield return new WaitForSeconds(run_end_interval);
            }

            //SessionEndHandler(i_session);
            yield return new WaitForSeconds(session_end_interval);
        }
    }

    public IEnumerator CheckVolume(float volume)
    {
        yield return new WaitForSeconds(1f);
        ExpService.Instance.SendSessionControlCode("request volume data");
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
