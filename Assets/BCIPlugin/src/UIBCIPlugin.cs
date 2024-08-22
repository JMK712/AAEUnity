using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIBCIPlugin : MonoBehaviour
{
    public string IP = "127.0.0.1";
    public int Port = 50000;
    public Text textLog;
    public InputField inputCmd;
    public Text textMain;
    public Text textCoefficient; 
    
    // Start is called before the first frame update
    void Start()
    {
        // NetService.Instance.ConnectToServer();  // 按钮失效，自启动时连接服务
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectToServer()
    {
        NetService.Instance.ConnectToServer();
    }

    public void SendCmd()
    {
        string cmd = inputCmd.text;
        NetService.Instance.SendMessage(cmd);
    }

    public void UpdateMainText(string msg)
    {
        textMain.text = msg;
    }

    public void OnStartTrialButtonPressed()
    {
        NetService.Instance.SendMessage("StartNewTrial");
        GameObject.Find("StartNewTrial").SetActive(false);
        Debug.Log("StartTrialButtonPressed");
        //new: figure out the button problem
        //py TODO: set receiver
    }

    public void UpdateCoefficient(string msg)
    {
        textCoefficient.text = msg;
    }
}