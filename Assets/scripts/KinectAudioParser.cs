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
using UnityEngine.Events;


// using Microsoft.Speech.AudioFormat;
// using Microsoft.Speech.Recognition;

public class KinectAudioParser : MonoBehaviour
{
    [Serializable]
    public struct KeywordAndAction
    {
        public string keyword;
        public UnityEvent action;
    }

    [SerializeField]
    private KeywordAndAction[] _keywords;

    private KeywordRecognizer _keywordRecognizer;

    // Use this for initialization
    void Start()
    {
        string[] keywordList = new string[_keywords.Length];
        for (int i = 0; i < _keywords.Length; i++)
        {
            keywordList[i] = _keywords[i].keyword;
        }
        _keywordRecognizer = new KeywordRecognizer(keywordList);
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();

    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        foreach (var pair in _keywords)
        {
            if (pair.keyword == args.text)
            {
                pair.action.Invoke();
            }
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0} ({1})\n", args.text, args.confidence);
        Debug.Log(sb.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
