using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Barmetler.RoadSystem;
using Barmetler;


public class RoadProgressTracker : MonoBehaviour
{
    [SerializeField] private List<Road> roads; // 所有可能的道路对象
    public static List<Road> ROADS;
    public float progress;  // 在线上的百分比
    public Text progressText; // UI中的文本组件，用于显示进度
    public Text CurrentRoadText;// UI中的文本组件，用于显示当前道路
    public float OnLineDistanceThreshold = 0.24f; // 距离阈值，判断玩家是否在道路中线上
    public float OnRoadDistanceThreshold = 5f; // 距离阈值，判断玩家是否在道路上
    public float Percision = 1.0f; // 产生平均点的间隔（不用于GetCurrentRoad)
    public string CurrentRoadName;
    public string PreviousRoadName;
    public bool isOnCheckPoint;
    private Vector3 Currentclosestpoint;
    private Vector3[] positions; // 存储道路上均匀分布的点的位置
    private float totalDistance; // 道路总长度
    private float currentDistance; // 当前已走的距离
    private Vector3 previousPlayerPosition; // 每一次调用GetOnLineDistance时新增的距离


    void Start()
    {
        // 初始化变量
        positions = null;
        totalDistance = 0f;
        currentDistance = 0f;
        previousPlayerPosition = transform.position;
        Currentclosestpoint = Vector3.zero;
        CurrentRoadName = "";
        PreviousRoadName = "";

        ROADS = roads;
    }

    void Update()
    {
        //如果当前道路名称和上一个道路名称不同，即玩家走到新的道路，则正常计算距离
        if (CurrentRoadName != PreviousRoadName)
        {
            // 获取道路上均匀分布的点的位置
            if (positions == null || totalDistance == 0f)
            {
                Bezier.OrientedPoint[] orientedPositions = GetPositionAlongRoad(GetCurrentRoad(), Percision); // 每隔Precesion单位距离取一个点
                positions = ExtractPositionsFromOrientedPoints(orientedPositions);
                totalDistance = CalculateTotalDistance(positions);
                Debug.Log("Current Total Distance: " + totalDistance);
            }

            // 更新当前已走的距离
            currentDistance += CalculateCurrentDistance();
            //Debug.Log("Current Distance: " + currentDistance);

            // 计算并更新UI上的进度
            UpdateProgressUI();
        }
        else if (CurrentRoadName == PreviousRoadName)
        {
            ResetOnLineCalculate();
        }
    }


    /// <summary>
    /// 获取道路上均匀分布的点的位置
    /// </summary>
    /// <param name="road">道路对象</param>
    /// <param name="interval">点之间的间隔距离</param>
    /// <returns>均匀分布的点的位置数组</returns>
    private Bezier.OrientedPoint[] GetPositionAlongRoad(Road road, float interval)
    {
        Bezier.OrientedPoint[] orientedPoints = road.GetEvenlySpacedPoints(interval);
        //Debug.Log("Positions : " + orientedPoints);
        return orientedPoints;
    }

    /// <summary>
    /// 将OrientedPoint数组转换为Vector3数组，只包含位置信息
    /// </summary>
    /// <param name="orientedPoints">OrientedPoint数组</param>
    /// <returns>Vector3数组，只包含位置信息</returns>
    private Vector3[] ExtractPositionsFromOrientedPoints(Bezier.OrientedPoint[] orientedPoints)
    {
        Vector3[] positions = new Vector3[orientedPoints.Length];
        Road road = GetCurrentRoad();
        for (int i = 0; i < orientedPoints.Length; i++)
        {
            Bezier.OrientedPoint orientedPoint = orientedPoints[i];

            Bezier.OrientedPoint worldOrientedPoint = orientedPoint.ToWorldSpace(road.transform);
            Vector3 worldPosition = worldOrientedPoint.position;

            positions[i] = worldPosition;
        }

        return positions;
    }

    /// <summary>
    /// 计算道路上所有点之间的总距离
    /// </summary>
    /// <param name="positions">道路上的点的位置</param>
    /// <returns>总距离</returns>
    private float CalculateTotalDistance(Vector3[] positions)
    {
        float distance = 0f;
        for (int i = 1; i < positions.Length; i++)
        {
            distance += Vector3.Distance(positions[i - 1], positions[i]);
        }
        return distance;
    }


    /// <summary>
    /// 计算玩家沿着道路走过的距离
    /// </summary>
    /// <returns>当前已走的距离增量</returns>
    private float CalculateCurrentDistance()
    {
        Vector3 playerPosition = transform.position;
        float distanceIncrement = 0f;

        // 遍历所有存储的点
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 currentPosition = positions[i];
            Vector3 nextPosition = positions[i + 1];

            // 计算玩家位置到当前线段上的距离最短的点
            Vector3 closestPointOnLine = GetClosestPointOnLineSegment(currentPosition, nextPosition, playerPosition);
            Vector3 closestPointOnLinePrevious = GetClosestPointOnLineSegment(currentPosition, nextPosition, previousPlayerPosition);

            if (Vector3.Distance(closestPointOnLine, playerPosition) < OnLineDistanceThreshold)
            {
                // 判断调用期间是否都在当前线上，避免经过下一个点时重复计算
                // 忽略垂直方向的坐标，因为路有倾斜角度
                Vector3 shortestDistance = playerPosition - closestPointOnLine;
                Vector3 shortestDistancePrevious = previousPlayerPosition - closestPointOnLinePrevious;
                Vector3 currentRoadSession = nextPosition - currentPosition;

                // 优化判断条件
                if (Mathf.Abs(Vector3.Dot(shortestDistance, currentRoadSession)) <= 0.0001f &&
                    Mathf.Abs(Vector3.Dot(shortestDistancePrevious, currentRoadSession)) <= 0.0001f)
                {
                    //Debug.Log("OnLine Event Triggered");
                    if (Currentclosestpoint != currentPosition)
                    {
                        distanceIncrement = Vector3.Distance(currentPosition, nextPosition);
                        Currentclosestpoint = currentPosition;
                    }
                }
            }
            previousPlayerPosition = playerPosition;
        }

        //Debug.Log("Calculated Current Distance Increment: " + distanceIncrement);
        return distanceIncrement;
    }
    /// <summary>
    /// 更新UI上的进度显示
    /// </summary>
    private void UpdateProgressUI()
    {
        progress = currentDistance / totalDistance;
        progressText.text = "On line: " + (progress * 100).ToString("F2") + "%";
        //Debug.Log("Progress Updated: " + progressText.text);
    }
    ///<summary>
    ///更新UI上的道路显示
    ///</summary>
    private void UpdateRoadUI()
    {
        CurrentRoadText.text = CurrentRoadName;
    }

    /// <summary>
    /// 根据玩家位置检测当前所在的道路
    /// </summary>
    /// <returns>当前道路对象</returns>
    private Road GetCurrentRoad()
    {
        Vector3 playerPosition = transform.position; // 使用玩家对象的transform.position
        Road closestRoad = null;

        foreach (var road in roads)
        {
            Bezier.OrientedPoint[] roadPositions = GetPositionAlongRoad(road, 5); // 每隔5单位距离取一个点

            // 将道路点位置转换到全局坐标系
            Bezier.OrientedPoint[] globalRoadPositions = new Bezier.OrientedPoint[roadPositions.Length];
            for (int i = 0; i < roadPositions.Length; i++)
            {
                globalRoadPositions[i] = roadPositions[i].ToWorldSpace(road.transform);
            }

            //Debug.Log($"Road Positions Length: {roadPositions.Length}");
            //DebugRoadPositionsAndDistances(road);

            bool isCloseToRoad = false;
            foreach (var pos in globalRoadPositions)
            {
                float distance = Vector3.Distance(playerPosition, pos.position);
                if (distance < OnRoadDistanceThreshold) // 距离阈值
                {
                    isCloseToRoad = true;
                    break;
                }
            }

            if (isCloseToRoad)
            {
                closestRoad = road;
                break;
            }
        }

        //Debug.Log("Closest Road: " + (closestRoad ? closestRoad.name : "null"));
        return closestRoad;
    }
    /// <summary>
    /// 获取线段上离给定点最近的点
    /// </summary>
    /// <param name="start">线段的起点</param>
    /// <param name="end">线段的终点</param>
    /// <param name="point">待检查的点</param>
    /// <returns>线段上离给定点最近的点</returns>
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
    
    /// <summary>
    /// 重置中线正确率的计算
    /// </summary>
    private void ResetOnLineCalculate()
    {
        currentDistance = 0f;
        totalDistance = 0f;
        UpdateProgressUI();
        UpdateRoadUI();
        Debug.Log("重置走线正确率，您已到达下一条路或重新进入道路范围");
    }




    // 更新道路名称的方法
    private void UpdateCurrentRoadName(string newRoadName)
    {
        if (newRoadName != CurrentRoadName)
        {
            // 当道路名称发生变化时
            PreviousRoadName = CurrentRoadName;
            CurrentRoadName = newRoadName;
            
            // 输出调试信息
            Debug.Log($"Road name changed from '{PreviousRoadName}' to '{CurrentRoadName}'");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            // 更新当前道路的名称到开放变量，来标识不同trail
            string newRoadName = GetCurrentRoad().name;
            UpdateCurrentRoadName(newRoadName);
            // 重置是否在检查点上标志，使trial保持进行
            isOnCheckPoint = false;
            // 更新当前道路名称显示UI
            UpdateRoadUI();
            // 重置当前道路的已走距离(进入新的道路或离开当前道路)
            ResetOnLineCalculate();
        }
        else if (other.gameObject.CompareTag("CheckPoint"))
        {
        // 设置是否在检查点上标志为true，使trial暂停
        isOnCheckPoint = true;
        //告知AAEOnlineParadigm trail结束，提供数据

        // 更新当前道路的名称到开放变量，来标识不同trail
        PreviousRoadName = CurrentRoadName;
        }
    }
    public bool GetisOnCheckPoint()
    {
        return isOnCheckPoint;
    }
}