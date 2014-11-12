using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// https://stackoverflow.com/a/5852926/1097920
[System.Serializable]
public class FixedSizeQueue<T> : Queue<T>, ISerializationCallbackReceiver {
    public int Size;
    public FixedSizeQueue(int size) {
        Size = size;
    }

    [SerializeField]
    private List<T> values = new List<T>();

    public void OnBeforeSerialize() {
        values.Clear();
        foreach(T value in this) {
            values.Add(value);
        }
    }

    public void OnAfterDeserialize() {
        this.Clear();
        for(int i = 0; i < values.Count; i++)
            this.Enqueue(values[i]);
    }

     public void Enqueue(T obj) {
        base.Enqueue(obj);
        T overflow;
        while (Count > Size)
            Dequeue();
     }
 }