using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingTemplateVisualizer : MonoBehaviour
{
   [SerializeField] private DrawingTemplate drawingTemplate;
   [SerializeField] private bool showPoints = true;
   [SerializeField] private bool showLines = true;
   [SerializeField] private float pointScale = 0.5f;
   [SerializeField] private bool showLutDirections = false;
   [SerializeField] private Settings settings;

   private void OnDrawGizmos()
   {
      if(!drawingTemplate)
         return;
      
      Gizmos.color = Color.red;
      if(showPoints || showLines)
         ShowPointsAndLines();
      
      Gizmos.color = Color.blue;
      if(showLutDirections)
         ShowDirections();
   }

   private void ShowPointsAndLines()
   {
      if(drawingTemplate.drawing.points == null)
         return;

      var prevPos = Vector3.zero;
      for (var i = 0; i < drawingTemplate.drawing.points.Count; i++)
      {
         var currentPoint = drawingTemplate.drawing.points[i];
         var currentPos = new Vector3(currentPoint.position.y, 0, currentPoint.position.x);
         
         if(showPoints)
            Gizmos.DrawCube(currentPos, Vector3.one * pointScale);

         if (i != 0)
         {
            var prevPoint = drawingTemplate.drawing.points[i - 1];
            if (showLines && currentPoint.strokeId == prevPoint.strokeId)
            {
               Gizmos.DrawLine(prevPos, currentPos);
            }
         }

         prevPos = currentPos;
      }
   }

   private void ShowDirections()
   {
      for (var x = 0; x < settings.matrixSize; x++)
      {
         for (var y = 0; y < settings.matrixSize; y++)
         {
            var pointPos = drawingTemplate.drawing.points[drawingTemplate.drawing.
                  lookUpTable[x * settings.matrixSize + y]].position;   
            Gizmos.DrawLine(new Vector3(y, 0, x), new Vector3(pointPos.y, 0, pointPos.x));
         }
      }
   }
}
