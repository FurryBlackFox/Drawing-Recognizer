using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Manager : MonoBehaviour
{
    
    [SerializeField] private QDollarRecognizer qDollarRecognizer;
    [SerializeField] private Camera screenshotCamera;
    [SerializeField] private Settings settings;
    
    private Camera mainCamera;
    private DrawingLine currentLine;
    private bool isDrawing = false;

    private List<DrawingLine> drawingLines;

    private void Awake()
    {
        mainCamera = Camera.main;
        drawingLines = new List<DrawingLine>();
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTrackingInput();
            isDrawing = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            return;
        }
        
        if(isDrawing)
            CheckInput();
    }

    #region Input

    private void CheckInput()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, settings.maxRaycastDistance, settings.drawerInteractionMask))
        {
            if (currentLine.TryGetLastMarkerPos(out var lastPos))
            {
                var testedPos = hitInfo.point;
                var sqrDistance = (testedPos - lastPos).sqrMagnitude;
                if(sqrDistance < settings.minSqrDistanceBetweenPoints)
                    return;
            }

            var position = hitInfo.point;
            position.y = 0f;
            
            currentLine.AddMarker(position);
        }
    }

    private void StartTrackingInput()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, settings.maxRaycastDistance, settings.drawerInteractionMask))
            return;

        if(currentLine && currentLine.GetAllMarkers().Count == 1)
            currentLine.Clear();
        else
        {
            currentLine = Instantiate(settings.drawingLinePrefab, transform);
            drawingLines.Add(currentLine);
        }
    }

    #endregion

    #region Saving

    public void SaveToTemplate()
    {
        var tempDrawing = new Drawing(TransformLinesToPoints());
        if (!tempDrawing.IsNormalized)
            tempDrawing.Normalize(settings.pointsSamplesCount, settings.matrixSize);

        var template = ScriptableObject.CreateInstance<DrawingTemplate>();
        template.drawing = tempDrawing;


        var screenshot = MakeScreenshot(template);
        var screenshotPath = SaveScreenshot(screenshot);
        if (screenshotPath == "")
            return;

        var loadedAssets = AssetDatabase.LoadAllAssetsAtPath(screenshotPath);
        template.image = loadedAssets.OfType<Texture2D>().First();
        template.sprite = loadedAssets.OfType<Sprite>().First();

        var templatePath = SaveTemplateAsset(template);
        if (templatePath == "")
            return;

        if (!settings.drawingTemplates.Any(x => x.name == template.name))
        {
            settings.drawingTemplates.Add(template);
        }


    }
    
    private Texture2D MakeScreenshot(DrawingTemplate template)
    {
        ClearLines();
        GeneratePreviewLines(template);
        
        screenshotCamera.Render();
        ClearLines();
        
        var oldRT = RenderTexture.active;
        var screenshotRT = screenshotCamera.targetTexture;
        RenderTexture.active = screenshotRT;
        var tex2d = new Texture2D(screenshotRT.width, screenshotRT.height);
        tex2d.ReadPixels(new Rect(0, 0, screenshotRT.width, screenshotRT.height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = oldRT;
        return tex2d;
    }
    
    private void GeneratePreviewLines(DrawingTemplate template)
    {
        var previewPoints = template.drawing.GetScaledInitialPoints(settings.matrixSize);
        
        currentLine = null;
        for (var i = 0; i < previewPoints.Count; i++)
        {
            var currentPoint = previewPoints[i];
            var currentPos = new Vector3(currentPoint.position.y, 0, currentPoint.position.x);
            if (i == 0 || currentPoint.strokeId != previewPoints[i - 1].strokeId)
            {
                currentLine = Instantiate(settings.drawingLinePrefab);
                drawingLines.Add(currentLine);
            }
            currentLine.AddMarker(currentPos);
        }
    }

    private string SaveScreenshot(Texture2D screenshot)
    {
        var path = EditorUtility.SaveFilePanelInProject("Saving preview png", "New shot", "png", 
            "meh", settings.defaultTemplateImageSavingPath);
        
        if (path != "")
        {
            File.WriteAllBytes(path, screenshot.EncodeToPNG());
            AssetDatabase.Refresh();
            
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.Refresh();

        }
        
        return path;
    }
    
    private string SaveTemplateAsset(DrawingTemplate template)
    {
        var path = EditorUtility.SaveFilePanelInProject("Saving template",
            "Saved Template", "asset", "meh", settings.defaultTemplateSavingPath);
        if (path != "")
        {
            AssetDatabase.CreateAsset(template, path);
            EditorUtility.SetDirty(template);
            AssetDatabase.Refresh();
        }
        
        return path;
    }
    

    #endregion

    #region LinesTransformation

    private List<Point> TransformLinesToPoints()
    {
        var outputPoints = new List<Point>();

        var i = 0;
        foreach (var drawingLine in drawingLines)
        {
            foreach (var marker in drawingLine.GetAllMarkers())
            {
                var newPoint = new Point {position = new Vector2(marker.z, marker.x), strokeId = i};
                outputPoints.Add(newPoint);
            }
            i++;
        }
        return outputPoints;
    }

    #endregion
    


    #region Public

    public void Recognize()
    {
        var tempDrawing = new Drawing(TransformLinesToPoints());
        qDollarRecognizer.Recognize(tempDrawing);   
    }

    public void ClearLines()
    {
        foreach (var drawingLine in drawingLines)
        { 
            DestroyImmediate(drawingLine.gameObject);
        }
        drawingLines.Clear();
    }

    public void ResampleTemplates()
    {
        foreach (var drawingTemplate in settings.drawingTemplates)
        {
            drawingTemplate.drawing.Normalize(settings.pointsSamplesCount, settings.matrixSize);
            EditorUtility.SetDirty(drawingTemplate);
            AssetDatabase.Refresh();
        }
   
    }

    #endregion
    
 
}
