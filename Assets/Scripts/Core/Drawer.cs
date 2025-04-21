using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class Drawer : MonoBehaviour
{
    public Transform drawsParent; // Parent for all draws
    public DrawObject drawObject; // Object to place and scale, if null - draw lines

    [Header("Line Settings")]
    public Color lineColor = Color.blue;
    public float lineWidth = 0.1f;

    private bool isDrawing = false;
    private Vector2 startPoint;
    private Vector2 currentPoint;
    private GameObject activeDrawing;
    private Material lineMaterial;

    private void Start()
    {
        // Create material for lines
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = lineColor;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && isDrawing)
        {
            CancelDrawing();
            isDrawing = false;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPoint = startPoint;
            isDrawing = true;
            CreateDrawing();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            UpdateDrawing();
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                CancelDrawing();
            }
            else
            {
                FinishDrawing();
            }
            isDrawing = false;
        }
    }

    private void CreateDrawing()
    {
        if (drawObject != null && drawObject.prefab != null)
        {
            activeDrawing = Instantiate(drawObject.prefab, startPoint, drawObject.prefab.transform.rotation);
            activeDrawing.transform.localScale = drawObject.prefab.transform.localScale * drawObject.minSize;
        }
        else
        {
            activeDrawing = new GameObject("DrawnLine");
            MeshFilter meshFilter = activeDrawing.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = activeDrawing.AddComponent<MeshRenderer>();
            meshRenderer.material = lineMaterial;
            UpdateLineMesh(Vector3.zero);
        }
        
        activeDrawing.transform.SetParent(drawsParent);
    }

    private void UpdateDrawing()
    {
        if (activeDrawing == null) return;

        Vector2 direction = (currentPoint - startPoint);
        float distance = direction.magnitude;

        if (drawObject != null && drawObject.prefab != null)
        {
            // Calculate scale based on distance and object settings, relative to prefab scale
            float scale = Mathf.Clamp(distance, drawObject.minSize, drawObject.maxSize);
            activeDrawing.transform.localScale = drawObject.prefab.transform.localScale * scale;
        }
        else
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            activeDrawing.transform.position = startPoint;
            activeDrawing.transform.rotation = Quaternion.Euler(0, 0, angle);
            UpdateLineMesh(new Vector3(distance, lineWidth, 1));
        }
    }

    private void FinishDrawing()
    {
        if (activeDrawing != null)
        {
            // Register the creation action for our custom undo system
            CreateAction currentAction = new CreateAction(activeDrawing);
            UndoManager.Instance.RegisterAction(currentAction);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(activeDrawing, "Drawing");
#endif
        }
        activeDrawing = null;
    }

    private void CancelDrawing()
    {
        if (activeDrawing != null)
        {
            Destroy(activeDrawing);
            activeDrawing = null;
        }
    }

    private void UpdateLineMesh(Vector3 scale)
    {
        if (activeDrawing == null) return;

        Mesh mesh = new Mesh();
        
        // Create a simple quad
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, -0.5f, 0),
            new Vector3(1, -0.5f, 0),
            new Vector3(0, 0.5f, 0),
            new Vector3(1, 0.5f, 0)
        };

        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        activeDrawing.GetComponent<MeshFilter>().mesh = mesh;
        activeDrawing.transform.localScale = scale;
    }
} 