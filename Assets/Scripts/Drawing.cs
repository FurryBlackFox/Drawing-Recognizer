using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class Drawing
{
    public List<Point> initialPoints;
    public List<Point> points;
    public List<int> lookUpTable;

    public bool IsNormalized { get; private set; } = false;
    
    public Drawing(List<Point> initialPoints, List<int> lookUpTable) : this(initialPoints)
    {
        this.lookUpTable = new List<int>();
        this.lookUpTable.AddRange(lookUpTable);
    }
    
    public Drawing(List<Point> initialPoints)
    {
        this.initialPoints = new List<Point>();
        this.initialPoints.AddRange(initialPoints);
    }

    public void Normalize(int desiredLength, int gridSize)
    {
        Resample(desiredLength);
        points = CalculateScaledAndCenteredPoints(points, gridSize);
        ComputeLookUpTable(gridSize);
        IsNormalized = true;
    }

    public List<Point> GetScaledInitialPoints(int gridSize)
    {
        return CalculateScaledAndCenteredPoints(initialPoints, gridSize);
    }
    
    public void Resample(int desiredLength)
    {
        points = new List<Point> {initialPoints[0]};

        float avgIntervalLength = PathLength(initialPoints) / (desiredLength - 1);
        float sumDistance = 0f;
        for (int i = 1; i < initialPoints.Count; i++)
        {
            if (initialPoints[i].strokeId == initialPoints[i - 1].strokeId)
            {
                float distance = (initialPoints[i - 1].position - initialPoints[i].position).magnitude;
                if (sumDistance + distance >= avgIntervalLength)
                {
                    Point firstPoint = initialPoints[i - 1];
                    while (sumDistance + distance >= avgIntervalLength)
                    {
                        // add interpolated point
                        float t = Math.Min(Math.Max((avgIntervalLength - sumDistance) / distance, 0.0f), 1.0f);
                        if (float.IsNaN(t))
                        {
                            Debug.Log("nan");
                            t = 0.5f;
                        }
                        
                        var newPoint = new Point();
                        newPoint.position = Vector2.Lerp(firstPoint.position, initialPoints[i].position, t);
                        newPoint.strokeId = initialPoints[i].strokeId;
                        points.Add(newPoint);
                        // update partial length
                        distance = sumDistance + distance - avgIntervalLength;
                        sumDistance = 0;
                        firstPoint = points[points.Count - 1];
                    }
                    sumDistance = distance;
                }
                else sumDistance += distance;
            }
        }

        if (points.Count == desiredLength - 1) // sometimes we fall a rounding-error short of adding the last point, so add 
        {
            points.Add(initialPoints[initialPoints.Count - 1]);
        }
    }
    
    private float PathLength(List<Point> points)
    {
        var length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            var currentPoint = points[i];
            var prevPoint = points[i - 1];
            if (currentPoint.strokeId == prevPoint.strokeId)
            {
                length += (prevPoint.position - currentPoint.position).magnitude;
            }
        }

        return length;
    }
    
    private List<Point> CalculateScaledAndCenteredPoints(List<Point> targetPoints, int localM)
    {
        var outputPoints = new List<Point>();
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var point in targetPoints)
        {
            minX = Mathf.Min(minX, point.position.x);
            minY = Mathf.Min(minY, point.position.y);
            
            maxX = Mathf.Max(maxX, point.position.x);
            maxY = Mathf.Max(maxY, point.position.y);
        }
        
        var scale = (localM - 1 ) / Math.Max(maxX - minX, maxY - minY);
        var avgCenter = new Vector2{x = (minX + maxX) / 2f, y = (minY + maxY) / 2f};
        var offset = (localM - 1) * 0.5f;
        foreach (var targetPoint in targetPoints)
        {
            var point = targetPoint;
            point.position.x = (point.position.x - avgCenter.x) * scale + offset;
            point.position.y = (point.position.y - avgCenter.y) * scale + offset;
            outputPoints.Add(point);
        }

        return outputPoints;
    }
    
    // private void TranslateToOrigin()
    // {
    //     var avgCenter = Vector2.zero;
    //
    //     float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
    //     foreach (var point in points)
    //     {
    //         minX = Mathf.Min(minX, point.position.x);
    //         minY = Mathf.Min(minY, point.position.y);
    //         
    //         maxX = Mathf.Max(maxX, point.position.x);
    //         maxY = Mathf.Max(maxY, point.position.y);
    //     }
    //
    //     foreach (var point in points)
    //     {
    //         point.position -= avgCenter;
    //     }
    // }
    
    private void ComputeLookUpTable(int localM)
    {
        lookUpTable = new List<int>();

        for (var x = 0; x < localM; x++)
        {
            for (var y = 0; y < localM; y++)
            {
                var minDistance = float.MaxValue;
                var currentMatrixPos = new Vector2(x, y);
                var index = int.MaxValue;
                for (var i = 0; i < points.Count; i++)
                {
                    var distance = (points[i].position - currentMatrixPos).sqrMagnitude;
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        index = i;
                    }
                }
                lookUpTable.Add(index);
            }
        }
    }
}
