using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class add_batch_file
{
    [MenuItem("Assets/Create/bluejayvrstudio/Add Protoc Batch Script")]
    private static void AddProtocBatch() {
        string sourcePath = Path.Combine("Packages", "com.bluejayvrstudio.utils", "Editor", "ProtobufScripts", "protobuild.bat");
        string targetPath = Path.Combine(AssetDatabase.GetAssetPath(Selection.activeObject), "protobuild.bat");
        
        File.Copy(sourcePath, targetPath, true);
        AssetDatabase.Refresh();
        Debug.Log("Batch file copied to: " + targetPath);
        
    }
}
