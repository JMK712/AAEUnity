using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Barmetler;
using Barmetler.RoadSystem;
using UnityEngine;
    

public class PlayDataCollector :MonoBehaviour
{
    public static bool isOnCheckPoint = false;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            isOnCheckPoint = false;
        }
        else if (other.gameObject.CompareTag("CheckPoint"))
        {
            isOnCheckPoint = true;
        }
    }
}
