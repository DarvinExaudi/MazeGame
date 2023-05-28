using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEasy : MonoBehaviour
{
    public float holep;
    public int w, h, x, y;
    public bool[,] hwalls, vwalls;
    public Transform Level, Player, Goal, Powerup;
    public GameObject Floor, Wall;
    public CinemachineVirtualCamera cam;
    public GameObject HighlightPrefab;
    List<Cell> grid;
    bool gotPowerup = false;

    struct Cell
    {
        public GameObject cellGO;
        public SpriteRenderer cellRenderer;

        public Cell(GameObject cellGO)
        {
            this.cellGO = cellGO;
            cellRenderer = null;
        }
    }

    Cell GetCellByPos(Vector2Int position)
    {
        Cell foundCell = grid.FirstOrDefault((cell) => WorldPositionToGridPosition(cell.cellGO.transform.position).Equals(position));
        return foundCell;
    }

    public Vector2Int WorldPositionToGridPosition(Vector3 worldPosition, float cellSize = 1f)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    void Start()
    {
        gotPowerup = false;
        foreach (Transform child in Level)
            Destroy(child.gameObject);

        hwalls = new bool[w + 1, h];
        vwalls = new bool[w, h + 1];
        var st = new int[w, h];
        grid = new List<Cell>();

        void dfs(int x, int y)
        {
            st[x, y] = 1;
            GameObject cell = Instantiate(Floor, new Vector3(x, y), Quaternion.identity, Level);
            grid.Add(new Cell(cell));

            var dirs = new[]
            {
                (x - 1, y, hwalls, x, y, Vector3.right, 90, KeyCode.A),
                (x + 1, y, hwalls, x + 1, y, Vector3.right, 90, KeyCode.D),
                (x, y - 1, vwalls, x, y, Vector3.up, 0, KeyCode.S),
                (x, y + 1, vwalls, x, y + 1, Vector3.up, 0, KeyCode.W),
            };
            foreach (var (nx, ny, wall, wx, wy, sh, ang, k) in dirs.OrderBy(d => Random.value))
                if (!(0 <= nx && nx < w && 0 <= ny && ny < h) || (st[nx, ny] == 2 && Random.value > holep))
                {
                    wall[wx, wy] = true;
                    Instantiate(Wall, new Vector3(wx, wy) - sh / 2, Quaternion.Euler(0, 0, ang), Level);
                }
                else if (st[nx, ny] == 0) dfs(nx, ny);
            st[x, y] = 2;
        }
        dfs(0, 0);

        x = Random.Range(0, w);
        y = Random.Range(0, h);
        Player.position = new Vector3(x, y);
        do Goal.position = new Vector3(Random.Range(0, w), Random.Range(0, h));
        while (Vector3.Distance(Player.position, Goal.position) < (w + h) / 4);

        Powerup.position = new Vector3(Random.Range(0, w), Random.Range(0, h));

        cam.m_Lens.OrthographicSize = Mathf.Pow(w / 3 + h / 2, 0.7f) + 1;
    }

    private Stack<Vector2Int> dfsStack;

    void Update()
    {
        var dirs = new[]
        {
        (x - 1, y, hwalls, x, y, Vector3.right, 90, KeyCode.A),
        (x + 1, y, hwalls, x + 1, y, Vector3.right, 90, KeyCode.D),
        (x, y - 1, vwalls, x, y, Vector3.up, 0, KeyCode.S),
        (x, y + 1, vwalls, x, y + 1, Vector3.up, 0, KeyCode.W),
    };
        foreach (var (nx, ny, wall, wx, wy, sh, ang, k) in dirs.OrderBy(d => Random.value))
        {
            if (Input.GetKeyDown(k))
            {
                if (wall[wx, wy])
                    Player.position = Vector3.Lerp(Player.position, new Vector3(nx, ny), 0.1f);
                else
                    (x, y) = (nx, ny);
            }
        }

        Player.position = Vector3.Lerp(Player.position, new Vector3(x, y), Time.deltaTime * 12);
        if (Vector3.Distance(Player.position, Goal.position) < 0.12f)
        {
            if (Random.Range(0, 5) < 3)
                w++;
            else
                h++;
            Start();
        }
        if (Vector3.Distance(Player.position, Powerup.position) < 0.12f && !gotPowerup)
        {
            //var powerupPosition = new Vector2Int(Mathf.RoundToInt(Powerup.position.x), Mathf.RoundToInt(Powerup.position.y));
            Vector2Int powerupPosition = WorldPositionToGridPosition(Powerup.position);
            Vector2Int goalPos = WorldPositionToGridPosition(Goal.position);
            DFS(powerupPosition, goalPos);

            // Highlight the path if found
            if (dfsStack != null)
            {
                Debug.Log($"Path found . Path lenght is {dfsStack.Count}");
                Debug.Log($"Goal : {goalPos}");
                gotPowerup = true;
                foreach (var position in dfsStack)
                {
                    Debug.Log(position);
                    HighlightCell(position);
                }

                Debug.Log("-----------------------");
            }
            else
                Debug.LogWarning("Path not found !");
        }
    }


    private void DFS(Vector2Int start, Vector2Int goal)
    {
        var stack = new Stack<Vector2Int>();
        stack.Push(start);

        var visited = new HashSet<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();

        bool foundPath = false; // Flag to track if a path has been found

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current == goal)
            {
                foundPath = true;
                //Debug.Log("YAY KETEMU");
                break; // Exit the search if the goal is reached
            }

            visited.Add(current);

            // Add the neighboring cells to the stack
            var neighbors = GetNeighbors(current);
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    stack.Push(neighbor);
                    parent[neighbor] = current;
                }
            }
        }

        if (foundPath && !parent.ContainsKey(goal))
            parent.Add(goal, goal);

        // Highlight the path from the power-up to the goal if found
        if (foundPath && parent.ContainsKey(goal))
        {
            dfsStack = new Stack<Vector2Int>(); // Create a new stack to store the path
            var node = goal;
            while (node != start)
            {
                dfsStack.Push(node); // Push the cells on the path to the stack
                node = parent[node];
            }
            dfsStack.Push(start); // Push the start position to the stack
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>();

        // Check the neighboring cells in all directions
        var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var direction in directions)
        {
            var neighbor = position + direction;

            // Check if the neighbor is within the maze boundaries
            if (neighbor.x >= 0 && neighbor.x < w && neighbor.y >= 0 && neighbor.y < h)
            {
                // Check if there is a wall between the current cell and the neighbor
                if ((direction == Vector2Int.up && !vwalls[position.x, position.y + 1]) ||
                    (direction == Vector2Int.right && !hwalls[position.x + 1, position.y]) ||
                    (direction == Vector2Int.down && !vwalls[position.x, position.y]) ||
                    (direction == Vector2Int.left && !hwalls[position.x, position.y]))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    private void HighlightCell(Vector2Int position)
    {

        Cell cell = GetCellByPos(position);
        if (cell.cellGO == null)
            return;

        if (cell.cellRenderer == null)
            cell.cellRenderer = cell.cellGO.GetComponent<SpriteRenderer>();

        cell.cellRenderer.color = Color.yellow;

        //Debug.Log($"Highlighted {changed} cells !");
    }



}