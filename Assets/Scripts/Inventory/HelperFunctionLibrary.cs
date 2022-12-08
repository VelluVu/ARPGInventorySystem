using UnityEngine;

public class HelperFunctionLibrary
{
    public static Vector2 GetLocalMousePositionOfRectTransform(Vector2 mousePosition, RectTransform rectTransform)
    {
        Vector2 localMousePosition = rectTransform.InverseTransformPoint(mousePosition);
        return localMousePosition;
    }

    public static Vector2 LocalPositionToVisualCoordinate(Vector2 position, Vector2 visualCellSize)
    {
        Vector2 scalar = visualCellSize * 0.001f;
        position *= scalar;
        position = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.y + 1f));
        return position;
    }

    public static Vector2Int PositionToCellCoordinate(Vector2 position)
    {      
        Vector2Int cellCoordinate = new Vector2Int(Mathf.RoundToInt(Mathf.Abs(position.x)), Mathf.RoundToInt(Mathf.Abs(position.y)));
        return cellCoordinate;
    }

    public static Vector2 SnapToCellPosition(Vector2 position, Vector2 rectSizeDelta, Vector2 cellSize)
    {
        position *= cellSize;
        Vector2 offset = new Vector2(rectSizeDelta.x * 0.5f, rectSizeDelta.y * -0.5f);
        position += offset;
        return position;
    }
}
