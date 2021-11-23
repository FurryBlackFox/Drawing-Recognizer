using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGrid : MonoBehaviour
{
    [SerializeField] private int m = 64;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        for (int i = 0; i < m; i++)
        {
            Gizmos.DrawLine(new Vector3(0, 0, i), new Vector3(m - 1, 0, i));
            Gizmos.DrawLine(new Vector3(i, 0, 0), new Vector3(i, 0, m - 1));
        }
    }
}
