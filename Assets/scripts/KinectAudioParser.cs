using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Kinect;
using Kinect = Windows.Kinect;
using UnityEngine;
using UnityEngine.Windows.Speech;

// using Microsoft.Speech.AudioFormat;
// using Microsoft.Speech.Recognition;

public class KinectAudioParser : MonoBehaviour
{

    [SerializeField]
    private string[] _keywords;

    private KeywordRecognizer _keywordRecognizer;

    // Use this for initialization
    void Start()
    {
        _keywordRecognizer = new KeywordRecognizer(_keywords);
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();

    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0} ({1})\n", args.text, args.confidence);
        Debug.Log(sb.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
