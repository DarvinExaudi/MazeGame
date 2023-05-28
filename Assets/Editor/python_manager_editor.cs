using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;

[CustomEditor(typeof(python_manager))]
public class python_manager_editor : Editor
{
    python_manager targetManager;

    void Start()
    {
        targetManager = (python_manager)target;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Launch Python Script", GUILayout.Height(35)))
        {
            string path = Application.dataPath + "/Python/shortestpath.py";
            PythonRunner.RunFile(path);
        }
    }
}
