using UnityEngine;
using UnityEditor;
using System.Collections;

public class ScriptEditorDebughelpers : EditorWindow
{

    static EditorWindow window = null;
    static ScriptEditor editor = null;

    [MenuItem("Window/ScriptEditorHelper")]
    static void init()
    {
        if(window)
            window.Close();
        window = EditorWindow.GetWindow<ScriptEditorDebughelpers>();
    }

    // Use this for initialization
    void OnGUI()
    {
        if (GUILayout.Button("Open/Reload", EditorStyles.miniButton))
        {
            openScriptEditor();
            init();
        }
    }

    public static void openScriptEditor()
    {
        if (editor)
            editor.Close();
        editor = EditorWindow.GetWindow<ScriptEditor>();
          }
}
