using System.Collections.Generic;
using UnityEngine;

namespace MazeWorks {
    public class MazeBuilder : MonoBehaviour {

        public int cellSize = 3; // Size of a cell in Unity units (int only for now)

        bool mazeNeedsBuilding = true;

        MeshFilter meshFilter;

        void OnDrawGizmos() {
            if (mazeNeedsBuilding) {
                BuildMaze();
            }
        }

        MeshFilter MeshFilter {
            get {
                if (!meshFilter) {
                    meshFilter = GetComponent<MeshFilter>();
                }
                return meshFilter;
            }
        }

        public MazeCell[] Cells => GetComponentsInChildren<MazeCell>();

        public const string BuildMazeText = "Build Maze";

        [ContextMenu(BuildMazeText)]
        public void BuildMaze() {
            mazeNeedsBuilding = false;
            UpdateCellNeighbours();
            MeshFilter.mesh = CreateMesh();
        }

        public void CellUpdated(MazeCell cell) {
            // TODO: Only updates things related to this cell
            mazeNeedsBuilding = true;
        }

        void UpdateCellNeighbours() {
            var cells = new Dictionary<Vector3Int, MazeCell>();
            foreach (var cell in Cells) {
                cells[cell.Location] = cell;
            }

            foreach (var cell in Cells) {
                cell.forward = null;
                cell.back = null;
                cell.left = null;
                cell.right = null;
                cell.up = null;
                cell.down = null;

                var location = cell.Location;
                if (cells.TryGetValue(location + Vector3Int.forward, out var forwardCell)) {
                    cell.forward = forwardCell;
                }
                if (cells.TryGetValue(location + Vector3Int.back, out var backCell)) {
                    cell.back = backCell;
                }
                if (cells.TryGetValue(location + Vector3Int.left, out var leftCell)) {
                    cell.left = leftCell;
                }
                if (cells.TryGetValue(location + Vector3Int.right, out var rightCell)) {
                    cell.right = rightCell;
                }
                if (cells.TryGetValue(location + Vector3Int.up, out var upCell)) {
                    cell.up = upCell;
                }
                if (cells.TryGetValue(location + Vector3Int.down, out var downCell)) {
                    cell.down = downCell;
                }
            }
        }

        Mesh CreateMesh() {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var ceilingIndices = new List<int>();
            var wallIndices = new List<int>();
            var floorIndices = new List<int>();

            var cells = Cells;
            foreach (var cell in cells) {
                CreateCellMeshData(cell, cellSize, vertices, normals, uvs, ceilingIndices, wallIndices, floorIndices);
            }

            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.subMeshCount = 3;
            mesh.SetTriangles(ceilingIndices.ToArray(), 0);
            mesh.SetTriangles(wallIndices.ToArray(), 1);
            mesh.SetTriangles(floorIndices.ToArray(), 2);
            mesh.RecalculateTangents();
            return mesh;
        }


        //           4           5          
        //           +-----------+          
        //          /|          /|          +Y  
        //        7/ |        6/ |           ^  +Z
        //        +-----------+  |           |  /
        //        | 0|    o   |  |1          | / 
        //        |  +--------|--+           |/
        //        | /         | /            o-----> +X
        //        |/          |/            
        //        +-----------+             
        //        3           2

        static Vector3[] cellVertices = {
            /*0*/ (Vector3.forward+Vector3.down+Vector3.left)*0.5f,
            /*1*/ (Vector3.forward+Vector3.down+Vector3.right)*0.5f,
            /*2*/ (Vector3.back+Vector3.down+Vector3.right)*0.5f,
            /*3*/ (Vector3.back+Vector3.down+Vector3.left)*0.5f,
            /*4*/ (Vector3.forward+Vector3.up+Vector3.left)*0.5f,
            /*5*/ (Vector3.forward+Vector3.up+Vector3.right)*0.5f,
            /*6*/ (Vector3.back+Vector3.up+Vector3.right)*0.5f,
            /*7*/ (Vector3.back+Vector3.up+Vector3.left)*0.5f,
        };

        static int[] forwardFace = { 0, 4, 5, 1 };
        static int[] backFace = { 2, 6, 7, 3 };
        static int[] leftFace = { 3, 7, 4, 0 };
        static int[] rightFace = { 1, 5, 6, 2 };
        static int[] upFace = { 4, 7, 6, 5 };
        static int[] downFace = { 3, 0, 1, 2 };

        static int[] quadToTriangles = { 0, 1, 2, 2, 3, 0 };

        static Vector2[] faceUVs = {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };

        void CreateCellMeshData(MazeCell cell, int size, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> ceilingIndices, List<int> wallIndices, List<int> floorIndices) {
            var center = cell.transform.localPosition;

            if (!cell.forward) {
                CreateCellFace(forwardFace, -Vector3.forward, center, size, vertices, normals, uvs, wallIndices);
            }
            if (!cell.back) {
                CreateCellFace(backFace, -Vector3.back, center, size, vertices, normals, uvs, wallIndices);
            }
            if (!cell.left) {
                CreateCellFace(leftFace, -Vector3.left, center, size, vertices, normals, uvs, wallIndices);
            }
            if (!cell.right) {
                CreateCellFace(rightFace, -Vector3.right, center, size, vertices, normals, uvs, wallIndices);
            }
            if (!cell.up) {
                CreateCellFace(upFace, -Vector3.up, center, size, vertices, normals, uvs, ceilingIndices);
            }
            if (!cell.down) {
                CreateCellFace(downFace, -Vector3.down, center, size, vertices, normals, uvs, floorIndices);
            }
        }

        void CreateCellFace(int[] quadIndices, Vector3 normal, Vector3 center, int size, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices) {
            foreach (var qi in quadToTriangles) {
                var v = cellVertices[quadIndices[qi]];
                var uv = faceUVs[qi];
                indices.Add(vertices.Count);
                vertices.Add(center + v * size);
                normals.Add(normal);
                uvs.Add(uv);
            }
        }
    }
}