using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YMatchThree.Core {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SlotTagAttribute : Attribute {
        int priority;
        Color color;
        Color background;
        
        public SlotTagAttribute(ConsoleColor color = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black, int priority = 0) {
            this.priority = priority;
            this.color = GetColor(color);
            this.background = GetColor(background);
        }
        
        public Color GetSymbolColor() {
            return color;
        }

        public Color GetBackgroundColor() {
            return background;
        }
        
        static Color GetColor(ConsoleColor color) {
            switch (color) {
                case ConsoleColor.Black: return Color.black;
                case ConsoleColor.Blue: return Color.blue;
                case ConsoleColor.Cyan: return Color.cyan;
                case ConsoleColor.DarkBlue: return new Color(0, 0, .5f, 1);
                case ConsoleColor.DarkCyan: return new Color(0, .5f, .5f, 1);
                case ConsoleColor.DarkGray: return new Color(.25f, .25f, .25f, 1);
                case ConsoleColor.DarkGreen: return new Color(0, .5f, 0, 1);
                case ConsoleColor.DarkMagenta: return new Color(.5f, 0, .5f, 1);
                case ConsoleColor.DarkRed: return new Color(.5f, 0, 0, 1);
                case ConsoleColor.DarkYellow: return new Color(.5f, .46f, .008f, 1);
                case ConsoleColor.Gray: return Color.gray;
                case ConsoleColor.Green: return Color.green;
                case ConsoleColor.Magenta: return Color.magenta;
                case ConsoleColor.Red: return Color.red;
                case ConsoleColor.White: return Color.white;
                case ConsoleColor.Yellow: return Color.yellow;
            }
            return Color.clear;
        }
    }
}