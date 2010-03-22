using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Xml;

namespace General
{
    /**
    * Class Settings is used to store application values for the next launch
    * Example: Color or Cursor
    * @author David Hoffmann
    * @see SetupComponent, ColorPicker, Cursor, ServerSetup
    */
    public static class Settings
    {
        private string settingsFileName = "settings.xml";
        private static Dictionary<string, object> settingsDic = new Dictionary<string, object>();
        /**
        * read the Settings File
        * Help: if the settings.xml is deleted the read method will create a default one
        */
        static public void Read()
        {
            if (File.Exists(settingsFileName))
            {
                XmlTextReader settingsReader = new XmlTextReader(settingsFileName);
                settingsReader.Read();
                while (settingsReader.Read())
                {
                    switch (settingsReader.NodeType)
                    {
                        case XmlNodeType.Element:

                            if (settingsReader.Name != string.Empty && settingsReader.Name != "settings")
                            {
                                Set(settingsReader.Name, settingsReader.ReadElementContentAsObject());
                            }

                            break;
                    }
                }

                settingsReader.Close();
            }
            else
            {
                settingsDic.Add("color", new Color(0, 180, 255));
                settingsDic.Add("hue", new Color(90, 206, 255));
                settingsDic.Add("colorPickerCurrentPos", new Vector2(532, 100));
                settingsDic.Add("huePickerCurrentPos", new Vector2(609, 100));
                settingsDic.Add("huePosition", new Vector2(249, 22));
                settingsDic.Add("cursor.Active", true);
                Write();
                Read();
            }
        }

        /**
        * write the Settings File
        */
        public static void Write()
        {
            XmlTextWriter settingsWriter = new XmlTextWriter(settingsFileName, System.Text.Encoding.UTF8);
            settingsWriter.Formatting = Formatting.Indented;
            settingsWriter.WriteStartDocument(false);
            settingsWriter.WriteStartElement("settings");
            foreach (KeyValuePair<string, object> entry in settingsDic)
            {
                settingsWriter.WriteElementString(Convert.ToString(entry.Key), Convert.ToString(entry.Value));
            }

            settingsWriter.WriteEndElement();
            settingsWriter.Flush();
            settingsWriter.Close();
        }

        /**
        * set a specific settings value
        * @param Object obj - the setting
        */
        public static void Set(string key, object obj)
        {
            if (!settingsDic.ContainsKey(key))
            {
                settingsDic.Add(key, obj);
            }
            else
            {
                settingsDic[key] = obj;
            }
        }

        /**
        * get a specific settings value
        * @param String key - the setting to search for
        */
        public static T Get<T>(string key)
        {
            Type type = typeof(T);
            if (type == typeof(Vector2))
            {
                return (T)(object)ToVector2(settingsDic[key]);
            }

            if (type == typeof(Color))
            {
                return (T)(object)ToColor(settingsDic[key]);
            }

            if (type == typeof(bool))
            {
                return (T)(object)Convert.ToBoolean(settingsDic[key]);
            }

            if (type == typeof(string))
            {
                return (T)(object)Convert.ToString(settingsDic[key]);
            }

            return default(T);
        }
      
        private static Vector2 ToVector2(object value)
        {
            string[] tmp = Convert.ToString(value).Split(' ');
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = tmp[i].Replace("{X:", string.Empty);
                tmp[i] = tmp[i].Replace("Y:", string.Empty);
                tmp[i] = tmp[i].Replace("}", string.Empty);
            }

            return new Vector2(float.Parse(tmp[0]), float.Parse(tmp[1]));
        }

        private static Color ToColor(object value)
        {
            string[] tmp = Convert.ToString(value).Split(' ');
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = tmp[i].Replace("{R:", string.Empty);
                tmp[i] = tmp[i].Replace("G:", string.Empty);
                tmp[i] = tmp[i].Replace("B:", string.Empty);
                tmp[i] = tmp[i].Replace("A:255}", string.Empty);
            }

            return new Color(float.Parse(tmp[0]) / 255, float.Parse(tmp[1]) / 255, float.Parse(tmp[2]) / 255);
        }
    }
}
