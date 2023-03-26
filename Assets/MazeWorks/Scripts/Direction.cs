using UnityEngine;

namespace MazeWorks {

    public enum Direction {
        Forward = 0, // North
        Right = 1, // East
        Back = 2, // South
        Left = 3, // West
        Up = 4,
        Down = 5,
    }

    public enum RelativeDirection {
        Forward = 0,
        Right = +1,
        Back = +2,
        Left = -1,
    }

    public static class DirectionExtensions {
        public static Direction Reverse(this Direction direction) {
            switch (direction) {
                case Direction.Forward: return Direction.Back;
                case Direction.Back: return Direction.Forward;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
            }
            throw new System.ArgumentOutOfRangeException($"Cannot reverse Direction {direction}.");
        }

        public static Vector3Int ToVector3Int(this Direction direction) {
            switch (direction) {
                case Direction.Forward: return Vector3Int.forward;
                case Direction.Back: return Vector3Int.back;
                case Direction.Left: return Vector3Int.left;
                case Direction.Right: return Vector3Int.right;
                case Direction.Up: return Vector3Int.up;
                case Direction.Down: return Vector3Int.down;
            }
            throw new System.ArgumentOutOfRangeException($"Cannot convert Direction {direction} to Vector3Int.");
        }

        public static Direction ToDirection(this Vector3Int direction) {
            if (direction == Vector3Int.forward) { return Direction.Forward; }
            if (direction == Vector3Int.back) { return Direction.Back; }
            if (direction == Vector3Int.left) { return Direction.Left; }
            if (direction == Vector3Int.right) { return Direction.Right; }
            if (direction == Vector3Int.up) { return Direction.Up; }
            if (direction == Vector3Int.down) { return Direction.Down; }
            throw new System.ArgumentOutOfRangeException($"Cannot convert Vector3Int {direction} to Direction.");
        }

        public static Direction Add(this Direction direction, RelativeDirection relativeDirection) {
            return (Direction)(((int)direction + (int)relativeDirection + 4) % 4);
        }

        public static Direction GetHorizontalDirection(float yaw) {
            var direction = Direction.Back;

            // Normalize yaw from -180 to +180
            yaw = ((yaw + 180) % 360) - 180;

            if (yaw >= -45 && yaw <= +45) { direction = Direction.Forward; }
            if (yaw < -45 && yaw > -135) { direction = Direction.Left; }
            if (yaw > +45 && yaw < +135) { direction = Direction.Right; }

            return direction;
        }

        public static Direction GetVerticalDirection(float pitch) {
            var direction = Direction.Forward;

            // Normalize pitch from -180 to +180
            pitch = ((pitch + 180) % 360) - 180;

            if (pitch < -45) { direction = Direction.Up; }
            if (pitch > +45) { direction = Direction.Down; }

            return direction;
        }

        public static float ToYaw(this Direction direction) {
            switch (direction) {
                case Direction.Forward: return 0;
                case Direction.Right: return 90;
                case Direction.Back: return 180;
                case Direction.Left: return -90;
            }
            throw new System.ArgumentOutOfRangeException($"Unknown direction {direction}.");
        }
    }
}