using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class WorkerAsset {
    [MenuItem("Assets/Create/Worker")]
    public static void CreateAsset() {
        CustomAssetUtility.CreateAsset<Worker>();
    }
}
