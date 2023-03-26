using System.Collections.Generic;
using UnityEngine;

namespace MazeWorks {
    public class MazeCell : MonoBehaviour {
        public MazeCell left; // -X
        public MazeCell right; // +X
        public MazeCell forward; // +Z 
        public MazeCell back;  // -Z
        public MazeCell up; // +Y
        public MazeCell down; // -Y  

        public List<MazeCellBlocker> blockers = new List<MazeCellBlocker>();

        public bool IsBlocked => blockers.Count > 0;

        int size = 3;

        MazeBuilder Manager => GetComponentInParent<MazeBuilder>();

        public int Size {
            get {
                var manager = Manager;
                if (manager) {
                    size = manager.cellSize;
                }
                return size;
            }
        }

        public Vector3Int Location {
            get {
                var p = transform.localPosition / Size;
                return new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
            }
        }

        void OnEnable() {
            UpdateManager();
        }

        void OnDisable() {
            UpdateManager();
        }

        Vector3 lastLocalPosition;

        void OnDrawGizmos() {
            if (transform.localPosition != lastLocalPosition) {
                lastLocalPosition = transform.localPosition;
                UpdateManager();
            }       
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one*Size);
        }

        void UpdateManager() {
            Manager?.CellUpdated(this);
        }
    }
}