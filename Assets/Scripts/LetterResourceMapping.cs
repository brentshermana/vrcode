using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterResourceMapping
{
    private static bool initialized;

    private static Dictionary<char, string> special_character_names;

    public static string resource_fname(char c)
    {
        construct();

        string ret;
        if (char.IsLetter(c))
        {
            ret = c.ToString();
            if (char.IsUpper(c)) ret += "_upper";
        }
        else if (special_character_names.ContainsKey(c))
        {
            ret = special_character_names[c];
        }
        else
        {
            Debug.LogError("Error: char '" + c + "' is not valid!");
            ret = "";
        }
        return ret;
    }

    private static void construct()
    {
        if (initialized) return;

        special_character_names = new Dictionary<char, string>();
        special_character_names.Add('&', "ampersand");
        special_character_names.Add('@', "at");
        special_character_names.Add('*', "asterisk");
        special_character_names.Add('\\', "backslash");
        special_character_names.Add('`', "backtick");
        special_character_names.Add('^', "caret");
        special_character_names.Add(':', "colon");
        special_character_names.Add('$', "dollar");
        special_character_names.Add('"', "double_quote");
        special_character_names.Add('8', "eight");
        special_character_names.Add('=', "equals");
        special_character_names.Add('!', "exclaimation");
        special_character_names.Add('/', "forward_slash");
        special_character_names.Add('4', "four");
        special_character_names.Add('?', "question");
        special_character_names.Add('>', "greater_than");
        special_character_names.Add('#', "hashtag");
        special_character_names.Add('{', "left_curly_bracket");
        special_character_names.Add('(', "left_parenthesis");
        special_character_names.Add('[', "left_square_bracket");
        special_character_names.Add('.', "period");
        special_character_names.Add(',', "comma");
        special_character_names.Add('<', "less_than");
        special_character_names.Add('-', "minus");
        special_character_names.Add('9', "nine");
        special_character_names.Add('5', "five");
        special_character_names.Add('1', "one");
        special_character_names.Add('%', "percent");
        special_character_names.Add('+', "plus");
        special_character_names.Add('}', "right_curly_bracket");
        special_character_names.Add(')', "right_parenthesis");
        special_character_names.Add(']', "right_square_bracket");
        special_character_names.Add(';', "semicolon");
        special_character_names.Add('7', "seven");
        special_character_names.Add('\'', "single_quote");
        special_character_names.Add('6', "six");
        special_character_names.Add('3', "three");
        special_character_names.Add('~', "tilda");
        special_character_names.Add('2', "two");
        special_character_names.Add('_', "underscore");
        special_character_names.Add('|', "vertical_bar");
        special_character_names.Add('0', "zero");

        initialized = true;
    }

    
}

