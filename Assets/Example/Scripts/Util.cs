using MazeWorks;
using UnityEngine;

public static class Util {

    public static MazeCell GetNextCell(this MazeCell cell, Direction direction) {
        switch (direction) {
            case Direction.Forward: return cell.forward;
            case Direction.Back: return cell.back;
            case Direction.Left: return cell.left;
            case Direction.Right: return cell.right;
            case Direction.Up: return cell.up;
            case Direction.Down: return cell.down;
        }
        Debug.LogWarning($"GetNextCell(MazeCell) called with unknown direction '{direction}'");
        return null;
    }

    public static MazeCell FindClosestCell(Vector3 position) {
        var closestDistance = float.PositiveInfinity;
        var closestCell = default(MazeCell);
        foreach (var cell in GameObject.FindObjectsOfType<MazeCell>()) {
            var distance = Vector3.Distance(position, cell.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestCell = cell;
            }
        }
        return closestCell;
    }

    public static Vector3 SnapToNearest90Degrees(Vector3 rotation) {
        rotation.x = SnapToNearest90Degrees(rotation.x);
        rotation.y = SnapToNearest90Degrees(rotation.y);
        rotation.z = SnapToNearest90Degrees(rotation.z);
        return rotation;
    }

    public static float SnapToNearest90Degrees(float angle) {
        var dy = Mathf.DeltaAngle(0, angle);
        angle = 0;
        if (dy > 45) {
            angle = 90;
        }
        if (dy > 135) {
            angle = 180;
        }
        if (dy < -45) {
            angle = -90;
        }
        if (dy < -135) {
            angle = -180;
        }
        return angle;
    }
}
