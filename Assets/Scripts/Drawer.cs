using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawer : MonoBehaviour
{
    [Header("Line Settings")]
    public Canvas parentCanvas;
    public Transform lineParent;
    Vector3 startPosition;
    GameObject currentLineObject;
    LineRenderer currentLineRenderer;
    public Material lineMaterial;
    public float alpha = 1;
    public float lineThickness;
    public float simplifyTolerance = 0.01f;
    public bool canDraw = true;
    int syncDot = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canDraw)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawingLine();
            }
            else if (Input.GetMouseButton(0) && currentLineRenderer != null)
            {
                PreviewLine();
            }
            else if (Input.GetMouseButtonUp(0) && currentLineRenderer != null)
            {
                EndDrawingLine();
            }
        }
    }

    Vector3 GetMousePosition()
    {
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition, parentCanvas.worldCamera,
            out movePos);
        Vector3 positionToReturn = parentCanvas.transform.TransformPoint(movePos);
        positionToReturn.z = parentCanvas.transform.position.z - 0.01f;
        return positionToReturn;
    }

    void StartDrawingLine()
    {
        startPosition = GetMousePosition();
        currentLineObject = new GameObject();
        currentLineObject.transform.position = startPosition;
        currentLineObject.transform.SetParent(lineParent);
        currentLineRenderer = currentLineObject.AddComponent<LineRenderer>();
        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startWidth = lineThickness;
        currentLineRenderer.endWidth = lineThickness;

        Gradient gradient = new Gradient();
        GradientAlphaKey key = new GradientAlphaKey();
        key.alpha = alpha;
        gradient.alphaKeys = new GradientAlphaKey[] { key, key };
        currentLineRenderer.colorGradient = gradient;

        currentLineRenderer.positionCount = 1;
        currentLineRenderer.SetPosition(0, startPosition);

        GetComponent<UConnect>().SendRequest("send", "draw", startPosition.x.ToString("n2"), startPosition.y.ToString("n2"), "true");
        syncDot = currentLineRenderer.positionCount;
    }

    void PreviewLine()
    {
        Vector3 lastPosition = GetMousePosition();
        currentLineRenderer.positionCount++;
        currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, lastPosition);

        startPosition = lastPosition;
        currentLineRenderer.Simplify(simplifyTolerance);

        Vector3 lastPos = currentLineRenderer.GetPosition(currentLineRenderer.positionCount - 1);
        if (currentLineRenderer.positionCount - 1 != syncDot)
        {
            GetComponent<UConnect>().SendRequest("send", "draw", lastPos.x.ToString("n2"), lastPos.y.ToString("n2"), "false");
            syncDot = currentLineRenderer.positionCount;
        }
    }

    void EndDrawingLine()
    {
        Vector3 lastPos = currentLineRenderer.GetPosition(currentLineRenderer.positionCount - 1);
        GetComponent<UConnect>().SendRequest("send", "draw", lastPos.x.ToString("n2"), lastPos.y.ToString("n2"), "false");
        syncDot = 0;

        startPosition = Vector3.zero;
        currentLineObject = null;
        currentLineRenderer = null;
    }

    public void DrawOnNetwork(NetworkData data)
    {
        if (canDraw)
            return;

        float x = float.Parse(data.array[0]);
        float y = float.Parse(data.array[1]);
        Vector3 dot = new Vector3(x, y, 0);

        if (bool.Parse(data.array[2]))
        {
            currentLineObject = new GameObject();
            currentLineObject.transform.position = dot;
            currentLineObject.transform.SetParent(lineParent);
            currentLineRenderer = currentLineObject.AddComponent<LineRenderer>();
            currentLineRenderer.material = lineMaterial;
            currentLineRenderer.startWidth = lineThickness;
            currentLineRenderer.endWidth = lineThickness;

            Gradient gradient = new Gradient();
            GradientAlphaKey key = new GradientAlphaKey();
            key.alpha = alpha;
            gradient.alphaKeys = new GradientAlphaKey[] { key, key };
            currentLineRenderer.colorGradient = gradient;

            currentLineRenderer.positionCount = 1;
            currentLineRenderer.SetPosition(0, dot);
        }
        else
        {
            currentLineRenderer.positionCount++;
            currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, dot);
        }
    }

    public void ClearLines()
    {
        int count = lineParent.childCount;

        for (int i = 0; i < count; i++)
        {
            Destroy(lineParent.GetChild(i).gameObject);
        }
    }
}
