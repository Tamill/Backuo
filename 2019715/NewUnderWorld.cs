using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using AnimeBodyGuardManager;
/*==================== New Under World Language - The simple game event runner ===========================
* Open Source Project: UW Engine
* Development Team: Alive Game Studio (Tamill)
* All functions only developed by C# API, see UWAPI.
* Copyright (c) 2017-2018 by Alive Game Studio (Tamill), All rights reserved. ***
*/

namespace Addon {

    public static class NewUnderWorld
    {
        public static Dictionary<string, object> GlobalVariable = new Dictionary<string, object>();
        static public object Inko(this string LocalName)
        {
            object value; var IsFinished = GlobalVariable.TryGetValue(LocalName, out value);
            return IsFinished ? value : LocalName;
        }

        public static async Task RunCode(string code){
            StringBuilder builder = new StringBuilder();
            bool includingStr = false;
            char lastWord = ' ';
            string lastLine = "";
            int __index = 0;
            try
            {
                foreach (char word in code)
                {
                    switch (word)
                    {
                        case '[': __index++; if (__index == 1) includingStr = true; break;
                        case ']': __index--; if (__index == 0) includingStr = false; break;
                    }
                    builder.Append(word);
                    if(!includingStr && (int)word == 10 && builder.Length > 0){
                        lastLine = builder.ToString().Trim();
                        if(lastLine.Length > 2 ? lastLine[0] != '/' && lastLine[1] != '/' : true)
                        {
                            var ret = InLine(lastLine);
                            if (ret is Task)
                                await (Task)ret;
                        }
                        builder.Clear();
                    }
                    lastWord = word;

                }  
                if(builder.Length > 0 && !includingStr)
                {
                    lastLine = builder.ToString().Trim();
                    if (lastLine.Length > 2 ? lastLine[0] != '/' && lastLine[1] != '/' : true)
                    {
                        var ret = InLine(lastLine);
                        if (ret is Task)
                            await (Task)ret;
                    }
                }
            }
            catch (Exception e) {
                if (e is System.Threading.Tasks.TaskCanceledException) return;
                var codeErrorLog = "[UW ERROR]: "  + lastLine.Trim();
                if (e is System.IndexOutOfRangeException)
                    codeErrorLog += "参数数量不匹配";
                Form1.ShowMessage(codeErrorLog + "\n" + e);

            }
        }



        static public object InLine(string info)
        {
            StringBuilder builder = new StringBuilder();
            object[] _params = null;
            ParameterInfo[] _paramInfos = null;
            MethodInfo _methodInfo = null ;
            bool includingStr = false;
            bool includingArray = false;
            char lastWord = ' ';
            int index = 0;
            int _index = 0;
            int __index = 0;
            int _operator = 0;
            string force = null;
            string force2 = null;
            for (int i = 0; i < info.Length; i++){
                var word = info[i];

                switch (word)
                {
                    case '[': __index++; if (__index == 1) { includingStr = true; continue; } break;
                    case ']': __index--; if (__index == 0) { includingStr = false; continue; } break;
                }
                if (!includingStr){
                    switch (word){
                        case ' ': continue;
                        case '?': if(index == 0)continue; break;
                        case '{': if((index == 1 && _operator == 0) || (index == 0 && _operator == 1)) includingArray = true; break;
                        case '}': if ((index == 1 && _operator == 0) || (index == 0 && _operator == 1)) includingArray = false; break;
                        case '+':
                            if(_operator == 0 && lastWord == '+' && index == 0)
                            {
                                force = builder.ToString().Remove(builder.Length-1,1);
                                builder.Clear();
                                _operator = 1052;
                                continue;
                            }
                            break;
                        case '<':
                            if (_operator == 0 && index == 0)
                            {
                                force = builder.ToString();
                                builder.Clear();
                                _operator = 1053;
                                continue;
                            }
                            break;
                        case '=':
                            if(lastWord != '=' && index == 0 && _operator == 0){
                                _operator = 1;
                                force = builder.ToString();
                                if (force == "lua") _operator = 2;
                                builder.Clear();
                                continue;
                            }
                            break;
                        case '@':
                            if (index == 0 && _operator == 0)
                            {
                                _operator = 3;
                                force = builder.ToString();
                                builder.Clear();
                                continue;
                            }
                            break;
                        case ':':
                        case '：':
                            if (index == 0 && _operator == 3)
                            {
                                force2 = builder.ToString();
                                builder.Clear();
                                continue;
                            }
                            if (index == 0 && _operator == 0)
                            {
                                force = builder.ToString();
                                _operator = 3;
                                if (force == "wait") _operator = 4;
                                builder.Clear();
                                continue;
                            }
                            break;
                            
                        case ',':
                            if (includingArray)
                            {
                                builder.Append("|");
                                continue;
                            }
                            else if (index == 1 && _operator == 0)
                            {
                                _params[_index] = Translator(_paramInfos[_index].ParameterType, builder.ToString());
                                _index++;
                                builder.Clear();
                                continue;
                            }
                            break;
                        case '(':
                            index++;
                            if (index == 1 && _operator == 0)
                            {
                                _methodInfo = GetMethod(builder.ToString());
                                if (_methodInfo == null) return false;
                                _paramInfos = _methodInfo.GetParameters();
                                _params = new object[_paramInfos.Length];
                                builder.Clear();
                                continue;
                            }
                            break;
                        case ')':
                            if (index == 1 && _operator == 0)
                            {
                                if(_index < _params.Length)
                                    _params[_index] = Translator(_paramInfos[_index].ParameterType, builder.ToString());
                                return _methodInfo.Invoke(null, _params);
                            }
                            index--;
                            break;
                    }
                }
                builder.Append(word);
                lastWord = word;
            }
            switch (_operator){
                case 1: GlobalVariable[force] = Translator(null, builder.ToString());    break;
                case 2: break;
                case 3: break;
                case 4: break;
                case 1052: break;
                case 1053: break;
            }
            return false;
        }

        public static MethodInfo GetTypeMethod(string type, string methodName)
        {
            Type _type = null;
            bool right = SavedType.TryGetValue(type, out _type);
            if (_type == null || !right)
            {
                _type = Type.GetType(type);
                if (_type == null)
                {
                    _type = Type.GetType(type + ", UnityEngine", true);
                }
                SavedType[type] = _type;
            }
            MethodInfo func = null;
            right = SavedMethods.TryGetValue(type + "." + methodName, out func);
            if (func == null || !right)
            {
                func = _type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (func == null)
                    return null;
                SavedMethods[type + "." + methodName] = func;
            }
            return func;
        }

        public static MethodInfo GetMethod(string methodName)
        {
            MethodInfo func = null;
            bool right = SavedMethods.TryGetValue("UWAPI" + "." + methodName, out func);
            if (func == null || !right)
            {
                func = typeof(UWAPI).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (func == null)
                    return null;
                SavedMethods[methodName] = func;
            }
            return func;
        }

        public static object Translator(Type type, string key)
        {
            if(key.Length < 1)return false;

            if (key[0] == '{' && key[key.Length - 1] == '}')
            {
                var g = key.Remove(0, 1).Remove(key.Length - 2, 1).Split('|');

                string[] gplus = new string[g.Length];
                for (int i = 0; i < g.Length; i++)
                {
                    gplus[i] = (string)Translator(typeof(string), g[i]);
                }
                return gplus;
            }
                

            if (key[0] == '@')
                return key.Length > 1 ? key[1] != '!' ? InLine(key.Remove(0,1)) : !(bool)InLine(key.Remove(0, 2)) : InLine(key.Remove(0, 1));

            if (type == typeof(string))
                return key.Inko().ToString();

            if (type == typeof(int))
                return Convert.ToInt32(key.Inko());

            if (type == typeof(float))
                return Convert.ToSingle(key.Inko());

            if (type == typeof(double))
                return Convert.ToDouble(key.Inko());

            if (type == typeof(bool))
                return key[0] != '!' ? Convert.ToBoolean(key.Inko()) : !Convert.ToBoolean(key.Remove(0,1).Inko());

            return key.Inko();
        }
        public static Dictionary<string, Type> SavedType = new Dictionary<string, Type>();
        public static Dictionary<string, MethodInfo> SavedMethods = new Dictionary<string, MethodInfo>();
    }
}
