using UnityEngine;

public class HelperFunctionLibrary
{
    public static Vector2 GetLocalMousePositionOfRectTransform(Vector2 mousePosition, RectTransform rectTransform)
    {
        //Debug.Log("Mouse position: " + mousePosition);
        Vector2 localMousePosition = rectTransform.InverseTransformPoint(mousePosition);
        //Debug.Log("Local mouse position: " + localMousePosition);
        return localMousePosition;
    }

    public static Vector2 LocalPositionToVisualCoordinate(Vector2 position, Vector2 visualCellSize)
    {
        //Debug.LogFormat("Dropped position: {0}", position);
        Vector2 scalar = visualCellSize * 0.001f;
        position *= scalar;
        //Debug.LogFormat("Dropped position to cell scale {0}", position);
        position = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.y + 1f));
        return position;
    }

    public static Vector2Int PositionToCellCoordinate(Vector2 position)
    {      
        Vector2Int cellCoordinate = new Vector2Int(Mathf.RoundToInt(Mathf.Abs(position.x)), Mathf.RoundToInt(Mathf.Abs(position.y)));
        //Debug.LogFormat("Dropped position floored {0}", position);
        return cellCoordinate;
    }

    public static Vector2 SnapToCellPosition(Vector2 position, Vector2 rectSizeDelta, Vector2 cellSize)
    {
        //Debug.LogFormat("Dropped position before snapping {0} and rect size {1}", position, rectSizeDelta);
        position *= cellSize;
        //Debug.LogFormat("Dropped position multiplied by cell size {0} is {1}", cellSize, position);

        Vector2 offset = new Vector2(rectSizeDelta.x * 0.5f, rectSizeDelta.y * -0.5f);
        position += offset;
        //Debug.LogFormat("Dropped position offsetted by {0} is {1}", offset, position);

        return position;
    }
}
