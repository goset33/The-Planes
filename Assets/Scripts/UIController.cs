using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public enum ToolState
    {
        None,
        Draw,
        Move,
        Rotate
    }

    [Header("Tools")]
    [SerializeField] private Drawer drawer;
    [SerializeField] private Mover mover;
    [SerializeField] private Rotator rotator;

    [Header("Tool Buttons")]
    [SerializeField] private Image drawButton;
    [SerializeField] private Image moveButton;
    [SerializeField] private Image rotateButton;

    [Header("Button Colors")]
    public Color defaultButtonColor = new Color(0.4811321f, 0.4811321f, 0.4811321f, 1f);
    public Color activeButtonColor = new Color(0.4831346f, 0.7264151f, 0.5051998f, 1f);

    private ToolState currentState = ToolState.None;

    private void Start()
    {
        SetToolState(ToolState.None);
    }

    private void SetToolState(ToolState newState)
    {
        // Reset all button colors
        drawButton.color = defaultButtonColor;
        moveButton.color = defaultButtonColor;
        rotateButton.color = defaultButtonColor;

        // Disable all tools first
        drawer.gameObject.SetActive(false);
        mover.gameObject.SetActive(false);
        rotator.gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);

        // Enable the selected tool and highlight its button
        switch (newState)
        {
            case ToolState.Draw:
                drawer.gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(true);
                drawButton.color = activeButtonColor;
                break;
            case ToolState.Move:
                mover.gameObject.SetActive(true);
                moveButton.color = activeButtonColor;
                break;
            case ToolState.Rotate:
                rotator.gameObject.SetActive(true);
                rotateButton.color = activeButtonColor;
                break;
        }

        currentState = newState;
    }

    // Button click handlers
    public void EnableDrawTool()
    {
        if (currentState != ToolState.Draw)
        {
            SetToolState(ToolState.Draw);
        }
        else
        {
            SetToolState(ToolState.None);
        }
    }

    public void EnableMoveTool()
    {
        if (currentState != ToolState.Move)
        {
            SetToolState(ToolState.Move);
        }
        else
        {
            SetToolState(ToolState.None);
        }
    }

    public void EnableRotateTool()
    {
        if (currentState != ToolState.Rotate)
        {
            SetToolState(ToolState.Rotate);
        }
        else
        {
            SetToolState(ToolState.None);
        }
    }
}
