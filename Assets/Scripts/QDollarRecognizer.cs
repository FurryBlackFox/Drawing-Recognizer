using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class QDollarRecognizer : MonoBehaviour
{
    public static event Action<DrawingTemplate, int> OnTemplateRecognized; 
    
    [SerializeField] private Settings settings;



    public void Recognize(Drawing drawing)
    {
        var cashedTime = DateTime.Now;
        
        if(!drawing.IsNormalized)
            drawing.Normalize(settings.pointsSamplesCount, settings.matrixSize);
        
        var score = float.MaxValue;
        DrawingTemplate resultTemplate = null;
        foreach (var template in settings.drawingTemplates)
        {
            var distance = CloudMatch(drawing, template.drawing, settings.pointsSamplesCount,
                settings.matrixSize, score);
            if (distance < score)
            {
                score = distance;
                resultTemplate = template;
            }
        }

        var compTimeMs = DateTime.Now.Subtract(cashedTime).Milliseconds;
        OnTemplateRecognized?.Invoke(resultTemplate, compTimeMs);
    }
    
        
    private float CloudMatch(Drawing drawing, Drawing template, int localN, int localM, float minSoFar)
    {
        var eps = 0.5f;
        var step = Mathf.FloorToInt(Mathf.Pow(localN,  1 - eps));

        var lowerBound1 = ComputeLowerBound(drawing, template, step, localN, localM);
        var lowerBound2 = ComputeLowerBound(template, drawing, step, localN, localM);

        for (int i = 0, j = 0; i < localN; i += step, j++)
        {
            if (lowerBound1[j] < minSoFar)
            {
                minSoFar = Mathf.Min(minSoFar, CloudDistance(drawing.points, template.points, localN, i, minSoFar));
            }    
            if (lowerBound2[j] < minSoFar)
            {
                minSoFar = Mathf.Min(minSoFar, CloudDistance(template.points,drawing.points, localN, i, minSoFar));
            }    
        }
        
        return minSoFar;
    }

    private float CloudDistance(List<Point> points, List<Point> template, int localN, int startIndex, float minSoFar)
    {
        var unmachedIndexes = new List<int>();
        for (var j = 0; j < localN; j++)
        {
            unmachedIndexes.Add(j);
        }
        
        var i = startIndex;
        var weight = localN;
        var sum = 0f;
        int indexNotMatched = 0; 
        
        do
        {
            var index = -1;
            var minDistance = float.MaxValue;
            for (var j = indexNotMatched; j < localN; j++)
            {
                var dist = (points[i].position - template[unmachedIndexes[j]].position).sqrMagnitude; 
                if (dist < minDistance)
                {
                    minDistance = dist;
                    index = j;
                }
            }
            unmachedIndexes[index] = unmachedIndexes[indexNotMatched]; 
            sum += (weight--) * minDistance;          
            
            if (sum >= minSoFar) 
                return sum;

            i = (i + 1) % localN;                     
            indexNotMatched++;                        
        } while (i != startIndex);

        return sum;
    }

    private float[] ComputeLowerBound(Drawing drawing, Drawing template, int step, int localN, int localM)
    {
        var lowerBound = new float[localN / step + 1];
        var summedAreaTable = new float[localN];

        lowerBound[0] = 0;
        for (int i = 0; i < localN; i++)
        {
            var approxX = Mathf.RoundToInt(drawing.points[i].position.x);
            var approxY = Mathf.RoundToInt(drawing.points[i].position.y);
            var index = template.lookUpTable[approxX * localM + approxY];
            var distance = (drawing.points[i].position - template.points[index].position).sqrMagnitude;
            summedAreaTable[i] = i == 0 ? distance : summedAreaTable[i - 1] + distance;
            lowerBound[0] += (localN - i) * distance;
        }
        
        for (int i = step, j = 1; i < localN; i += step, j++)
            lowerBound[j] = lowerBound[0] + i * summedAreaTable[localN - 1] - localN * summedAreaTable[i - 1];

        return lowerBound;
    }


}
