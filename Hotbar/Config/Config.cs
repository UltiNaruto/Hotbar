using Hotbar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hotbar
{
    public class Orientation
    {
        int val;
        public static readonly Orientation Horizontal = new Orientation(0);
        public static readonly Orientation Vertical = new Orientation(1);

        Orientation(int v) { this.val = v; }

        public static Orientation FromStr(string ori)
        {
            switch (ori.ToLower())
            {
                case "horizontal":
                    return Orientation.Horizontal;
                case "vertical":
                    return Orientation.Vertical;
                default:
                    throw new Exception($"Unknown orientation value {ori}");
            }
        }

        public override String ToString()
        {
            switch (val)
            {
                case 0:
                    return "horizontal";
                case 1:
                    return "vertical";
                default:
                    throw new Exception($"Unknown orientation {val}");
            }
        }
    }

    public class ShowToggle
    {
        int val;
        public static readonly ShowToggle Icon = new ShowToggle(0);
        public static readonly ShowToggle Text = new ShowToggle(1);

        ShowToggle(int v) { this.val = v; }

        public static ShowToggle FromStr(string ori)
        {
            switch (ori.ToLower())
            {
                case "icon":
                    return ShowToggle.Icon;
                case "text":
                    return ShowToggle.Text;
                default:
                    throw new Exception($"Unknown show toggle value {ori}");
            }
        }

        public override String ToString()
        {
            switch (val)
            {
                case 0:
                    return "icon";
                case 1:
                    return "text";
                default:
                    throw new Exception($"Unknown show toggle {val}");
            }
        }
    }

    public class Config
    {
        public readonly Rect Hotbar = new Rect();
        public readonly Orientation HotbarOrientation;
        public readonly bool ShowItemAmount;
        public readonly bool ShowItemDuration;
        public readonly bool ShowAmmo;
        public readonly ShowToggle ShowItemAs;

        public Config(String jsonStr)
        {
            JSON json = new JSON(jsonStr);
            HotbarOrientation = Orientation.FromStr((string)json.AsDict["orientation"]);

            //ShowItemAs = ShowToggle.FromStr((string)json.AsDict["show_item_as"]);
            ShowItemAs = ShowToggle.Text;
            ShowItemAmount = (bool)json.AsDict["show_item_amount"];
            ShowItemDuration = (bool)json.AsDict["show_item_duration"];
            ShowAmmo = (bool)json.AsDict["show_ammo"];

            (Hotbar.position, Hotbar.size) = ReadPadding(((List<string>)json.AsDict["padding"]).ToArray());
        }

        Tuple<Vector2, Vector2> ReadPadding(string[] padding, Rect parent = default(Rect))
        {
            float top, bottom, left, right, tmp;

            if (!padding.All(p => p.Contains('%')))
                throw new Exception("Invalid padding detected!");

            if (parent == default(Rect))
                parent = new Rect(0, 0, Screen.width, Screen.height);

            string[] trimmed_padding = padding.Select(p => p.Trim(' ')).ToArray();

            if (trimmed_padding[0][0] == '-')
            {
                left = parent.width - ((float)int.Parse(trimmed_padding[0].Substring(1, trimmed_padding[0].Length - 1)) * parent.width / 100f);
            }
            else
            {
                left = (float)int.Parse(trimmed_padding[0].Substring(0, trimmed_padding[0].Length - 1)) * parent.width / 100f;
            }

            if (trimmed_padding[1][0] == '-')
            {
                top = parent.height - ((float)int.Parse(trimmed_padding[1].Substring(1, trimmed_padding[1].Length - 1)) * parent.height / 100f);
            }
            else
            {
                top = (float)int.Parse(trimmed_padding[1].Substring(0, trimmed_padding[1].Length - 1)) * parent.height / 100f;
            }

            if (trimmed_padding[2][0] != '-')
            {
                right = parent.width - ((float)int.Parse(trimmed_padding[2].Substring(0, trimmed_padding[2].Length - 1)) * parent.width / 100f);
            }
            else
            {
                right = (float)int.Parse(trimmed_padding[2].Substring(1, trimmed_padding[2].Length - 1)) * parent.width / 100;
            }

            if (trimmed_padding[3][0] != '-')
            {
                bottom = parent.height - ((float)int.Parse(trimmed_padding[3].Substring(0, trimmed_padding[3].Length - 1)) * parent.height / 100f);
            }
            else
            {
                bottom = (float)int.Parse(trimmed_padding[3].Substring(1, trimmed_padding[3].Length - 1)) * parent.height / 100f;
            }

            if (right < left)
            {
                tmp = right;
                right = left;
                left = tmp;
            }

            if (bottom < top)
            {
                tmp = bottom;
                bottom = top;
                top = tmp;
            }

            return Tuple.New<Vector2, Vector2>(new Vector2(left, top), new Vector2(right - left, bottom - top));
        }
    }
}
