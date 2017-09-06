using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechDemo : MonoBehaviour
{
    public string[] words = new string[] { "up", "down", "left", "right" };
    public ConfidenceLevel confidence_threshold;

    private Text text;

    protected PhraseRecognizer recognizer;

    private void Start()
    {
        text = GetComponent<Text>();
        if (words != null)
        {
            recognizer = new KeywordRecognizer(words, confidence_threshold);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
        }
    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        text.text = args.text + " : " + args.confidence.ToString();
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }
}

