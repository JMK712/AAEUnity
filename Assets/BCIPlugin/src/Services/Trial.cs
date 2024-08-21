using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Barmetler;
using Barmetler.RoadSystem;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Trial
{
    private const float THRESHOLD_SQR = 0.3f;
    
    private Road road;
    private Vector3[] roadPoints;
    private HashSet<int> goodSegments = new HashSet<int>();
    public Transform playerTransform;

    public Trial(Road road)
    {
        this.road = road;
        playerTransform = GameObject.Find("layerArmaturer").transform;
        
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
        // TODO set initiated player.transform.position
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

        return true; // TODO: check when return false to end trial(check point)
    }

    public void HandleCmd(string[] cmd)
    {
        if (cmd[0] == "SetMusicVolume")
        {
            //TODO: set volume in Unity
            float volume = int.cmd[2];
            AAEOnlineParadigm.CheckVolume(volume);
        }
    }

    public float MatchCoefficient()
    {
        return (float)goodSegments.Count / (roadPoints.Length - 1);
    }
    /// <summary>
    ///report match coefficient data to server
    /// </summary>
    public void Report()
    {
// TODO: netservice
    }
}
//TODO:mne and eeg stuff