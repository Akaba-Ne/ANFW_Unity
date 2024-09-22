using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ANFW 
{
    namespace Tools 
    {
        public static class CsvReader
        {
            /// <summary>
            /// テキスト形式での読み込み
            /// </summary>
            /// <param name="path">Addressablesパス</param>
            /// <returns></returns>
            public static async Task<string> getString(string path)
            {
                TextAsset csv = await AssetManager.Instance.getResource<TextAsset>(path);
                return csv.text;
            }

            /// <summary>
            /// List形式での読み込み
            /// </summary>
            /// <param name="path">Addressableパス</param>
            /// <returns></returns>
            public static async Task<List<Dictionary<string, string>>> getList(string path)
            {
                // csv の読み込み
                string csv = await CsvReader.getString(path);
                StringReader sr = new StringReader(csv);
                // 読み込んだデータの整形
                int lineNum = 0;
                List<string> keys = new List<string>();
                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                while (sr.Peek() != -1) {
                    string row = sr.ReadLine();
                    string[] columns = row.Split(',');
                    if (lineNum == 0) {
                        // 1行目：キー一覧作成
                        foreach (string column in columns) {
                            keys.Add(column);
                        }
                    } else {
                        // 2行目以降：Class作成
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        int columnNum = 0;
                        foreach (string column in columns) {
                            data.Add(keys[columnNum], column);
                            columnNum++;
                        }
                        dataList.Add(data);
                    }
                    lineNum++;
                }

                return dataList;
            }

            /// <summary>
            /// Class形式での読み込み
            /// </summary>
            /// <typeparam name="T">モデルクラス</typeparam>
            /// <param name="path">Addressableパス</param>
            /// <returns></returns>
            public static async Task<List<T>> getClass<T>(string path) where T : new()
            {
                // csv の読み込み
                string csv = await CsvReader.getString(path);
                StringReader sr = new StringReader(csv);
                // 読み込んだデータの整形
                int lineNum = 0;
                List<string> keys = new List<string>();
                List<T> classList = new List<T>();
                while (sr.Peek() != -1) {
                    string row = sr.ReadLine();
                    string[] columns = row.Split(',');
                    if (lineNum == 0) {
                        // 1行目：キー一覧作成
                        foreach (string column in columns) {
                            keys.Add(column);
                        }
                    } else {
                        // 2行目以降：Class作成
                        T model = new T();
                        int columnNum = 0;
                        foreach (string column in columns) {
                            var property = typeof(T).GetProperty(keys[columnNum]);
                            property.SetValue(model, column);
                            columnNum++;
                        }
                        classList.Add(model);
                    }
                    lineNum++;
                }

                return classList;
            }
        }
    }
}