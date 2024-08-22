using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//int value represent MItype
public enum MItype
{   
    LeftHand,
    RightHand,
    Tongue,
    LeftFoot,
    RightFoot,
    Rest
}
public enum AAEtype
{
    Road1,
    Road2,
    Road3,
    Road4,
    Pause
}



public class ValueService
{
    static ValueService instance;
    public static ValueService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ValueService();
            }
            return instance;
        }
    }
    //Values
    public Dictionary<string, int> values = new Dictionary<string, int>();
    //Values

    public void ValueHandler(string msg)
    {
        //Debug.Log("ValueService received: "+msg);
        string[] messages = msg.Split('_');
        values[messages[1]] = int.Parse(messages[2]);
        
    }

    public int GenerateMIstate(int size)
    {
        System.Random r1 = new System.Random();
        int a1 = r1.Next(0, size);
        return a1;
    }

    public ValueService()
    {
        values["AAEstate"] = -1;
        values["Attention"] = -1;
        values["MIstate"] = -1;
        values["SSVEPstate"] = -1;
        values["P300state"] = -1;
    }

    public void SendValueUpdate(string msg)
    {
        NetService.Instance.SendMessage(msg);
    }

}
