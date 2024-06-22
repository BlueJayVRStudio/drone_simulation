using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class add_proto_file
{
    [MenuItem("Assets/Create/bluejayvrstudio/New Proto File")]
    private static void AddProtoFile() {
        string sourcePath = Path.Combine("Packages", "com.bluejayvrstudio.utils", "Editor", "ProtobufScripts", "sample.proto");
        string targetPath = Path.Combine(AssetDatabase.GetAssetPath(Selection.activeObject), "sample.proto");
        
        File.Copy(sourcePath, targetPath, true);
        AssetDatabase.Refresh();
        Debug.Log("Batch file copied to: " + targetPath);
        
    }
}
