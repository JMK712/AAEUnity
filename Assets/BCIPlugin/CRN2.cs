using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets; // 假设我们使用Socket编程来与BCIPlugin通信
using Barmetler.RoadSystem;
using Barmetler;
using TMPro;
using UnityEngine.Experimental.GlobalIllumination;
using PointList = System.Collections.Generic.List<Barmetler.Bezier.OrientedPoint>;

public class ExperimentNavigator : MonoBehaviour
{
    public RoadSystem currentRoadSystem;
    public Transform Goal;
    public float GraphStepSize = 1;
    public float MinDistanceYScale = 1;
    public float MinDistanceToRoadToConnect = 10;

    public float RemainingDistance { get; private set; }
    public bool IsOnTrack { get; private set; }
    public Vector3 ClosestPointOnTrack { get; private set; }
    public float ProgressPercentage { get; private set; }

    public TextMeshProUGUI TimeText; // UI元素，用于显示实时用时
    public TextMeshProUGUI TrackText; // UI元素，用于显示是否在道路中线上
    public TextMeshProUGUI ProgressText; // UI元素，用于显示剩余道路的百分比

    public float StartTime { get; private set; }

    private PointList CurrentPoints { get; set; } = new PointList();
    private AsyncUpdater<PointList> currentPoints;

    private float lastUpdateRemainingDistanceTime = 0f;
    private float lastCheckIfOnTrackTime = 0f;
    private const float UPDATE_REMAINING_DISTANCE_INTERVAL = 0.5f; // 每0.5秒更新一次
    private const float CHECK_IF_ON_TRACK_INTERVAL = 1f; // 每1秒检查一次

    private Socket clientSocket; // 用于与BCIPlugin通信的Socket实例
    private byte[] receiveBuffer; // 接收数据的缓冲区

    private void Start()
    {
        // 初始化Socket连接
        InitializeSocketConnection();

        // 计算开始时间
        StartTime = Time.time;

        // 初始化AsyncUpdater
        currentPoints = new AsyncUpdater<PointList>(this, GetNewWayPoints, new PointList(), 1f / 144);

        // 更新UI
        UpdateUI();
    }

    private void Update()
    {
        currentPoints.Update();
        UpdateUI(); // 更新UI
    }

    private void FixedUpdate()
    {
        var points = currentPoints.GetData();
        if (points != CurrentPoints)
        {
            CurrentPoints = points;
            UpdateRemainingDistanceIfNeeded();
            CheckIfOnTrackIfNeeded();
            
            void RemovePointsBehind()
            {
                var pos = transform.position;
                int count = 0;
                for (; count < CurrentPoints.Count - 1; ++count)
                {
                    // 如果下一个点更远，则停止（但不要在当前点非常接近的情况下停止）
                    float dx = CurrentPoints[count].position.x - pos.x;
                    float dy = CurrentPoints[count].position.y - pos.y;
                    float dz = CurrentPoints[count].position.z - pos.z;
                    float sqrDst = dx * dx + dy * dy + dz * dz;

                    float nextDx = CurrentPoints[count + 1].position.x - pos.x;
                    float nextDy = CurrentPoints[count + 1].position.y - pos.y;
                    float nextDz = CurrentPoints[count + 1].position.z - pos.z;
                    float nextSqrDst = nextDx * nextDx + nextDy * nextDy + nextDz * nextDz;

                    if (sqrDst < nextSqrDst && sqrDst > (GraphStepSize / 2f) * (GraphStepSize / 2f)) break;
                }

                if (count > 0)
                {
                    CurrentPoints.RemoveRange(0, count);
                }
                RemovePointsBehind();
            }
        }
    }

    private void UpdateRemainingDistanceIfNeeded()
    {
        if (Time.time - lastUpdateRemainingDistanceTime >= UPDATE_REMAINING_DISTANCE_INTERVAL)
        {
            lastUpdateRemainingDistanceTime = Time.time;
            UpdateRemainingDistance();
        }
    }

    private void CheckIfOnTrackIfNeeded()
    {
        if (Time.time - lastCheckIfOnTrackTime >= CHECK_IF_ON_TRACK_INTERVAL)
        {
            lastCheckIfOnTrackTime = Time.time;
            CheckIfOnTrack();
        }
    }

    private void UpdateRemainingDistance()
    {
        float totalDistance = 0;
        for (int i = 0; i < CurrentPoints.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(CurrentPoints[i].position, CurrentPoints[i + 1].position);
        }
        RemainingDistance = totalDistance - Vector3.Distance(transform.position, CurrentPoints[0].position);
        ProgressPercentage = (totalDistance - RemainingDistance) / totalDistance * 100f;
    }

    private void CheckIfOnTrack()
    {
        float minDistance;
        Road road;
        Vector3 closestPoint;
        float distanceAlongRoad;
        minDistance = currentRoadSystem.GetMinDistance(transform.position, Mathf.Max(0.1f, GraphStepSize), MinDistanceYScale, out road, out closestPoint, out distanceAlongRoad);
        IsOnTrack = minDistance <= GraphStepSize / 2;
        ClosestPointOnTrack = closestPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            // 触发碰撞事件，通知BCIPlugin
            SendDataToBCIPlugin("checkpoint");

            // 清除当前路径点
            CurrentPoints.Clear();
        }
    }

    private void UpdateUI()
    {
        TimeText.text = $"Time: {Time.time - StartTime:F2} s";
        TrackText.text = $"On Track: {(IsOnTrack ? "Yes" : "No")}";
        ProgressText.text = $"Progress: {ProgressPercentage:F2}%";
    }

    private void SendDataToBCIPlugin(string message)
    {
        // 将消息转换为字节数组并发送给BCIPlugin
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        clientSocket.Send(messageBytes);
    }

    private void InitializeSocketConnection()
    {
        // 创建Socket实例
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 连接到BCIPlugin
        clientSocket.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 9000));

        // 创建接收缓冲区
        receiveBuffer = new byte[1024];
    }

    private void OnDestroy()
    {
        // 关闭Socket连接
        clientSocket.Close();
    }

    PointList GetNewWayPoints()
    {
        return currentRoadSystem.FindPath(
            transform.position, Goal.position, MinDistanceYScale,
            Mathf.Max(0.1f, GraphStepSize), MinDistanceToRoadToConnect);
    }
}


