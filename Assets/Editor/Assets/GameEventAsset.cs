using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class GameEventAsset {
    [MenuItem("Assets/Create/GameEvent")]
    public static void CreateAsset() {
        CustomAssetUtility.CreateAsset<GameEvent>();
    }
}
