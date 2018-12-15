using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThunderWire.JsonManager
{
    [Serializable]
    public enum FilePath
    {
        GameDataPath,
        GameSavesPath,
        DocumentsPath
    }

    /// <summary>
    /// Provides methods for writing and reading JSON files.
    /// </summary>
    public static class JsonManager
    {
        private static string folderPath;
        private static string fullPath;

        private static bool enableDebug = false;
        private static bool enableEncryption = false;
        private static bool pathSet = false;

        private static Dictionary<string, object> JsonArray = new Dictionary<string, object>();

        private static string jsonString = "";
        private static string cipherKey = "";


        public static void EnableDebugging(bool Enabled)
        {
            enableDebug = Enabled;
        }

        public static void Settings(SaveLoadScriptable managerSettings)
        {
            cipherKey = managerSettings.cipherKey;
            enableEncryption = managerSettings.enableEncryption;
            SetFilePath(managerSettings.filePath);
        }

        public static void SerializeData(string Filename)
        {
            if (!pathSet)
            {
                folderPath = FolderPath(FilePath.GameSavesPath);
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (Filename.Contains('.'))
            {
                fullPath = folderPath + Filename;
            }
            else
            {
                fullPath = folderPath + Filename + ".sav";
            }

            JsonSerialize();

            if (enableDebug) { Debug.Log("<color=green>Game Saved: </color> " + fullPath); }
        }

        public static void DeserializeData(string Filename)
        {
            if (Filename.Contains('.'))
            {
                fullPath = folderPath + Filename;
            }
            else
            {
                fullPath = folderPath + Filename + ".sav";
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = JsonRead();
        }

        public static void DeserializeData(FilePath filePath, string Filename)
        {
            if (Filename.Contains('.'))
            {
                fullPath = FolderPath(filePath) + Filename;
            }
            else
            {
                fullPath = FolderPath(filePath) + Filename + ".sav";
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = JsonRead();
        }

        public static void SetFilePath(FilePath Filepath)
        {
            folderPath = FolderPath(Filepath);
            pathSet = true;
        }

        public static string FolderPath(FilePath Filepath)
        {
            if (Filepath == FilePath.GameSavesPath)
            {
                return Application.dataPath + "/Data/SavedGame/";
            }
            else if (Filepath == FilePath.GameDataPath)
            {
                return Application.dataPath + "/Data/";
            }
            else if (Filepath == FilePath.DocumentsPath)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/" + "SavedGame/";
            }

            return null;
        }

        public static void SetCustomPath(string customPath)
        {
            folderPath = customPath;
            pathSet = true;
        }

        public static string GetCurrentPath()
        {
            return folderPath;
        }

        public static void Clear()
        {
            JsonArray.Clear();
            jsonString = "";
        }

        public static void UpdateJsonArray(string Key, object Value)
        {
            JsonArray.Add(Key, Value);
        }

        public static string JsonOut()
        {
            return jsonString;
        }

        public static JObject Json()
        {
            JObject rss = JObject.Parse(jsonString);

            return rss;
        }

        public static JObject Json(string Json)
        {
            JObject rss = JObject.Parse(Json);

            return rss;
        }

        public static T Json<T>()
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static T JsonString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static string EncryptData(byte[] toEncrypt)
        {
            byte[] result;
            byte[] IV;
            byte[] AESkey = Encoding.UTF8.GetBytes(cipherKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = AESkey;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;

                IV = aes.IV;

                try
                {
                    ICryptoTransform Encryptor = aes.CreateEncryptor();
                    result = Encryptor.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
                }
                finally
                {
                    aes.Clear();
                }
            }

            byte[] cmbIV = new byte[IV.Length + result.Length];
            Array.Copy(IV, 0, cmbIV, 0, IV.Length);
            Array.Copy(result, 0, cmbIV, IV.Length, result.Length);

            return Convert.ToBase64String(cmbIV);
        }

        private static string DecryptString(byte[] toDecrypt)
        {
            byte[] result;
            byte[] DataToDecrypt = toDecrypt;
            byte[] AESkey = Encoding.UTF8.GetBytes(cipherKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = AESkey;

                byte[] IV = new byte[aes.BlockSize / 8];
                byte[] cipherText = new byte[DataToDecrypt.Length - IV.Length];
                Array.Copy(DataToDecrypt, IV, IV.Length);
                Array.Copy(DataToDecrypt, IV.Length, cipherText, 0, cipherText.Length);

                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                try
                {
                    ICryptoTransform Encryptor = aes.CreateDecryptor();
                    result = Encryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
                finally
                {
                    aes.Clear();
                }
            }

            return Encoding.UTF8.GetString(result);
        }

        private static void JsonSerialize()
        {
            string jsonString = JsonConvert.SerializeObject(JsonArray, Formatting.Indented);

            if (!enableEncryption)
            {
                File.WriteAllText(fullPath, jsonString);
            }
            else
            {
                string encryptedString = EncryptData(Encoding.UTF8.GetBytes(jsonString));
                File.WriteAllText(fullPath, encryptedString);
            }
        }

        public static FileInfo GetSerializeInfo()
        {
            return new FileInfo(fullPath);
        }

        private static string JsonRead()
        {
            string json = "";

            if (File.Exists(fullPath))
            {
                string jsonRead = File.ReadAllText(fullPath);

                if (enableEncryption)
                {
                    json = DecryptString(Convert.FromBase64String(jsonRead));
                }
                else
                {
                    json = jsonRead;
                }

                if (enableDebug) { Debug.Log("<color=green>Json string readed successfully</color>"); }

                return json;
            }
            else
            {
                if (enableDebug) { Debug.Log("<color=red>File does not exist: </color> " + fullPath); }
            }

            return null;
        }
    }
}