using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class Rotator : MonoBehaviour
{
    public Transform rotationTarget; // Transform that contains objects to rotate
    public Color sectorColor = Color.yellow;
    public Color dotColor = Color.red;

    private Vector3 rotationPoint;
    private bool isRotating = false;
    private float initialAngle;
    private float lastAngle;
    private float cumulativeAngle;
    private int rotationDirection = 0; // 1 for clockwise, -1 for counterclockwise

    private LineRenderer lineRenderer;
    private TextMeshPro angleText;
    private GameObject rotationPointIndicator;
    private MeshFilter sectorMeshFilter;
    private MeshRenderer sectorMeshRenderer;
    private Mesh sectorMesh;

    void Start()
    {
        // Create container for all visual objects
        Transform visualContainer = transform;
        
        // Create and setup line renderer
        GameObject lineObj = new GameObject("RotationArc");
        lineObj.transform.SetParent(visualContainer);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        // Create material for line
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = sectorColor;
        lineRenderer.material = lineMaterial;
        lineRenderer.sortingOrder = 100; 
        lineRenderer.startColor = sectorColor;
        lineRenderer.endColor = sectorColor;
        lineRenderer.positionCount = 0;

        // Create rotation point indicator
        rotationPointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rotationPointIndicator.transform.SetParent(visualContainer);
        rotationPointIndicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        // Set material for the dot
        var renderer = rotationPointIndicator.GetComponent<Renderer>();
        Material dotMaterial = new Material(Shader.Find("Sprites/Default"));
        dotMaterial.color = dotColor;
        renderer.material = dotMaterial;
        renderer.sortingOrder = 101;
        rotationPointIndicator.SetActive(false);

        // Create sector mesh
        GameObject sectorObj = new GameObject("RotationSector");
        sectorObj.transform.SetParent(visualContainer);
        sectorMeshFilter = sectorObj.AddComponent<MeshFilter>();
        sectorMeshRenderer = sectorObj.AddComponent<MeshRenderer>();
        // Create material for sector
        Material sectorMaterial = new Material(Shader.Find("Sprites/Default"));
        sectorMaterial.color = new Color(sectorColor.r, sectorColor.g, sectorColor.b, 0.3f);
        sectorMeshRenderer.material = sectorMaterial;
        sectorMeshRenderer.sortingOrder = 99;
        sectorObj.SetActive(false);

        // Initialize sector mesh
        sectorMesh = new Mesh();
        sectorMesh.name = "Sector Mesh";
        sectorMeshFilter.mesh = sectorMesh;

        // Create angle text
        GameObject textObj = new GameObject("AngleText");
        textObj.transform.SetParent(visualContainer);
        angleText = textObj.AddComponent<TextMeshPro>();
        angleText.fontSize = 2.5f;
        angleText.fontStyle = FontStyles.Bold;
        angleText.alignment = TextAlignmentOptions.Center;
        angleText.color = Color.black;
        angleText.sortingOrder = 100;
        angleText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Don't process input if over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Check for right-click cancel first
        if (Input.GetMouseButtonDown(1))
        {
            if (isRotating)
            {
                isRotating = false;
                HideVisualizations();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            rotationPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rotationPoint.z = 0;

            // Show rotation point
            rotationPointIndicator.transform.position = rotationPoint;
            rotationPointIndicator.SetActive(true);

            // Initialize visualization with exact mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            UpdateRotationVisualization(mousePos);
        }
        else if (Input.GetMouseButton(0) && isRotating)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePos.z = 0;
            UpdateRotationVisualization(currentMousePos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isRotating)
            {
                Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentMousePos.z = 0;
                
                // Apply final rotation to the target transform
                rotationTarget.RotateAround(rotationPoint, Vector3.forward, -cumulativeAngle);

                HideVisualizations();
            }
            isRotating = false;
        }
    }

    private float GetNormalizedAngle(Vector3 point)
    {
        float angle = Mathf.Atan2(point.y - rotationPoint.y, point.x - rotationPoint.x) * Mathf.Rad2Deg;
        // Normalize to make 0 at the top
        return (angle + 270) % 360;
    }

    private void UpdateRotationVisualization(Vector3 mousePos)
    {
        float radius = Vector3.Distance(rotationPoint, mousePos);
        
        // Calculate current angle from top (0 degrees)
        float currentAngle = GetNormalizedAngle(mousePos);

        if (isRotating)
        {
            // Handle angle wrapping for continuous rotation
            float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);
            
            // Determine rotation direction on first movement
            if (rotationDirection == 0 && deltaAngle != 0)
            {
                rotationDirection = deltaAngle > 0 ? 1 : -1;
            }

            // Invert delta angle to make clockwise positive
            cumulativeAngle -= deltaAngle;
            
            // Keep angle in reasonable range to prevent floating point issues
            cumulativeAngle = cumulativeAngle % 360;
            
            lastAngle = currentAngle;

            // Reset visualization when reaching full circle
            if (Mathf.Abs(cumulativeAngle) >= 360)
            {
                cumulativeAngle = 0;
                lastAngle = currentAngle;
            }
        }
        else
        {
            // Initialize angles when starting rotation
            lastAngle = currentAngle;
            cumulativeAngle = 0;
            rotationDirection = 0;
        }

        // Calculate angles for visualization
        float startAngleRad = Mathf.PI / 2;  // Start from top (90 degrees)
        float angleDiff = -(cumulativeAngle * Mathf.Deg2Rad);  // Convert to radians
        float currentAngleRad = startAngleRad + angleDiff;

        // Update sector mesh
        UpdateSectorMesh(radius, startAngleRad, currentAngleRad);

        // Update line renderer for the arc
        int segments = 20;
        lineRenderer.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = startAngleRad + angleDiff * t;
            Vector3 point = rotationPoint + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lineRenderer.SetPosition(i, point);
        }

        // Update angle text
        string angleTextStr = $"{cumulativeAngle:F1}°";
        angleText.text = angleTextStr;
        
        // Position text outside the sector along its middle angle
        float midAngle = startAngleRad + angleDiff * 0.5f;
        Vector3 midDirection = new Vector3(Mathf.Cos(midAngle), Mathf.Sin(midAngle), 0);
        Vector3 textPosition = rotationPoint + midDirection * (radius + 0.5f);
        angleText.transform.position = textPosition;
        
        angleText.gameObject.SetActive(true);
    }

    private void UpdateSectorMesh(float radius, float startAngle, float endAngle)
    {
        int segments = 20;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        // Center point
        vertices[0] = rotationPoint;

        // Create vertices
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = startAngle + (endAngle - startAngle) * t;
            vertices[i + 1] = rotationPoint + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        }

        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // Update mesh
        sectorMesh.Clear();
        sectorMesh.vertices = vertices;
        sectorMesh.triangles = triangles;
        sectorMeshFilter.mesh = sectorMesh;
        sectorMeshFilter.gameObject.SetActive(true);
    }

    private void HideVisualizations()
    {
        // Hide visualizations
        lineRenderer.positionCount = 0;
        rotationPointIndicator.SetActive(false);
        angleText.gameObject.SetActive(false);
        sectorMeshFilter.gameObject.SetActive(false);
    }
}
