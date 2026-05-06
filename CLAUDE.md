# ANFW (AkabaNeFramework) - Claude Code ガイド

## プロジェクト概要

Unity 向け共通フレームワーク。ゲーム開発における頻出パターンを再利用可能な形でまとめる。
想定プラットフォーム: PC / Mobile（3D ゲーム基準）

## 環境

- Unity: 6.3 (6000.3.14f1)
- 言語: C# (.NET Standard 2.1)
- 主要パッケージ:
  - Addressables（リソース管理）
  - Input System（入力管理）
  - UniTask（非同期処理）
  - Unity Test Framework（テスト）

## ディレクトリ構成

```
ANFW_Unity/                       # Unity プロジェクトルート
├── Assets/
│   ├── ANFW/
│   │   ├── Runtime/              # フレームワーク本体（スクリプト）
│   │   │   ├── Core/
│   │   │   │   ├── GameLauncher/ # 起動時シーケンス管理
│   │   │   │   ├── StateMachine/ # ゲーム状態管理
│   │   │   │   └── Logger/       # ログラッパー
│   │   │   ├── Addressables/     # リソースロード管理
│   │   │   ├── Input/            # InputSystem ラッパー
│   │   │   ├── Sound/            # SoundManager
│   │   │   ├── Scene/            # GameSceneManager
│   │   │   └── UI/               # UIManager
│   │   ├── Editor/
│   │   │   └── Addressables/     # AddressablesResources 自動登録スクリプト
│   │   └── Tests/
│   │       ├── Runtime/
│   │       └── Editor/
│   ├── AddressablesResources/    # このフォルダ配下が自動で Addressables 登録される
│   └── Scenes/
│       └── Bootstrap.unity       # 起動シーン（GameLauncher エントリポイント）
├── Packages/
└── ProjectSettings/
```

※ `Runtime/` が通常プロジェクトでいう `Scripts/` に相当する。Unity Package 規約に従いこの構成を採用。

## 設計方針

### 全般
- 各 Manager は MonoBehaviour を継承しない純粋 C# クラスを基本とし、DontDestroyOnLoad な GameObject へのアタッチは GameLauncher が一元管理する
- Manager 間の通信は直接参照を避け、EventBus 経由を原則とする
- `ANFW` namespace を必ず付与する（例: `ANFW.Sound`, `ANFW.Scene`）

### SceneManager
- Unity 組み込みの `UnityEngine.SceneManagement.SceneManager` との衝突を避けるため、クラス名は `GameSceneManager` とする

### 非同期処理
- `UniTask` を使用する（`Awaitable` は将来の移行候補として意識しつつ、現時点では UniTask で統一）
- CancellationToken は必ず引数で受け取る設計にする

### Addressables
- `AddressablesResources` フォルダ配下のアセットは Editor スクリプト（AssetPostprocessor）で自動的に Addressables グループへ登録する
- アドレスキーはフォルダ相対パスをそのまま使用する

### ログ
- `Debug.Log` を直接呼ばず、必ず `ANFWLogger` ラッパーを使う
- `DEVELOPMENT_BUILD` または `UNITY_EDITOR` シンボルが定義されていない場合は出力をすべて無効化する

## コーディング規約

- クラス・メソッド・プロパティ: `PascalCase`
- フィールド（private）: `_camelCase`（アンダースコアプレフィックス）
- 定数: `UPPER_SNAKE_CASE`
- namespace: `ANFW` またはサブ名前空間（例: `ANFW.Sound`）
- ファイル名はクラス名と一致させる
- コメントは「なぜ」が自明でない場合のみ記述（何をするかはコードで表現）

## テスト方針

- Unity Test Framework（EditMode / PlayMode）を使用
- 外部依存（Addressables、UniTask）はモック化せず Integration Test として記述する
- テストクラスは対象クラス名 + `Tests` サフィックス（例: `SoundManagerTests`）

## 注意事項

- Unity プロジェクトは `ANFW_Unity/` サブフォルダに配置されている
- `Library/`, `Temp/`, `Obj/` は .gitignore 済みのためコミット不要
- Assembly Definition ファイル（.asmdef）を各フォルダに配置し、依存関係を明示する
