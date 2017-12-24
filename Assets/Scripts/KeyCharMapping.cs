using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

/*
 * essential utility classes which provide a mapping between
 * characters and corresponding key events, and vice versa
 * (with minimal extension, at least)
 */

class KeyCharMapping
{
    static HashSet<char> requiresshift = new HashSet<char>(
        new char[]
        {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'Q',
            'R',
            'S',
            'U',
            'V',
            'W',
            'X',
            'Y',
            // non letters
            '%',
            '~',
            '{',
            '}',
            '|',
        }
    );
    static Dictionary<char, KeyCode> basecode = new Dictionary<char, KeyCode>()
    {
        { '\b', KeyCode.Backspace},
        { 'a', KeyCode.A},
        { 'b', KeyCode.B},
        { 'c', KeyCode.C},
        { 'd', KeyCode.D},
        { 'e', KeyCode.E},
        { 'f', KeyCode.F},
        { 'g', KeyCode.G},
        { 'h', KeyCode.H},
        { 'i', KeyCode.I},
        { 'j', KeyCode.J},
        { 'k', KeyCode.K},
        { 'l', KeyCode.L},
        { 'm', KeyCode.M},
        { 'n', KeyCode.N},
        { 'o', KeyCode.O},
        { 'p', KeyCode.P},
        { 'q', KeyCode.Q},
        { 'r', KeyCode.R},
        { 's', KeyCode.S},
        { 't', KeyCode.T},
        { 'u', KeyCode.U},
        { 'v', KeyCode.V},
        { 'w', KeyCode.W},
        { 'x', KeyCode.X},
        { 'y', KeyCode.Y},
        { 'z', KeyCode.Z},
        { '1', KeyCode.Alpha1},
        { '2', KeyCode.Alpha2},
        { '3', KeyCode.Alpha3},
        { '4', KeyCode.Alpha4},
        { '5', KeyCode.Alpha5},
        { '6', KeyCode.Alpha6},
        { '7', KeyCode.Alpha7},
        { '8', KeyCode.Alpha8},
        { '9', KeyCode.Alpha9},
        { '0', KeyCode.Alpha0},
        { '!', KeyCode.Exclaim},
        { '@', KeyCode.At},
        { '#', KeyCode.Hash},
        { '$', KeyCode.Dollar},
        { '%', KeyCode.Alpha5}, //requires shift
        { '^', KeyCode.Backspace},
        { '&', KeyCode.Backspace},
        { '*', KeyCode.Backspace},
        { '(', KeyCode.Backspace},
        { ')', KeyCode.Backspace},
        { '`', KeyCode.BackQuote},
        { '~', KeyCode.BackQuote}, // requires shift
        { ';', KeyCode.Semicolon},
        { ':', KeyCode.Colon},
        { '\'', KeyCode.Quote},
        { '\"', KeyCode.DoubleQuote},
        { '<', KeyCode.Less},
        { ',', KeyCode.Comma},
        { '>', KeyCode.Greater},
        { ' ', KeyCode.Space},
        { '{', KeyCode.LeftBracket}, // requires shift
        { '[', KeyCode.LeftBracket},
        { '}', KeyCode.RightBracket}, // requires shift
        { ']', KeyCode.RightBracket},
        { '-', KeyCode.Minus},
        { '_', KeyCode.Underscore},
        { '/', KeyCode.Slash},
        { '?', KeyCode.Question},
        { '+', KeyCode.Plus},
        { '=', KeyCode.Equals},
        { '\\', KeyCode.Backslash},
        { '|', KeyCode.Backslash}, // requires shift
        { '.', KeyCode.Period},
    };
    public static KeyCode GetCode(char c)
    {
        char lower = Char.ToLower(c);
        if (basecode.ContainsKey(lower))
        {
            return basecode[lower];
        }
        else
        {
            return KeyCode.None;
        }
    }
    public static List<Event> KeyEvents(char c)
    {
        List<Event> events = new List<Event>();

        KeyCode code = GetCode(c);
        if (code != KeyCode.None)
        {
            if (requiresshift.Contains(c))
            {
                events.Add(new Event() { type = EventType.KeyDown, keyCode = KeyCode.LeftShift});
                events.Add(new Event() { type = EventType.KeyDown, keyCode = code });
                // the following event shouldn't logically have to be put in, but
                // unity creates it when I do normal keyboard presses...
                events.Add(new Event() { type = EventType.KeyDown, keyCode = KeyCode.None, character = c });
                events.Add(new Event() { type = EventType.KeyUp, keyCode = code });
                events.Add(new Event() { type = EventType.KeyUp, keyCode = KeyCode.LeftShift });
            }
            else
            {
                events.Add(new Event() { type = EventType.KeyDown, keyCode = code });
                events.Add(new Event() { type = EventType.KeyDown, keyCode = KeyCode.None, character = c });
                events.Add(new Event() { type = EventType.KeyUp, keyCode = code });
            }
        }
        Debug.Log("Mapping Produced " + events.Count + " Events ");
        return events;
    }
}
