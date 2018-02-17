using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MazeGenerator generator = target as MazeGenerator;
        if (generator.generateFromEditor)
            generator.GenerateMazeFromEditor();
    }

}
