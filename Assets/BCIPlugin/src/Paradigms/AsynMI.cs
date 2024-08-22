using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AsynMI : MonoBehaviour
{
    public UIBCIPlugin ui;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int i_MIstate = ValueService.Instance.values["MIstate"];
        MItype mi_state = (MItype)i_MIstate;
        ui.UpdateMainText("Feedback: " + mi_state.ToString());
    }
}
