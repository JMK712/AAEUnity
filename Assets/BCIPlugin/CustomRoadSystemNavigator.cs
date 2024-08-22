using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Barmetler.RoadSystem
{
    using PointList = List<Bezier.OrientedPoint>;

    public class OptimizedRoadSystemNavigator : MonoBehaviour
    {
        public RoadSystem currentRoadSystem;

        public Vector3 Goal = Vector3.zero;

        public float GraphStepSize = 1;
        public float MinDistanceYScale = 1;
        public float MinDistanceToRoadToConnect = 10;

        public float RemainingDistance { get; private set; }
        public bool IsOnTrack { get; private set; }
        public Vector3 ClosestPointOnTrack { get; private set; }

        public PointList CurrentPoints { private set; get; } = new PointList();
        private AsyncUpdater<PointList> currentPoints;

        private float lastUpdateRemainingDistanceTime = 0f;
        private float lastCheckIfOnTrackTime = 0f;
        private const float UPDATE_REMAINING_DISTANCE_INTERVAL = 0.5f; // 每0.5秒更新一次
        private const float CHECK_IF_ON_TRACK_INTERVAL = 1f; // 每1秒检查一次

        private void Awake()
        {
            currentPoints = new AsyncUpdater<PointList>(this, GetNewWayPoints, new PointList(), 1f / 144);
        }

        private void Update()
        {
            currentPoints.Update();
        }

        private void FixedUpdate()
        {
            var points = currentPoints.GetData();
            if (points != CurrentPoints)
            {
                CurrentPoints = points;
                UpdateRemainingDistanceIfNeeded();
                CheckIfOnTrackIfNeeded();
            }

            RemovePointsBehind();
        }

        private void OnEnable()
        {
            CalculateWayPointsSync();
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
                // 触发碰撞事件，通知ExpService
                ExpService.Instance.SendSessionControlCode("checkpoint");
            }
        }

        void RemovePointsBehind()
        {
            var pos = transform.position;
            int count = 0;
            for (; count < CurrentPoints.Count - 1; ++count)
            {
                // if next point is further away, stop (but don't stop if current point is really close)
                float sqrDst = (CurrentPoints[count].position - pos).sqrMagnitude;
                if (
                    sqrDst < (CurrentPoints[count + 1].position - pos).sqrMagnitude &&
                    sqrDst > GraphStepSize / 2 * GraphStepSize / 2
                    ) break;
            }

            if (count > 0)
            {
                CurrentPoints.RemoveRange(0, count);
            }
        }

        public void CalculateWayPointsSync()
        {
            CurrentPoints = GetNewWayPoints();
        }

        PointList GetNewWayPoints()
        {
            return currentRoadSystem.FindPath(
                transform.position, Goal, MinDistanceYScale,
                Mathf.Max(0.1f, GraphStepSize), MinDistanceToRoadToConnect);
        }
    }
}