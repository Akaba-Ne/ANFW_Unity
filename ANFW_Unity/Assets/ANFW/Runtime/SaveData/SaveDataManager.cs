using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ANFW.SaveData
{
    public class SaveDataManager
    {
        private string _saveRoot;

        /// <summary>
        /// セーブルートディレクトリを作成して初期化する
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        /// <param name="saveRoot">セーブデータの保存先ルートパス。null の場合は persistentDataPath/save を使用する</param>
        public UniTask InitializeAsync(CancellationToken ct, string saveRoot = null)
        {
            _saveRoot = saveRoot ?? Path.Combine(Application.persistentDataPath, "save");

            if (!Directory.Exists(_saveRoot))
                Directory.CreateDirectory(_saveRoot);

            ANFWLogger.Log("SaveDataManager: Initialized");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// SaveDataManager を破棄する
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// data を JSON にシリアライズして指定スロットの {key}.json に保存する
        /// </summary>
        /// <param name="slot">セーブスロット番号（スロット 0 は共通データ用として推奨）</param>
        /// <param name="key">データを識別するキー名（英数字・アンダースコア推奨）</param>
        /// <param name="data">保存するデータ。[Serializable] を付与したクラスを渡すこと</param>
        public void Save<T>(int slot, string key, T data)
        {
            var dir = GetSlotDirectory(slot);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(GetFilePath(slot, key), JsonUtility.ToJson(data));
        }

        /// <summary>
        /// 指定スロットの {key}.json を読み込んで T にデシリアライズして返す。ファイルが存在しない場合は default(T) を返す
        /// </summary>
        /// <param name="slot">セーブスロット番号</param>
        /// <param name="key">データを識別するキー名</param>
        public T Load<T>(int slot, string key)
        {
            var path = GetFilePath(slot, key);
            if (!File.Exists(path)) return default;

            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }

        /// <summary>
        /// 指定スロットの {key}.json が存在するか確認する
        /// </summary>
        /// <param name="slot">セーブスロット番号</param>
        /// <param name="key">データを識別するキー名</param>
        public bool HasKey(int slot, string key) => File.Exists(GetFilePath(slot, key));

        /// <summary>
        /// 指定スロットの {key}.json を削除する
        /// </summary>
        /// <param name="slot">セーブスロット番号</param>
        /// <param name="key">データを識別するキー名</param>
        public void Delete(int slot, string key)
        {
            var path = GetFilePath(slot, key);
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// 指定スロットのディレクトリごと削除する（スロット全体のリセット）
        /// </summary>
        /// <param name="slot">削除するセーブスロット番号</param>
        public void DeleteSlot(int slot)
        {
            var dir = GetSlotDirectory(slot);
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }

        /// <summary>
        /// すべてのスロットのセーブデータを削除する
        /// </summary>
        public void DeleteAll()
        {
            if (Directory.Exists(_saveRoot))
                Directory.Delete(_saveRoot, recursive: true);

            Directory.CreateDirectory(_saveRoot);
        }

        private string GetSlotDirectory(int slot) => Path.Combine(_saveRoot, $"slot_{slot}");
        private string GetFilePath(int slot, string key) => Path.Combine(GetSlotDirectory(slot), $"{key}.json");
    }
}
