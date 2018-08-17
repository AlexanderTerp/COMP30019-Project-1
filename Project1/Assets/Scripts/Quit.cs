﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Quit : MonoBehaviour {

    // A reference to the actual button that will be pressed to trigger
    // script.
    private Button button;

    // Use this for initialization
    void Start ()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Application.Quit);
    }
}
