using MazeWorks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellBlocker : MonoBehaviour
{
    public Vector3 center;
    public Vector3 size = Vector3.one * 3;
    public List<MazeCell> blocking = new List<MazeCell>();

    void OnEnable() {
        var localBounds = new Bounds(center, size);

        var cells = FindObjectsOfType<MazeCell>();
        foreach (var cell in cells) {
            var localPosition = transform.InverseTransformPoint(cell.transform.position);
            if (localBounds.Contains(localPosition)) {
                cell.blockers.Add(this);
                blocking.Add(cell);
            }
        }
    }

    void OnDisable() {
        foreach (var cell in blocking) {
            cell.blockers.Remove(this);
        }
        blocking.Clear();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(center, size);
    }
}
