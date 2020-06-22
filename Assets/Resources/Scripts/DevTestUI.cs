﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DevTestUI : MonoBehaviour
{
    public static DevTestUI devTestUI;

    public InputField smoothCoeffField;
    public InputField lerpCoeffField;
    private void Awake()
    {
        devTestUI = this;
    }

    public void SetSmoothingCoeff()
    {
        float res;
        res = float.Parse(smoothCoeffField.text);
        PlayerTransformView.smoothCoeff = res;
        print("smooth coeff set to " + res);
    }

    public void SetLerpCoeff()
    {
        float res;
        res = float.Parse(lerpCoeffField.text);
        PlayerTransformView.lerpCoeff = res;
        print("lerp coeff set to " + res);
    }
}