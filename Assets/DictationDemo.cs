using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class DictationDemo: MonoBehaviour
{

    public Text certaintext;
    public Text hypothesistext;

    protected DictationRecognizer recognizer;

    private void Start()
    {
            recognizer = new DictationRecognizer();
            //    new KeywordRecognizer(words, confidence_threshold);
            recognizer.DictationResult += Recognizer_OnPhraseRecognized;
            recognizer.DictationHypothesis += OnHypothesis;
            recognizer.DictationComplete += OnDictationComplete;
            recognizer.DictationError += OnDictationError;
            recognizer.Start();
    }

    private void Recognizer_OnPhraseRecognized(string newtext, ConfidenceLevel confidence)
    {
        certaintext.text = certaintext.text + " " + newtext;
    }

    private void OnHypothesis(string newtext)
    {
        hypothesistext.text = newtext;
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
            recognizer.DictationResult -= Recognizer_OnPhraseRecognized;
            recognizer.DictationHypothesis -= OnHypothesis;
            recognizer.DictationComplete -= OnDictationComplete;
            recognizer.DictationError -= OnDictationError;
            recognizer.Stop();
            recognizer.Dispose();
        }
    }
}


