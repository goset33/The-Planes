using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using TMPro;

public class DrawingController : MonoBehaviour
{
    [Header("References")]
    public Drawer drawer;
    public Transform currentObject;
    public GameObject objectCardPrefab;
    public Transform cardsContainer;
    public GameObject drawingWindow;

    [Header("Drawing Objects")]
    public DrawObject[] drawableObjects;

    private DrawObject selectedObject;

    private void Start()
    {
        if (drawer == null)
        {
            throw new NullReferenceException("Drawer reference is not set!");
        }

        CreateObjectCards();
        UpdateSelectedObjectDisplay(drawableObjects[0]);
        drawingWindow.SetActive(false);
    }

    private void CreateObjectCards()
    {
        // Clear existing cards
        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsContainer.GetChild(i).gameObject);
        }

        // Create new cards
        foreach (var drawObject in drawableObjects)
        {
            Transform card = Instantiate(objectCardPrefab, cardsContainer).transform;
            
            // Set up card UI
            Image iconImage = card.GetChild(0).GetComponent<Image>();
            iconImage.sprite = drawObject.icon;

            TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            nameText.text = drawObject.objectName;

            // Add click handler
            Button button = card.GetComponent<Button>();
            button.onClick.AddListener(() => SelectObject(drawObject));
        }
    }

    public void SelectObject(DrawObject drawObject)
    {
        selectedObject = drawObject;
        drawer.drawObject = drawObject;
        drawingWindow.SetActive(false);
        UpdateSelectedObjectDisplay(drawObject);
    }

    private void UpdateSelectedObjectDisplay(DrawObject drawObject)
    {
        Image iconImage = currentObject.GetChild(0).GetComponent<Image>();
        iconImage.sprite = drawObject.icon;

        TextMeshProUGUI nameText = currentObject.GetComponentInChildren<TextMeshProUGUI>();
        nameText.text = drawObject.objectName;
    }

    // Call this from a UI button to toggle the drawing window
    public void ToggleDrawingWindow()
    {
        drawingWindow.SetActive(!drawingWindow.activeSelf);
    }
} 