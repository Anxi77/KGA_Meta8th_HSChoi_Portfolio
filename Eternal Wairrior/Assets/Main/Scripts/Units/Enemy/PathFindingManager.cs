using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class PathFindingManager : MonoBehaviour
{
    #region Singleton
    private static PathFindingManager instance;
    public static PathFindingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PathFindingManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PathFindingManager");
                    instance = go.AddComponent<PathFindingManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region Fields & Properties
    private const int GRID_SIZE = 100;
    public const float NODE_SIZE = 1f;

    private Node[,] grid;
    private List<Node> openSet;
    private HashSet<Node> closedSet;
    private Vector2 gridWorldCenter;
    private Vector2 bottomLeft;
    #endregion

    #region Object Pooling
    private class PathFindingInstance
    {
        public List<Node> openSet = new List<Node>(GRID_SIZE);
        public HashSet<Node> closedSet = new HashSet<Node>();
        public List<Vector2> path = new List<Vector2>(50);
    }

    private Queue<PathFindingInstance> instancePool = new Queue<PathFindingInstance>();
    private const int INITIAL_POOL_SIZE = 20;

    private void InitializePools()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            instancePool.Enqueue(new PathFindingInstance());
        }
    }

    private PathFindingInstance GetPathFindingInstance()
    {
        if (instancePool.Count == 0)
        {
            return new PathFindingInstance();
        }
        var instance = instancePool.Dequeue();
        instance.openSet.Clear();
        instance.closedSet.Clear();
        instance.path.Clear();
        return instance;
    }

    private void ReturnPathFindingInstance(PathFindingInstance instance)
    {
        instancePool.Enqueue(instance);
    }
    #endregion

    #region Initialization & Setup
    private void Awake()
    {
        InitializeSingleton();
        InitializePools();
        CreateGrid();
    }

    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Grid Management
    private void CreateGrid()
    {
        InitializeGridCenter();
        InitializeNodes();
    }

    private void InitializeGridCenter()
    {
        grid = new Node[GRID_SIZE, GRID_SIZE];
        gridWorldCenter = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? Vector2.zero;
        bottomLeft = gridWorldCenter - new Vector2(GRID_SIZE * NODE_SIZE / 2, GRID_SIZE * NODE_SIZE / 2);
    }

    private void InitializeNodes()
    {
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                CreateNode(x, y);
            }
        }
    }

    private void CreateNode(int x, int y)
    {
        Vector2 worldPosition = bottomLeft + new Vector2(x * NODE_SIZE, y * NODE_SIZE);
        bool isWalkable = !Physics2D.OverlapCircle(worldPosition, NODE_SIZE * 0.4f, LayerMask.GetMask("Obstacle"));
        grid[x, y] = new Node(isWalkable, worldPosition, x, y);
    }
    #endregion

    #region Pathfinding Core
    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        var pathFindingInstance = GetPathFindingInstance();
        try
        {
            return ExecutePathfinding(startPos, targetPos, pathFindingInstance);
        }
        finally
        {
            ReturnPathFindingInstance(pathFindingInstance);
        }
    }

    private List<Vector2> ExecutePathfinding(Vector2 startPos, Vector2 targetPos, PathFindingInstance instance)
    {
        openSet = instance.openSet;
        closedSet = instance.closedSet;

        if (TryGetDirectPath(startPos, targetPos, out var directPath))
        {
            return directPath;
        }

        return PerformAStarPathfinding(startPos, targetPos);
    }

    private bool TryGetDirectPath(Vector2 startPos, Vector2 targetPos, out List<Vector2> directPath)
    {
        directPath = null;
        float distanceToTarget = Vector2.Distance(startPos, targetPos);

        if (distanceToTarget < NODE_SIZE * 2f && !Physics2D.Linecast(startPos, targetPos, LayerMask.GetMask("Obstacle")))
        {
            directPath = new List<Vector2> { startPos, targetPos };
            return true;
        }

        return false;
    }

    private List<Vector2> PerformAStarPathfinding(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = GetNodeFromWorldPosition(startPos);
        Node targetNode = GetNodeFromWorldPosition(targetPos);

        if (!ValidateNodes(ref startNode, ref targetNode))
        {
            return null;
        }

        InitializePathfindingNodes(startNode, targetNode);
        return ExecuteAStarAlgorithm(startNode, targetNode, startPos, targetPos);
    }

    private bool ValidateNodes(ref Node startNode, ref Node targetNode)
    {
        if (!startNode.walkable || !targetNode.walkable)
        {
            startNode = FindNearestWalkableNode(startNode);
            targetNode = FindNearestWalkableNode(targetNode);
            return startNode != null && targetNode != null;
        }
        return true;
    }

    private void InitializePathfindingNodes(Node startNode, Node targetNode)
    {
        int startX = Mathf.Max(0, startNode.gridX - 20);
        int endX = Mathf.Min(GRID_SIZE - 1, targetNode.gridX + 20);
        int startY = Mathf.Max(0, startNode.gridY - 20);
        int endY = Mathf.Min(GRID_SIZE - 1, targetNode.gridY + 20);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                grid[x, y].gCost = float.MaxValue;
                grid[x, y].CalculateFCost();
                grid[x, y].previousNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, targetNode);
        startNode.CalculateFCost();
    }

    private List<Vector2> ExecuteAStarAlgorithm(Node startNode, Node targetNode, Vector2 startPos, Vector2 targetPos)
    {
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);
            if (currentNode == targetNode)
            {
                var path = CalculatePath(targetNode);
                if (path != null && path.Count > 0)
                {
                    path[0] = startPos;
                    path[path.Count - 1] = targetPos;
                    return OptimizePath(path);
                }
                return null;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (closedSet.Contains(neighbour)) continue;
                if (!neighbour.walkable)
                {
                    closedSet.Add(neighbour);
                    continue;
                }

                float tentativeGCost = currentNode.gCost + CalculateDistance(currentNode, neighbour);
                if (tentativeGCost < neighbour.gCost)
                {
                    neighbour.previousNode = currentNode;
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = CalculateDistance(neighbour, targetNode);
                    neighbour.CalculateFCost();

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Path Optimization & Smoothing
    private List<Vector2> OptimizePath(List<Vector2> path)
    {
        if (path.Count <= 2) return path;

        var optimizedPath = new List<Vector2>(50);
        optimizedPath.Add(path[0]);

        int i = 0;
        while (i < path.Count - 2)
        {
            i = FindFurthestVisibleNode(path, i, optimizedPath);
        }

        if (i != path.Count - 1)
        {
            optimizedPath.Add(path[path.Count - 1]);
        }

        path.Clear();
        return optimizedPath;
    }

    private int FindFurthestVisibleNode(List<Vector2> path, int currentIndex, List<Vector2> optimizedPath)
    {
        Vector2 current = path[currentIndex];
        int furthestVisible = currentIndex + 1;

        for (int j = currentIndex + 2; j < path.Count; j++)
        {
            if (IsNodeVisible(current, path[j]))
            {
                furthestVisible = j;
            }
            else break;
        }

        optimizedPath.Add(path[furthestVisible]);
        return furthestVisible;
    }

    private bool IsNodeVisible(Vector2 from, Vector2 to)
    {
        bool hasObstacle = Physics2D.Linecast(from, to, LayerMask.GetMask("Obstacle"));
        return !hasObstacle && CheckPathClearance(from, to);
    }
    #endregion

    #region Node Operations
    private Node GetNodeFromWorldPosition(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x - bottomLeft.x) / (GRID_SIZE * NODE_SIZE);
        float percentY = (worldPosition.y - bottomLeft.y) / (GRID_SIZE * NODE_SIZE);

        int x = Mathf.Clamp(Mathf.FloorToInt(GRID_SIZE * percentX), 0, GRID_SIZE - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(GRID_SIZE * percentY), 0, GRID_SIZE - 1);

        return grid[x, y];
    }

    private List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>(8);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < GRID_SIZE && checkY >= 0 && checkY < GRID_SIZE)
                {
                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        bool canMoveDiagonally = grid[node.gridX + x, node.gridY].walkable &&
                                               grid[node.gridX, node.gridY + y].walkable;
                        if (!canMoveDiagonally) continue;
                    }

                    Node neighbour = grid[checkX, checkY];
                    if (neighbour.walkable)
                    {
                        if (!Physics2D.Linecast(node.worldPosition, neighbour.worldPosition,
                            LayerMask.GetMask("Obstacle")))
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }
        }

        return neighbours;
    }

    private float CalculateDistance(Node a, Node b)
    {
        float dx = Mathf.Abs(a.gridX - b.gridX);
        float dy = Mathf.Abs(a.gridY - b.gridY);

        return (dx + dy) + (1.4f - 2) * Mathf.Min(dx, dy);
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<Vector2> CalculatePath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.previousNode;
        }
        path.Reverse();
        return path;
    }

    private Node FindNearestWalkableNode(Node node)
    {
        if (node.walkable) return node;

        Queue<Node> openNodes = new Queue<Node>();
        HashSet<Node> visitedNodes = new HashSet<Node>();
        openNodes.Enqueue(node);
        visitedNodes.Add(node);

        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes.Dequeue();

            foreach (Node neighbor in GetNeighbours(currentNode))
            {
                if (!visitedNodes.Contains(neighbor))
                {
                    if (neighbor.walkable)
                    {
                        return neighbor;
                    }
                    openNodes.Enqueue(neighbor);
                    visitedNodes.Add(neighbor);
                }
            }
        }
        return null;
    }

    private List<Vector2> SmoothPath(List<Vector2> path)
    {
        if (path == null || path.Count <= 2) return path;

        List<Vector2> smoothedPath = new List<Vector2>();
        smoothedPath.Add(path[0]);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 prev = path[i - 1];
            Vector2 current = path[i];
            Vector2 next = path[i + 1];

            float t = 0.5f;
            Vector2 p0 = prev;
            Vector2 p1 = current;
            Vector2 p2 = next;

            Vector2 smoothedPoint = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;

            if (!Physics2D.Raycast(current, (smoothedPoint - current).normalized,
                Vector2.Distance(current, smoothedPoint), LayerMask.GetMask("Obstacle")))
            {
                smoothedPath.Add(smoothedPoint);
            }
            else
            {
                smoothedPath.Add(current);
            }
        }

        smoothedPath.Add(path[path.Count - 1]);
        return smoothedPath;
    }

    private List<Vector2> SimplifyPath(List<Vector2> path)
    {
        List<Vector2> simplifiedPath = new List<Vector2>();
        simplifiedPath.Add(path[0]);

        for (int i = 2; i < path.Count; i++)
        {
            Vector2 directionOld = (path[i - 1] - path[i - 2]).normalized;
            Vector2 directionNew = (path[i] - path[i - 1]).normalized;

            if (Vector2.Dot(directionOld, directionNew) < 0.98f)
            {
                simplifiedPath.Add(path[i - 1]);
            }
        }

        simplifiedPath.Add(path[path.Count - 1]);
        return simplifiedPath;
    }

    private bool CheckPathClearance(Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = Vector2.Perpendicular(direction) * (NODE_SIZE * 0.3f);

        bool leftClear = !Physics2D.Linecast(start + perpendicular, end + perpendicular,
            LayerMask.GetMask("Obstacle"));
        bool rightClear = !Physics2D.Linecast(start - perpendicular, end - perpendicular,
            LayerMask.GetMask("Obstacle"));

        return leftClear && rightClear;
    }
    #endregion

    #region Visualization
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireCube(gridWorldCenter, new Vector3(GRID_SIZE * NODE_SIZE, GRID_SIZE * NODE_SIZE, 1));

        Camera cam = Camera.main;
        if (cam == null) return;

        float viewDistance = 20f;
        Vector2 camPos = cam.transform.position;

        int startX = Mathf.Max(0, Mathf.FloorToInt((camPos.x - viewDistance - bottomLeft.x) / NODE_SIZE));
        int endX = Mathf.Min(GRID_SIZE - 1, Mathf.CeilToInt((camPos.x + viewDistance - bottomLeft.x) / NODE_SIZE));
        int startY = Mathf.Max(0, Mathf.FloorToInt((camPos.y - viewDistance - bottomLeft.y) / NODE_SIZE));
        int endY = Mathf.Min(GRID_SIZE - 1, Mathf.CeilToInt((camPos.y + viewDistance - bottomLeft.y) / NODE_SIZE));

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (grid[x, y] != null)
                {
                    Gizmos.color = grid[x, y].walkable ?
                        new Color(1, 1, 1, 0.1f) :
                        new Color(1, 0, 0, 0.2f);

                    Gizmos.DrawCube(grid[x, y].worldPosition, Vector3.one * NODE_SIZE * 0.8f);
                }
            }
        }

        if (FindObjectOfType<Enemy>()?.currentPath != null)
        {
            var path = FindObjectOfType<Enemy>().currentPath;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.color = new Color(0, 0, 1, 0.8f);
                Gizmos.DrawLine(path[i], path[i + 1]);

                Gizmos.DrawSphere(path[i], NODE_SIZE * 0.3f);
            }
            if (path.Count > 0)
            {
                Gizmos.color = new Color(0, 1, 0, 1f);
                Gizmos.DrawSphere(path[path.Count - 1], NODE_SIZE * 0.3f);
            }
        }
    }
    #endregion
}


