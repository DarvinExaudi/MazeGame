using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEasy : MonoBehaviour
{
    public float holep; // Probabilitas untuk menghasilkan lubang di antara tembok
    public int w, h, x, y; // Variabel untuk lebar (w), tinggi (h), dan posisi pemain (x, y)
    public bool[,] hwalls, vwalls; // Array boolean untuk tembok horizontal dan vertikal
    public Transform Level, Player, Goal, Powerup; // Transform objek untuk level, pemain, tujuan, dan power-up
    public GameObject Floor, Wall; // Prefab untuk lantai dan tembok
    public CinemachineVirtualCamera cam; // Kamera virtual Cinemachine
    public GameObject HighlightPrefab; // Prefab untuk penyorotan sel
    List<Cell> grid; // List untuk menyimpan sel-sel dalam level
    bool gotPowerup = false; // Flag untuk menandakan apakah pemain telah mengambil power-up

    // Struktur data untuk sel dalam level
    struct Cell
    {
        public GameObject cellGO; // Objek sel di dalam level
        public SpriteRenderer cellRenderer; // Komponen SpriteRenderer untuk sel

        public Cell(GameObject cellGO)
        {
            this.cellGO = cellGO;
            cellRenderer = null;
        }
    }

    // Mendapatkan sel berdasarkan posisinya dalam grid
    Cell GetCellByPos(Vector2Int position)
    {
        Cell foundCell = grid.FirstOrDefault((cell) => WorldPositionToGridPosition(cell.cellGO.transform.position).Equals(position));
        return foundCell;
    }

    // Mengonversi posisi dunia menjadi posisi dalam grid
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

        hwalls = new bool[w + 1, h]; // Inisialisasi array tembok horizontal
        vwalls = new bool[w, h + 1]; // Inisialisasi array tembok vertikal
        var st = new int[w, h]; // Array untuk melacak status sel saat melakukan depth-first search
        grid = new List<Cell>(); // Inisialisasi list sel dalam level

        // Fungsi depth-first search untuk membangun level
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

        // Menempatkan tujuan dengan jarak minimum dari pemain
        do Goal.position = new Vector3(Random.Range(0, w), Random.Range(0, h));
        while (Vector3.Distance(Player.position, Goal.position) < (w + h) / 4);

        Powerup.position = new Vector3(Random.Range(0, w), Random.Range(0, h));

        // Menyesuaikan ukuran kamera berdasarkan ukuran level
        cam.m_Lens.OrthographicSize = Mathf.Pow(w / 3 + h / 2, 0.7f) + 1;
    }

    private Stack<Vector2Int> dfsStack; // Stack untuk menyimpan jalur dari power-up ke tujuan

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
            // Menambahkan panjang atau lebar level secara acak jika pemain mencapai tujuan
            if (Random.Range(0, 5) < 3)
                w++;
            else
                h++;
            Start();
        }
        if (Vector3.Distance(Player.position, Powerup.position) < 0.12f && !gotPowerup)
        {
            Vector2Int powerupPosition = WorldPositionToGridPosition(Powerup.position);
            Vector2Int goalPos = WorldPositionToGridPosition(Goal.position);
            DFS(powerupPosition, goalPos);

            // Menyoroti jalur jika ditemukan
            if (dfsStack != null)
            {
                Debug.Log($"Jalur ditemukan. Panjang jalur adalah {dfsStack.Count}");
                Debug.Log($"Tujuan: {goalPos}");
                gotPowerup = true;
                foreach (var position in dfsStack)
                {
                    Debug.Log(position);
                    HighlightCell(position);
                }

                Debug.Log("-----------------------");
            }
            else
                Debug.LogWarning("Jalur tidak ditemukan!");
        }
    }

    // Depth-First Search untuk mencari jalur dari power-up ke tujuan
    private void DFS(Vector2Int start, Vector2Int goal)
    {
        var stack = new Stack<Vector2Int>();
        stack.Push(start);

        var visited = new HashSet<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();

        bool foundPath = false; // Flag untuk melacak apakah jalur telah ditemukan

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current == goal)
            {
                foundPath = true;
                break; // Keluar dari pencarian jika tujuan tercapai
            }

            visited.Add(current);

            // Menambahkan sel tetangga ke dalam stack
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

        // Menyoroti jalur dari power-up ke tujuan jika ditemukan
        if (foundPath && parent.ContainsKey(goal))
        {
            dfsStack = new Stack<Vector2Int>(); // Membuat stack baru untuk menyimpan jalur
            var node = goal;
            while (node != start)
            {
                dfsStack.Push(node); // Memasukkan sel-sel dalam jalur ke dalam stack
                node = parent[node];
            }
            dfsStack.Push(start); // Memasukkan posisi awal ke dalam stack
        }
    }

    // Mengembalikan daftar tetangga dari suatu posisi dalam koordinat grid
    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>();

        // Memeriksa sel tetangga di semua arah
        var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var direction in directions)
        {
            var neighbor = position + direction;

            // Memeriksa apakah tetangga berada dalam batas-batas labirin
            if (neighbor.x >= 0 && neighbor.x < w && neighbor.y >= 0 && neighbor.y < h)
            {
                // Memeriksa apakah ada dinding antara sel saat ini dan tetangga
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

    // Menyoroti sel pada posisi tertentu
    private void HighlightCell(Vector2Int position)
    {
        Cell cell = GetCellByPos(position);
        if (cell.cellGO == null)
            return;

        if (cell.cellRenderer == null)
            cell.cellRenderer = cell.cellGO.GetComponent<SpriteRenderer>();

        cell.cellRenderer.color = Color.yellow;

        //Debug.Log($"Meyoroti sel pada posisi {position}");
    }
}
