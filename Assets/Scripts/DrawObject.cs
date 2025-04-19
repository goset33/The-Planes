using UnityEngine;

[CreateAssetMenu(fileName = "New Draw Object", menuName = "Drawing/Draw Object")]
public class DrawObject : ScriptableObject
{
    [Header("Object Info")]
    public string objectName;
    public Sprite icon;

    [Header("Drawing Settings")]
    public GameObject prefab;
    public float minSize = 0.1f;
    public float maxSize = 10f;
} 