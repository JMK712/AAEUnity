using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExpService
{

    private Dictionary<string, string> sessionControlCode;
    private Dictionary<string, string> reverseSessionControlCode;

    static ExpService instance;

    public static ExpService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ExpService();
            }
            return instance;
        }
    }

    public ExpService()
    {
        sessionControlCode = new Dictionary<string, string>()
        {
            {"off","512" },

            {"session start","257"},
            {"session end", "258"},
            {"run start", "259"},
            {"run end","260" },
            {"trial start", "261" },
            {"trial end", "262" },

            {"record data","263" },
            {"request data","264" },


        };

        reverseSessionControlCode = sessionControlCode.Reverse().ToDictionary(a => a.Value, a => a.Key);
    }

    public void SessionControlHandler(string msg)
    {
        Debug.Log("ExpService received: "+msg);
    }

    public void SendSessionControlCode(string type)
    {
        string msg = "SessionControl_" + sessionControlCode[type];
        NetService.Instance.SendMessage(msg);
    }

}
