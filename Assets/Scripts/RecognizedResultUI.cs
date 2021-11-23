using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class RecognizedResultUI : MonoBehaviour
{
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI previewText;

    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        QDollarRecognizer.OnTemplateRecognized += ShowTemplate;
    }

    private void OnDisable()
    {
        QDollarRecognizer.OnTemplateRecognized -= ShowTemplate;
    }

    private void ShowTemplate(DrawingTemplate recognizedTemplate, int computationTime)
    {
        canvasGroup.alpha = 1f;
        
        previewImage.sprite = recognizedTemplate.sprite;
        previewText.SetText($"{recognizedTemplate.name}, {computationTime}ms");
  
    }
}
