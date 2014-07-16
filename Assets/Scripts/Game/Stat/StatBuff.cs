﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BuffType {ADD, MULTIPLY};

[System.Serializable]
public class StatBuff {
    public string name;
    public float value;
    public int duration;
    public BuffType type;

    public StatBuff(string name_, float value_, int duration_ = 0, BuffType type_ = BuffType.ADD) {
        name = name_;
        value = value_;
        duration = duration_;
        type = type_;
    }
}