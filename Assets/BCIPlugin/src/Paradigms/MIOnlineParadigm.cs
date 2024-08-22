using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MIOnlineParadigm : MonoBehaviour
{
    public delegate void Handler(int i);

    public Handler SessionStartHandler;
    public Handler SessionEndHandler;
    public Handler RunStartHandler;
    public Handler RunEndHandler;
    public Handler TrialStartHandler;
    public Handler TrialEndHandler;
    public Handler RequestFeedbackHandler;

    public int n_session = 1;
    public int n_run = 3;
    public int n_trial = 4;

    public float session_start_interval = 1f;
    public float run_start_interval = 1f;
    public float trial_start_interval = 1f;

    public float session_end_interval = 1f;
    public float run_end_interval = 1f;
    public float trial_end_interval = 1f;

    public float MI_interval = 4f;
    public int MIstate_size = 0;
    public bool trigger = false;

    public UIBCIPlugin ui;

    public Dictionary<int, MItype> MItable;
    public Dictionary<MItype, bool> MIconfig;

    // Start is called before the first frame update
    void Start()
    {
        SessionStartHandler = MIOnlineSessionStart;
        SessionEndHandler = MIOnlineSessionEnd;
        RunStartHandler = MIOnlineRunStart;
        RunEndHandler = MIOnlineRunEnd;
        TrialStartHandler = MIOnlineTrialStart;
        TrialEndHandler = MIOnlineTrialEnd;
        RequestFeedbackHandler = MIOnlineRequestHandler;

        NetService.Instance.AddDistributer(HandleCmd);

        MIconfig = new Dictionary<MItype, bool>();
        Array allMItypes = Enum.GetValues(typeof(MItype));
        foreach (MItype value in allMItypes)
        {
            MIconfig.Add(value, false);
        }

    }

    void ResetMItable()
    {
        int MI_i = 0;
        this.MItable = new Dictionary<int, MItype>();
        foreach (MItype t in MIconfig.Keys)
        {
            if (MIconfig[t])
            {
                this.MItable[MI_i] = t;
                MI_i++;

            }
        }
        this.MIstate_size = MI_i;
    }

    public void HandleCmd(string msg)
    {
        string[] messages = msg.Split('_');
        //Debug.Log(messages[0]);
        if (messages[0] == "Cmd")
        {
            if (messages[1] == "MIOnline")
            {
                if (messages[2] == "SetNSession")
                {
                    n_session = int.Parse(messages[3]);
                }

                if (messages[2] == "SetNRun")
                {
                    n_run = int.Parse(messages[3]);
                }

                if (messages[2] == "SetNTrial")
                {
                    n_trial = int.Parse(messages[3]);
                }

                if (messages[2] == "SetTrialLength")
                {
                    MI_interval = float.Parse(messages[3]);
                }

                if (messages[2] == "Start")
                {
                    Debug.Log("MIOnline Started");
                    trigger = true;
                }


                if (messages[2] == "SetLeftHandOn")
                {
                    MIconfig[MItype.LeftHand] = true;
                }
                if (messages[2] == "SetLeftHandOff")
                {
                    MIconfig[MItype.LeftHand] = false;
                }
                if (messages[2] == "SetRightHandOn")
                {
                    MIconfig[MItype.RightHand] = true;
                }
                if (messages[2] == "SetRightHandOff")
                {
                    MIconfig[MItype.RightHand] = false;
                }
                if (messages[2] == "SetTongueOn")
                {
                    MIconfig[MItype.Tongue] = true;
                }
                if (messages[2] == "SetTongueOff")
                {
                    MIconfig[MItype.Tongue] = false;
                }
                if (messages[2] == "SetLeftFootOn")
                {
                    MIconfig[MItype.LeftFoot] = true;
                }
                if (messages[2] == "SetLeftFootOff")
                {
                    MIconfig[MItype.LeftFoot] = false;
                }
                if (messages[2] == "SetRightFootOn")
                {
                    MIconfig[MItype.RightFoot] = true;
                }
                if (messages[2] == "SetRightFootOff")
                {
                    MIconfig[MItype.RightFoot] = false;
                }
                if (messages[2] == "SetRestOn")
                {
                    MIconfig[MItype.Rest] = true;
                }
                if (messages[2] == "SetRestOff")
                {
                    MIconfig[MItype.Rest] = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            startParadigm();
            trigger = false;
        }

    }

    public void startParadigm()
    {
        ResetMItable();
        StartCoroutine(TrialLogic(n_session, n_run, n_trial));
    }

    public void MIOnlineRequestHandler(int i_trial)
    {
        Debug.Log("Requesting Feedback for Trial" + (i_trial + 1).ToString());
        ExpService.Instance.SendSessionControlCode("request data");
    }


    public void MIOnlineSessionStart(int i_session)
    {
        Debug.Log("Session " + (i_session + 1).ToString() + "Start");
        ExpService.Instance.SendSessionControlCode("session start");
        ui.UpdateMainText("Session Start");
    }

    public void MIOnlineSessionEnd(int i_session)
    {
        Debug.Log("Session " + (i_session + 1).ToString() + "End");
        ExpService.Instance.SendSessionControlCode("session end");
        ui.UpdateMainText("Session End");
    }

    public void MIOnlineRunStart(int i_run)
    {
        Debug.Log("Run " + (i_run + 1).ToString() + "Start");
        ExpService.Instance.SendSessionControlCode("run start");
        ui.UpdateMainText("Run Start");
    }

    public void MIOnlineRunEnd(int i_run)
    {
        Debug.Log("Run " + (i_run + 1).ToString() + "End");
        ExpService.Instance.SendSessionControlCode("run end");
        ui.UpdateMainText("Run End");
    }
    /*
    public void MIOnlineTrialStart(int i_trial)
    {
        Debug.Log("Trial " + (i_trial + 1).ToString() + "Start");
        ExpService.Instance.SendSessionControlCode("trial start");

        int i_MIstate = ValueService.Instance.GenerateMIstate(MIstate_size);
        int MIstate = (int)MItable[i_MIstate];
        ValueService.Instance.values["MIstate"] = MIstate;
        string msg = "Value_MIstate_" + MIstate.ToString();
        ValueService.Instance.SendValueUpdate(msg);

        ui.UpdateMainText("Cue: "+MItable[i_MIstate].ToString());
    }
    */

    public void MIOnlineTrialStart(int i_trial)
    {
        Debug.Log("Trial " + (i_trial + 1).ToString() + " Start");
        ExpService.Instance.SendSessionControlCode("trial start");

        int i_MIstate = ValueService.Instance.GenerateMIstate(MIstate_size);
        int MIstate = (int)MItable[i_MIstate];
        ValueService.Instance.values["MIstate"] = MIstate;
        string msg = "Value_MIstate_" + MIstate.ToString();
        ValueService.Instance.SendValueUpdate(msg);

        // 映射 MItype 到对应的 UI 文本
        string miTypeText = GetMIText((MItype)MIstate);
        ui.UpdateMainText("Cue: " + miTypeText);
    }

    // 新增方法，用于将 MItype 映射到文本
    private string GetMIText(MItype miType)
    {
        switch (miType)
        {
            case MItype.LeftHand:
                return "Road 1";
            case MItype.RightHand:
                return "Road 2";
            case MItype.Tongue:
                return "Tongue";
            case MItype.LeftFoot:
                return "Road 3";
            case MItype.RightFoot:
                return "Road 4";
            case MItype.Rest:
                return "Pause";
            default:
                return "Unknown";
        }
    }

    public void MIOnlineTrialEnd(int i_trial)
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
            SessionStartHandler(i_session);

            for (int i_run = 0; i_run < n_run; i_run++)
            {
                yield return new WaitForSeconds(trial_start_interval);
                RunStartHandler(i_run);

                for (int i_trial = 0; i_trial < n_trial; i_trial++)
                {
                    yield return new WaitForSeconds(trial_start_interval);
                    TrialStartHandler(i_trial);

                    yield return new WaitForSeconds(MI_interval);
                    RequestFeedbackHandler(i_trial);

                    yield return new WaitForSeconds(4.0f); //Wait for receiving message
                    int i_MIstate = ValueService.Instance.values["MIstate"];
                    MItype mi_state = (MItype)i_MIstate;
                    ui.UpdateMainText("Feedback: "+ mi_state.ToString());

                    yield return new WaitForSeconds(2.0f); //Wait for message to transfer to server
                    TrialEndHandler(i_trial);
                    yield return new WaitForSeconds(trial_end_interval);
                }
                RunEndHandler(i_run);
                yield return new WaitForSeconds(run_end_interval);
            }
            SessionEndHandler(i_session);
            yield return new WaitForSeconds(session_end_interval);
        }
    }
}
