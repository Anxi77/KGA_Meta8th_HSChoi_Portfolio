using UnityEngine;
using System.Collections.Generic;

public class PathFinding : MonoBehaviour
{
    private static PathFinding instance;
    public static PathFinding Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PathFinding>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PathFinding");
                    instance = go.AddComponent<PathFinding>();
                }
            }
            return instance;
        }
    }

    private const float NODE_RADIUS = 0.25f;
    private const int GRID_SIZE = 100; // 맵 크기에 따라 조절
    private PathNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    private Vector2 gridWorldSize = new Vector2(50, 50); // 맵 크기에 맞게 조정

    private void Awake()
    {
        nodeDiameter = NODE_RADIUS * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + NODE_RADIUS) + Vector2.up * (y * nodeDiameter + NODE_RADIUS);
                bool walkable = !Physics2D.CircleCast(worldPoint, NODE_RADIUS * 0.8f, Vector2.zero, 0f, LayerMask.GetMask("Obstacle"));
                grid[x, y] = new PathNode(worldPoint, x, y, walkable);
            }
        }
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        PathNode startNode = NodeFromWorldPoint(startPos);
        PathNode targetNode = NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null) return null;
        if (!startNode.isWalkable) startNode.isWalkable = true; // 시작 위치는 항상 이동 가능하게
        if (!targetNode.isWalkable) targetNode.isWalkable = true; // 목표 위치도 이동 가능하게

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return SimplifyPath(RetracePath(startNode, targetNode));
            }

            foreach (PathNode neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

                float newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    float GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    PathNode NodeFromWorldPoint(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    // 경로 단순화 - 불필요한 중간 지점 제거
    private List<Vector2> SimplifyPath(List<Vector2> path)
    {
        if (path == null || path.Count < 3) return path;

        List<Vector2> simplifiedPath = new List<Vector2>();
        Vector2 oldDirection = Vector2.zero;

        simplifiedPath.Add(path[0]);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 newDirection = (path[i + 1] - path[i - 1]).normalized;
            if (Vector2.Angle(newDirection, oldDirection) > 10f)
            {
                simplifiedPath.Add(path[i]);
                oldDirection = newDirection;
            }
        }

        simplifiedPath.Add(path[path.Count - 1]);
        return simplifiedPath;
    }
}