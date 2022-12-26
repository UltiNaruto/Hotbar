/*
 * Uses : https://notes.eatonphil.com/writing-a-simple-json-parser.html
 * from Phil Eaton
 */

using Hotbar.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Hotbar
{
    public class JSON
    {
        const char JSON_COMMA = ',';
        const char JSON_COLON = ':';
        const char JSON_LEFTBRACKET = '[';
        const char JSON_RIGHTBRACKET = ']';
        const char JSON_LEFTBRACE = '{';
        const char JSON_RIGHTBRACE = '}';
        const char JSON_QUOTE = '"';
        char[] JSON_WHITESPACE = new char[] { ' ', '\t', '\b', '\n', '\r' };
        char[] JSON_SYNTAX = new char[] { JSON_COMMA, JSON_COLON, JSON_LEFTBRACKET, JSON_RIGHTBRACKET, JSON_LEFTBRACE, JSON_RIGHTBRACE };

        public Dictionary<string, object> AsDict = null;

        public JSON(string json)
        {
            var tokens = Lex(json);
            var (json_dict, _) = Parse(tokens, true);
            AsDict = (Dictionary<string, object>)json_dict;
        }

        object[] Lex(string json)
        {
            List<object> tokens = new List<object>();
            char c = '\0';
            object json_string = null;
            object json_number = null;
            object json_bool = null;
            object json_null = null;

            while (json.Length > 0)
            {
                (json_string, json) = LexString(json);
                if(json_string != null)
                {
                    tokens.Add(json_string);
                    continue;
                }

                (json_number, json) = LexNumber(json);
                if (json_number != null)
                {
                    tokens.Add(json_number);
                    continue;
                }

                (json_bool, json) = LexBool(json);
                if (json_bool != null)
                {
                    tokens.Add(json_bool);
                    continue;
                }

                (json_null, json) = LexNull(json);
                if (json_null != null)
                {
                    tokens.Add(null);
                    continue;
                }

                c = json[0];

                if (JSON_WHITESPACE.Any(ch => ch == c))
                    json = json.Substring(1);
                else if (JSON_SYNTAX.Any(ch => ch == c))
                {
                    tokens.Add($"{c}");
                    json = json.Substring(1);
                }
                else
                    throw new System.Exception($"Unexpected character: {c}");
            }

            return tokens.ToArray();
        }

        Tuple<object, string> LexString(string json)
        {
            string json_string = "";
            if (json[0] == JSON_QUOTE)
                json = json.Substring(1);
            else
                return Tuple.New<object, string>(null, json);

            foreach(var c in json)
            {
                if (c == JSON_QUOTE)
                    return Tuple.New<object, string>(json_string, json.Substring(json_string.Length + 1));
                else
                    json_string += c;
            }
            throw new System.Exception("Expected end-of-string quote");
        }

        Tuple<object, string> LexNumber(string json)
        {
            string json_number = "";

            string number_chars = "0123456789-e.";

            foreach (var c in json)
            {
                if (number_chars.ToCharArray().Any(ch => ch == c))
                {
                    if (c == '.')
                        json_number += ',';
                    else
                        json_number += c;
                }
                else
                    break;
            }

            var rest = json.Substring(json_number.Length);

            if(json_number == "")
                return Tuple.New<object, string>(null, json);

            if(json_number.Contains(','))
                return Tuple.New<object, string>(float.Parse(json_number), rest);

            return Tuple.New<object, string>(int.Parse(json_number), rest);
        }

        Tuple<object, string> LexBool(string json)
        {
            if (json.Length >= 4 && json.Substring(0, 4) == "true")
                return Tuple.New<object, string>(true, json.Substring(4));
            else if (json.Length >= 5 && json.Substring(0, 5) == "false")
                return Tuple.New<object, string>(true, json.Substring(5));
            else
                return Tuple.New<object, string>(null, json);
        }

        Tuple<object, string> LexNull(string json)
        {
            if (json.Length >= 4 && json.Substring(0, 4) == "null")
                return Tuple.New<object, string>(true, json.Substring(0, 4));
            else
                return Tuple.New<object, string>(null, json);
        }

        Tuple<object, object[]> Parse(object[] tokens, bool is_root=false)
        {
            object t = tokens[0];

            if (is_root && (string)t != $"{JSON_LEFTBRACE}")
                throw new System.Exception("Root must be an object");

            if (t is string)
            {
                if ((string)t == $"{JSON_LEFTBRACKET}")
                    return ParseArray(tokens.Skip(1).ToArray());
                else if ((string)t == $"{JSON_LEFTBRACE}")
                    return ParseObject(tokens.Skip(1).ToArray());
            }
            return Tuple.New<object, object[]>(t, tokens.Skip(1).ToArray());
        }

        Tuple<object, object[]> ParseArray(object[] tokens)
        {
            List<string> json_array = new List<string>();
            object json = null;

            object t = tokens[0];

            if ((string)t == $"{JSON_RIGHTBRACKET}")
                return Tuple.New<object, object[]>(json_array, tokens.Skip(1).ToArray());

            while (true)
            {
                (json, tokens) = Parse(tokens);
                json_array.Add($"{json}");

                t = tokens[0];

                if ((string)t == $"{JSON_RIGHTBRACKET}")
                    return Tuple.New<object, object[]>(json_array, tokens.Skip(1).ToArray());
                else if ((string)t != $"{JSON_COMMA}")
                    throw new System.Exception("Expected comma after object in array");
                else
                    tokens = tokens.Skip(1).ToArray();
            }

            throw new System.Exception("Expected end-of-array bracket");
        }

        Tuple<object, object[]> ParseObject(object[] tokens)
        {
            Dictionary<string, object> json_object = new Dictionary<string, object>();
            object json_key = null;
            object json_value = null;

            object t = tokens[0];

            if ((string)t == $"{JSON_RIGHTBRACE}")
                return Tuple.New<object, object[]>(json_object, tokens.Skip(1).ToArray());

            while (true)
            {
                json_key = tokens[0];
                if (json_key is string)
                    tokens = tokens.Skip(1).ToArray();
                else
                    throw new System.Exception($"Expected string key, got: {json_key}");

                if ((string)tokens[0] != $"{JSON_COLON}")
                    throw new System.Exception($"Expected colon after key in object, got: {t}");

                (json_value, tokens) = Parse(tokens.Skip(1).ToArray());

                json_object.Add((string)json_key, json_value);

                t = tokens[0];

                if ((string)t == $"{JSON_RIGHTBRACE}")
                    return Tuple.New<object, object[]>(json_object, tokens.Skip(1).ToArray());
                else if ((string)t != $"{JSON_COMMA}")
                    throw new System.Exception($"Expected comma after pair in object, got: {t}");
                
                tokens = tokens.Skip(1).ToArray();
            }

            throw new System.Exception("Expected end-of-array bracket");
        }
    }
}
