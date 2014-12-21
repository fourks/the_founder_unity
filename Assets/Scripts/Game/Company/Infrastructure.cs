using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Infrastructure : SerializableDictionary<Infrastructure.Type, int> {
    public enum Type {
        Datacenter,
        Factory,
        Studio,
        Lab
    }

    public static Type[] Types {
        get { return Enum.GetValues(typeof(Type)) as Type[]; }
    }

    public static Infrastructure ForType(Type t) {
        Infrastructure inf = new Infrastructure();
        inf[t] = 1;
        return inf;
    }

    public Infrastructure() {
        // Initialize with 0 of each infrastructure type.
        foreach (Type t in Enum.GetValues(typeof(Type))) {
            Add(t, 0);
        }
    }

    // Returns the cost for this set of infrastructure.
    public int cost {
        get {
            // TO DO
            // All infrastructure costs the same. Should it?
            int baseCost = 10000;
            int cost = 0;
            foreach(KeyValuePair<Type, int> item in this) {
                cost += item.Value * baseCost;
            }
            return cost;
        }
    }

    public override string ToString() {
        string repr = "";
        foreach(KeyValuePair<Type, int> item in this) {
            if (item.Value > 0)
                repr += item.Key.ToString() + ":" + item.Value.ToString() + " ";
        }
        return repr;
    }

    public bool Equals(Infrastructure right) {
        foreach(KeyValuePair<Type, int> item in this) {
            if (item.Value != right[item.Key])
                return false;
        }
        return true;
    }

    public static bool operator <=(Infrastructure left, Infrastructure right) {
        foreach(KeyValuePair<Type, int> item in left) {
            if (item.Value > right[item.Key])
                return false;
        }
        return true;
    }

    public static bool operator >=(Infrastructure left, Infrastructure right) {
        foreach(KeyValuePair<Type, int> item in left) {
            if (item.Value < right[item.Key])
                return false;
        }
        return true;
    }

    public static Infrastructure operator +(Infrastructure left, Infrastructure right) {
        Infrastructure result = new Infrastructure();
        foreach(KeyValuePair<Type, int> item in left) {
            result[item.Key] = item.Value + right[item.Key];
        }
        return result;
    }

    public static Infrastructure operator -(Infrastructure left, Infrastructure right) {
        Infrastructure result = new Infrastructure();
        foreach(KeyValuePair<Type, int> item in left) {
            result[item.Key] = item.Value - right[item.Key];

            // The smallest is 0, no negatives.
            if (result[item.Key] < 0)
                result[item.Key] = 0;
        }
        return result;
    }
}
