using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(order = 0, menuName = "Scriptable Objects/Drawing Template", fileName = "Drawing Template")]
public class DrawingTemplate : ScriptableObject
{
    //public string templateName;
    public Drawing drawing;
    public Texture2D image;
    public Sprite sprite;
}
