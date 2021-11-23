using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawingLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void AddMarker(Vector3 position)
    {
        var currentPos = lineRenderer.positionCount++;
        lineRenderer.SetPosition(currentPos, position);
    }

    public bool TryGetLastMarkerPos(out Vector3 lastPos)
    {
        var positions = lineRenderer.positionCount;
        var result = positions > 0;
        lastPos = result ? lineRenderer.GetPosition(positions - 1) : Vector3.zero;
        return result;
    }

    public List<Vector3> GetAllMarkers()
    {
        var markers = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(markers);
        return markers.ToList();
    }

    public void Clear()
    {
        lineRenderer.positionCount = 0;
    }
}
