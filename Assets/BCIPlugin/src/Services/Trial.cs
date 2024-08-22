using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Barmetler;
using Barmetler.RoadSystem;
using TMPro;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Trial
{
    private const float THRESHOLD_SQR = 0.3f;
    private GameObject BCIPlugin;
    private Road road;
    private Vector3[] roadPoints;
    private HashSet<int> goodSegments = new HashSet<int>();
    public static float volume;
    public Transform playerTransform;
    private GameObject Player;

    public Trial(Road road)
    {
        this.road = road;
        playerTransform = GameObject.Find("PlayerArmature").transform;
        Initialize();
    }

    private void Initialize()
    {
        Bezier.OrientedPoint[] orientedPositions = this.road.GetEvenlySpacedPoints(1f);
        roadPoints = new Vector3[orientedPositions.Length];

        for (int i = 0; i < orientedPositions.Length; i++)
        {
            Bezier.OrientedPoint orientedPoint = orientedPositions[i];

            Bezier.OrientedPoint worldOrientedPoint = orientedPoint.ToWorldSpace(road.transform);
            Vector3 worldPosition = worldOrientedPoint.position;

            roadPoints[i] = worldPosition;
        }
        // new: set initiated player.transform.position
        Player = GameObject.Find("PlayerArmature");
        playerTransform = Player.transform;
        BCIPlugin = GameObject.Find("BCIPlugin");
        UpdateUI_CurrentTrialName();
        DisableUI();  // new: disable UI, enable player controller to control player via input of mouse
    }
    
    private Vector3 GetClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineVec = end - start;
        Vector3 pointVec = point - start;
        float lineLenSqr = lineVec.sqrMagnitude;
        float t = Vector3.Dot(pointVec, lineVec) / lineLenSqr;

        if (t < 0.0f) t = 0.0f;
        if (t > 1.0f) t = 1.0f;

        return start + lineVec * t;
    }

    public bool Update()
    {
        var playerPosition = playerTransform.position;
        for (int i = 0; i < roadPoints.Length - 1; i++)
        {
            if (goodSegments.Contains(i)) continue;
            var currentPoint = roadPoints[i];
            var nextPoint = roadPoints[i + 1];
            var distanceSqr =
                (playerPosition - GetClosestPointOnLineSegment(currentPoint, nextPoint, playerPosition)).sqrMagnitude;
            if (distanceSqr < THRESHOLD_SQR)
                goodSegments.Add(i);
        }
        UpdateUI_Coeffient();
        
        // new: use a static var to get is
        // return PlayDataCollector.isOnCheckPoint switch
        // {
        //     false => true,
        //     true => false
        // };
        // new :check when return false to end trial(check point)
        return true;
    }

    public void HandleCmd(string[] cmd)
    {
        if (cmd[0] == "SetMusicVolume")
        {
            //new :set volume in Unity
            volume = Convert.ToSingle(cmd[1]);
            Debug.Log("get volume from server : " + volume );
        }
    }

    private float MatchCoefficient()  // new : set private method
    {
        return (float)goodSegments.Count / (roadPoints.Length - 1);
    }
    
    /// <summary>
    ///report match coefficient data to server
    /// </summary>
    public void Report()
    {
    // use Net service to send coefficient to py
    var coefficient = MatchCoefficient();
    var msg = "TrialCmd_Report_" + coefficient;  //py TODO:py set receiver
    NetService.Instance.SendMessage(msg);  //convert only 3 decimal places
    }

    /// <summary>
    /// disable mouse input for input system, therefore enable UI to receive click input
    /// </summary>
    public void EnableUI()
    {
        Player.GetComponent<StarterAssets.StarterAssetsInputs>().cursorLocked = false;
        Player.GetComponent<StarterAssets.StarterAssetsInputs>().cursorInputForLook = false;
    }

    /// <summary>
    /// enable mouse input for input system, therefore disable UI to receive click input
    /// </summary>
    public void DisableUI()
    {
        Player.GetComponent<StarterAssets.StarterAssetsInputs>().cursorLocked = true;
        Player.GetComponent<StarterAssets.StarterAssetsInputs>().cursorInputForLook = true;
    }

    private void UpdateUI_Coeffient()
    {
        BCIPlugin.GetComponent<UIBCIPlugin>().UpdateCoefficient("Progress"+ (MatchCoefficient()*100f).ToString("f2"));
    }

    private void UpdateUI_CurrentTrialName()
    {
        BCIPlugin.GetComponent<UIBCIPlugin>().UpdateMainText(road.name);
    }
}
//py TODO:mne and eeg stuff