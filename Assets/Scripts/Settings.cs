using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, menuName = "Scriptable Objects/Settins", fileName = "Settings")]
public class Settings : ScriptableObject
{
    [Header("Drawing")]
    public LayerMask drawerInteractionMask;
    public DrawingLine drawingLinePrefab;
    public float maxRaycastDistance = 100f;
    public float minSqrDistanceBetweenPoints = 1f;

    [Header("Drawing Recognition")]
    public List<DrawingTemplate> drawingTemplates;
    public int pointsSamplesCount = 32;
    public int matrixSize = 64;

    [Header("Saving Templates")]
    public string defaultTemplateSavingPath = "Assets/ScriptableObjects/Templates";
    public string defaultTemplateImageSavingPath = "Assets/Images";
}
