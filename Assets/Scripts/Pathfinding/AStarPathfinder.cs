using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    private static readonly Vector3Int[] Dirs = {
        Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left
    };

    /// <summary>
    /// Returns a list of cell positions from start to goal (inclusive), or null if no path exists.
    /// </summary>
    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, int floor)
    {
        PathfindingGrid grid = PathfindingGrid.Instance;
        if (grid == null) return null;
        if (!grid.IsWalkable(start, floor) || !grid.IsWalkable(goal, floor)) return null;
        if (start == goal) return new List<Vector3Int> { start };

        var open = new MinHeap();
        var gScore = new Dictionary<Vector3Int, float>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var closed = new HashSet<Vector3Int>();

        gScore[start] = 0f;
        open.Push(new Node(start, 0f, Heuristic(start, goal)));

        while (open.Count > 0)
        {
            Node current = open.Pop();

            if (current.Pos == goal)
                return BuildPath(cameFrom, goal);

            if (!closed.Add(current.Pos)) continue;

            foreach (Vector3Int dir in Dirs)
            {
                Vector3Int nb = current.Pos + dir;
                if (closed.Contains(nb) || !grid.CanMove(current.Pos, nb, floor)) continue;

                float g = gScore[current.Pos] + grid.GetWalkCost(nb, floor);
                if (!gScore.TryGetValue(nb, out float best) || g < best)
                {
                    gScore[nb] = g;
                    cameFrom[nb] = current.Pos;
                    open.Push(new Node(nb, g, Heuristic(nb, goal)));
                }
            }
        }

        return null;
    }

    private static float Heuristic(Vector3Int a, Vector3Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static List<Vector3Int> BuildPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int end)
    {
        var path = new List<Vector3Int> { end };
        while (cameFrom.TryGetValue(end, out Vector3Int prev))
        {
            path.Add(prev);
            end = prev;
        }
        path.Reverse();
        return path;
    }

    private struct Node
    {
        public Vector3Int Pos;
        public float G, H;
        public float F => G + H;
        public Node(Vector3Int pos, float g, float h) { Pos = pos; G = g; H = h; }
    }

    private class MinHeap
    {
        private readonly List<Node> _data = new();
        public int Count => _data.Count;

        public void Push(Node n)
        {
            _data.Add(n);
            BubbleUp(_data.Count - 1);
        }

        public Node Pop()
        {
            Node top = _data[0];
            int last = _data.Count - 1;
            _data[0] = _data[last];
            _data.RemoveAt(last);
            if (_data.Count > 0) SiftDown(0);
            return top;
        }

        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (_data[i].F >= _data[p].F) break;
                (_data[i], _data[p]) = (_data[p], _data[i]);
                i = p;
            }
        }

        private void SiftDown(int i)
        {
            int n = _data.Count;
            while (true)
            {
                int s = i, l = 2 * i + 1, r = 2 * i + 2;
                if (l < n && _data[l].F < _data[s].F) s = l;
                if (r < n && _data[r].F < _data[s].F) s = r;
                if (s == i) break;
                (_data[i], _data[s]) = (_data[s], _data[i]);
                i = s;
            }
        }
    }
}
