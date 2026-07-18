# CLAUDE.md — Temae Trainer

裏千家・薄茶平点前(風炉)のMRトレーニングアプリ。Meta Quest 3S のパススルー上に、畳への足運びガイドと手順ナビを重畳表示する。詳細仕様は `Docs/SPEC_temae_trainer.md` を必ず参照すること。

## 技術スタック

- Unity 6(6000.x LTS)/ URP / Input System(新)
- Meta XR All-in-One SDK(v203系)+ Building Blocks / XRプラグインは OpenXR
- 対象デバイス: Meta Quest 3S のみ(コントローラは補助、基本ハンドトラッキング)
- データ: `Assets/StreamingAssets/Data/` 配下の JSON(手順・足運び)
- AI開発ループ: Unity CLI Loop(`uloop` コマンド)。コンパイル・テスト・ログ取得・エディタ操作に使用。開発時専用ツールであり、アプリ本体のコードから依存してはならない(将来差し替え可能なレイヤーとして扱う。代替は Unity 純正 `-batchmode` CLI)

## プロジェクト構成

```
Assets/App/
  Scripts/
    Calibration/   # 使用畳(temae/approach1..)の座標系確立・Spatial Anchor永続化
    Sequence/      # JSONロード、ステップ進行のステートマシン(唯一の状態保持者)
    Guide/         # 足跡・畳輪郭・道具ゴーストの表示(Sequenceを購読)
    UI/            # 手順パネル、注視ドウェルボタン、メニュー(Sequenceを購読)
  Data/            # 点前JSON(StreamingAssetsへ移す前の編集用)
  Prefabs/
Docs/              # SPEC_temae_trainer.md ほか設計ドキュメント
```

## アーキテクチャ原則

- 一方向データフロー: `Sequence`(ステートマシン)が現在ステップを保持し、`Guide` と `UI` はイベント購読で表示を更新するだけ。表示側から状態を書き換えない
- データ駆動: 点前の内容(所作テキスト・足跡座標・道具配置)は必ず JSON に置く。C# に点前知識をハードコードしない。内容の正誤は人間が監修する領域であり、Claude はスキーマ整合性のみ責任を持つ
- 座標系: 原点は点前畳(`temae`)。足跡は「畳役割ID + 畳ローカル正規化座標(u,v: 0..1)+ yawDeg」で表現(SPEC §6 参照)
- 軽量方針: リアルタイム影・ポストプロセス・重いアセット禁止。72fps 維持

## 役割分担(重要)

- Claude Code がやる: C# スクリプト、JSON スキーマとバリデータ、エディタ拡張。加えて Unity CLI Loop(`uloop`)経由で以下を自走する:
  - `uloop` によるコンパイル実行・エラー取得 → 自己修正のループ(コードを書いたら必ずコンパイルを通してから完了報告する)
  - Test Runner 実行、コンソールログの取得・解析
  - execute-dynamic-code によるシーン構築・GameObject 配置・コンポーネント設定
  - screenshot によるエディタ/UI の見た目確認
  - `-batchmode` + `adb` による APK ビルドと実機転送(人間の指示があった時のみ)
- 人間がやる: Unity エディタの起動、パッケージ導入・Project Settings 等の初回セットアップ、Building Blocks の追加、**ヘッドセットを被っての実機確認**(パススルー・ハンドトラッキング・Spatial Anchor の体験検証は AI には不可能)
- 禁止: Building Blocks が生成したオブジェクト/コンポーネントの改変、`.meta` ファイルの手動編集、シーンファイル(.unity)や Prefab の YAML 直接編集(シーン変更は必ず uloop の execute-dynamic-code またはエディタ拡張経由で行う)

## コーディング規約

- C#: Unity 標準規約(PascalCase のメソッド/プロパティ、`[SerializeField] private` でインスペクタ公開)
- 名前空間: `TemaeTrainer.<フォルダ名>`(例: `TemaeTrainer.Sequence`)
- MonoBehaviour は薄く保ち、ロジックは Plain C# クラスに寄せる(将来のテスト容易性のため。ただしテスト整備自体は現時点でスコープ外)
- 非同期は async/await(Awaitable)を使用。コルーチンは新規で書かない
- コメント・ログ・UI 文言は日本語で良い。茶道用語(点前、帛紗、棗 等)はそのまま使う

## 検証方法

- コード変更後は必ず `uloop` でコンパイルを実行し、エラーゼロを確認してから完了とする。エラーが出たら自分で修正して再コンパイルする(人間にエラーを丸投げしない)
- ロジック変更時は関連する Edit Mode テストを `uloop` の run-tests で実行する
- シーンや UI を変更した場合は screenshot で見た目を確認し、結果を報告に添える
- MR 特有の挙動(パススルー・ハンドトラッキング・アンカー)は AI では検証不能。該当する変更では「実機で確認すべきポイント」を箇条書きで人間に引き継ぐこと
- JSON を変更した場合は、バリデータ(`Tools > Temae > Validate Data` に実装予定)を通す

## 現在の状況

- フェーズ: Phase 0(環境構築)。**実機は未購入**。当面は Meta XR Simulator(合成環境)で動作確認する。実機がないと検証できない事項は「実機確認待ちリスト」として `Docs/DEVICE_CHECKLIST.md` に積んでいくこと
- 次のマイルストーン: M0(シミュレータ版)= 合成環境内でピンチにより立方体の色が変わる。実機到着後に M0 実機版を再確認
- Phase 1 の設計判断は SPEC §4, §6.3, §7.1 に確定済み。着手前に必ず読むこと
