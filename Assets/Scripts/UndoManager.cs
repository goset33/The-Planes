using UnityEngine;
using System.Collections.Generic;

public class UndoManager : MonoBehaviour
{
    private static UndoManager instance;
    public static UndoManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UndoManager");
                instance = go.AddComponent<UndoManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Stack<UndoableAction> undoStack = new Stack<UndoableAction>();

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastAction();
        }
    }

    public void RegisterAction(UndoableAction action)
    {
        undoStack.Push(action);
    }

    private void UndoLastAction()
    {
        if (undoStack.Count > 0)
        {
            UndoableAction action = undoStack.Pop();
            action.Undo();
        }
    }
}

[System.Serializable]
public abstract class UndoableAction
{
    public abstract void Undo();
}

[System.Serializable]
public class TransformAction : UndoableAction
{
    private Transform target;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    public TransformAction(Transform target)
    {
        this.target = target;
        this.originalPosition = target.position;
        this.originalRotation = target.rotation;
        this.originalScale = target.localScale;
    }

    public override void Undo()
    {
        if (target != null)
        {
            target.position = originalPosition;
            target.rotation = originalRotation;
            target.localScale = originalScale;
        }
    }
}

[System.Serializable]
public class CreateAction : UndoableAction
{
    private GameObject createdObject;

    public CreateAction(GameObject createdObject)
    {
        this.createdObject = createdObject;
    }

    public override void Undo()
    {
        if (createdObject != null)
        {
            GameObject.Destroy(createdObject);
            createdObject = null;
        }
    }
} 