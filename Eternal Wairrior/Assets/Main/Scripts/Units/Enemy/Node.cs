using UnityEngine;

public class Node
{
    #region Properties
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public float gCost;
    public float hCost;
    public float fCost;

    public Node previousNode;
    #endregion

    #region Constructor
    public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
    #endregion

    #region Methods
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    #endregion
}