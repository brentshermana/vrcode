using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System;

public class DictationGetter : MonoBehaviour
{

    public Line3D line;

    protected DictationRecognizer recognizer;

    private char[] split_tokens = new char[] { ' ', '\n', '\t'};

    private void Start()
    {
        // Not actually useful:
        //DictationTopicConstraint constraint = new DictationTopicConstraint();

        recognizer = new DictationRecognizer();
        //    new KeywordRecognizer(words, confidence_threshold);
        recognizer.DictationResult += OnPhraseRecognition;
        recognizer.DictationHypothesis += OnHypothesis;
        recognizer.DictationComplete += OnDictationComplete;
        recognizer.DictationError += OnDictationError;

        //never stop listening
        recognizer.InitialSilenceTimeoutSeconds = float.MaxValue;
        recognizer.AutoSilenceTimeoutSeconds = float.MaxValue;
        

        recognizer.Start();
    }

    private void OnPhraseRecognition(string newtext, ConfidenceLevel confidence)
    {
        //certaintext.text = certaintext.text + " " + newtext;
        string[] words = newtext.Split(split_tokens);
        foreach (string word in words) {
            if (word.Length > 0)
                line.AddWord(word);
        }
    }

    private void OnHypothesis(string newtext)
    {
        //hypothesistext.text = newtext;
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        Debug.Log(cause.ToString());
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError(error);
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null)
        {
            recognizer.DictationResult -= OnPhraseRecognition;
            recognizer.DictationHypothesis -= OnHypothesis;
            recognizer.DictationComplete -= OnDictationComplete;
            recognizer.DictationError -= OnDictationError;
            recognizer.Stop();
            recognizer.Dispose();
        }
    }
}
