using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Framework.IO
{
    /// <summary>
    /// Stores and retrieves data as JSON. Note: Data is cached internally as JSON.
    /// </summary>
    static class FileStorage
    {
        private static Dictionary<Type, string> _cache;

        static FileStorage()
        {
            _cache = new Dictionary<Type, string>();
        }

        public static void Save<T>(T data, string fileName) where T : class
        {
            var json = JsonUtility.ToJson(data);

            // Cache the data.
            if (_cache.ContainsKey(typeof(T)))
            {
                _cache[typeof(T)] = json;
            }
            else
            {
                _cache.Add(typeof(T), json);
            }

            try
            {
                // Attempt to write the data to JSON.
                using (var fw = new StreamWriter(Application.persistentDataPath + "." + fileName))
                {
                    fw.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public static T Load<T>(string fileName, bool forceLoad = false) where T : class
        {
            if (_cache.ContainsKey(typeof(T)) && !forceLoad)
            {
                return JsonUtility.FromJson<T>(_cache[typeof(T)]);
            }
            else
            {
                try
                {
                    using (var fs = new StreamReader(Application.persistentDataPath + fileName))
                    {
                        var json = fs.ReadToEnd();
                        var obj = JsonUtility.FromJson<T>(json);

                        // Cache json.
                        _cache.Add(typeof(T), json);

                        return obj;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

                return default(T);
            }
        }
    }
}
