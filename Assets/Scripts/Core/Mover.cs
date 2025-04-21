using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Mover : MonoBehaviour
{
    public Color vectorColor = Color.yellow;

    private bool isDragging = false;
    private Vector2 startPoint;
    private Vector2 currentPoint;

    private LineRenderer lineRenderer;
    private GameObject arrowHead;
    private SpriteRenderer arrowRenderer;
    private TextMeshPro coordinateText;

    void Start()
    {
        SetupLineRenderer();
        CreateArrowHead();
        CreateCoordinateText();
    }

    void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = vectorColor;
        lineRenderer.endColor = vectorColor;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void CreateArrowHead()
    {
        arrowHead = new GameObject("ArrowHead");
        arrowHead.transform.SetParent(transform);
        arrowRenderer = arrowHead.AddComponent<SpriteRenderer>();
        
        // Create a triangle sprite for the arrowhead
        Texture2D arrowTexture = new Texture2D(32, 32);
        for (int y = 0; y < arrowTexture.height; y++)
        {
            for (int x = 0; x < arrowTexture.width; x++)
            {
                // Create a triangle shape
                if (x <= y && x <= (arrowTexture.width - y))
                {
                    arrowTexture.SetPixel(x, y, vectorColor);
                }
                else
                {
                    arrowTexture.SetPixel(x, y, Color.clear);
                }
            }
        }
        arrowTexture.Apply();

        // Create sprite from texture
        Sprite arrowSprite = Sprite.Create(arrowTexture, new Rect(0, 0, arrowTexture.width, arrowTexture.height), new Vector2(0, 0.5f), 100);
        arrowRenderer.sprite = arrowSprite;
        arrowRenderer.sortingOrder = 100;
        arrowRenderer.enabled = false;
        
        // Set initial scale
        arrowHead.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }

    void CreateCoordinateText()
    {
        GameObject textObject = new GameObject("CoordinateText");
        textObject.transform.SetParent(transform);
        coordinateText = textObject.AddComponent<TextMeshPro>();
        coordinateText.alignment = TextAlignmentOptions.Center;
        coordinateText.fontSize = 2.5f;
        coordinateText.fontStyle = FontStyles.Bold;
        coordinateText.color = vectorColor;
        coordinateText.sortingOrder = 100;
        coordinateText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPoint = startPoint;
            isDragging = true;
            
            ShowVisualizations();
            
            // Set both line points to the same position initially
            Vector3 point = new Vector3(startPoint.x, startPoint.y, 0);
            lineRenderer.SetPosition(0, point);
            lineRenderer.SetPosition(1, point);
            
            // Set arrow position and rotation
            arrowHead.transform.position = point;
            arrowHead.transform.rotation = Quaternion.identity;
            
            // Update text position and content
            UpdateCoordinateText(point, Vector2.zero);
        }

        if (isDragging)
        {
            currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = currentPoint - startPoint;
            
            // Update line position
            lineRenderer.SetPosition(0, new Vector3(startPoint.x, startPoint.y, 0));
            lineRenderer.SetPosition(1, new Vector3(currentPoint.x, currentPoint.y, 0));
            
            // Update arrow position and rotation
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // Position the arrow slightly back from the end point to overlap with the line
                Vector3 arrowOffset = direction.normalized * -0.05f;
                Vector3 arrowPosition = new Vector3(currentPoint.x, currentPoint.y, 0) + arrowOffset;
                arrowHead.transform.position = arrowPosition;
                arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                // Update text position and content
                UpdateCoordinateText(arrowPosition, direction);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // Register the start of the transform action
                TransformAction currentAction = new TransformAction(Camera.main.transform);
                UndoManager.Instance.RegisterAction(currentAction);

                Vector2 drawnVector = currentPoint - startPoint;
                Vector2 oppositeVector = -drawnVector;
                
                Camera.main.transform.position += new Vector3(oppositeVector.x, oppositeVector.y, 0);
            }

            isDragging = false;
            HideVisualizations();
        }
        else if (Input.GetMouseButtonDown(1) && isDragging) // Right click to cancel
        {
            isDragging = false;
            HideVisualizations();
        }
    }

    private void ShowVisualizations()
    {
        lineRenderer.enabled = true;
        arrowRenderer.enabled = true;
        coordinateText.gameObject.SetActive(true);
    }

    private void HideVisualizations()
    {
        lineRenderer.enabled = false;
        arrowRenderer.enabled = false;
        coordinateText.gameObject.SetActive(false);
    }

    void UpdateCoordinateText(Vector3 position, Vector2 vector)
    {
        if (vector == Vector2.zero)
        {
            coordinateText.transform.position = position + new Vector3(0, 0.2f, 0);
            coordinateText.text = "(0.00, 0.00)";
            return;
        }

        // Calculate the true middle point between start and end positions
        Vector3 midPoint = new Vector3(
            (startPoint.x + currentPoint.x) / 2,
            (startPoint.y + currentPoint.y) / 2,
            0
        );

        // Calculate normalized perpendicular vector (normal to the vector direction)
        Vector2 direction = (currentPoint - startPoint).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        // Scale offset based on how vertical the vector is
        float verticalFactor = Mathf.Abs(direction.y); // 0 for horizontal, 1 for vertical
        float scaledOffset = Mathf.Lerp(0.3f, 1f, verticalFactor);
        
        Vector3 offset = new Vector3(perpendicular.x, perpendicular.y, 0) * scaledOffset;
        
        // Position the text at midpoint offset by the perpendicular vector
        coordinateText.transform.position = midPoint + offset;
        
        // Format the vector coordinates to 2 decimal places
        coordinateText.text = $"({vector.x:F2}, {vector.y:F2})";
    }
}
