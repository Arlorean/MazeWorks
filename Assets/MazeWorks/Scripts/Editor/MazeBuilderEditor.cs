using UnityEditor;
using UnityEngine;

namespace MazeWorks {
    [CustomEditor(typeof(MazeBuilder))]
    [CanEditMultipleObjects]
    public class MazeBuilderEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var builder = (MazeBuilder)target;
            if (GUILayout.Button(MazeBuilder.BuildMazeText)) {
                Undo.RecordObject(builder.GetComponent<MeshFilter>(), MazeBuilder.BuildMazeText);

                builder.BuildMaze();
            }
        }
    }
}