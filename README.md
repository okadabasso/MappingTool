MappingTool
============

小さなオブジェクトマッピング実験リポジトリ。
いくつかのコンソールサンプルとベンチマーク、ユニットテスト、簡易マッパ実装（MapperFactory / SimpleMapper）を含みます。

概要
-----
- MapperFactory: 式ツリーを生成してオブジェクト間のマッピング関数を作成／キャッシュします。
- SimpleMapper: 型安全なラッパー。呼び出し側は SimpleMapper<TSource,TDestination>.Map/Map5 等を使います。
- preserveReferences: 循環参照や同一参照を保つための MappingContext をサポートします（placeholder を用いた安全なライフサイクル）。
- Numeric widening / Enum conversion: 数値の拡張変換や列挙型から文字列・数値への変換をサポートします。

セットアップ & ビルド
-------------------
前提: .NET SDK (推奨: 9.0.x) がインストールされていること。

ビルド:
```powershell
dotnet build MappingTool.sln
```

テスト:
```powershell
dotnet test MappingTool.sln
```

-------------
サンプルの実行
-------------
`Experimental1` と `Experimental2` は実装中の実験コード（experimental/prototype）を格納しています。
これらのプロジェクトは安定版ライブラリではなく、API やファイル配置、動作が頻繁に変わる可能性があります。実運用用途としては扱わないでください。

以下のようにコマンドを実行できます（例: sample3 の method1 を実行）:

```powershell
cd Experimental1
dotnet run -- sample3 method1
```

開発ノート（設計ハイライト）
---------------------------
- MapperFactory は Expression を用いて高速なマッピングコードを動的生成します。生成コードはプロパティやコンストラクタ初期化子、コレクションのマッピングなどを含みます。
- preserveReferences は MappingContext に保存された参照辞書で実現。例外パスでも placeholder が残らないように try/finally と replaced フラグで安全にクリーンアップします。
- 型変換: 整数の拡張（short -> int など）や enum と数値/文字列間の変換ロジックを生成に含めています。

よくあるトラブルシューティング
-----------------------------
- "duplicate top-level statements" など SDK/プロジェクトの glob 取り込みの問題が出る場合、プロジェクトの `csproj` の `Compile` include を確認してください。
- Console サンプルは `ConsoleAppFramework` に依存する箇所があります。もし参照エラーが出る場合は `Experimental1/Experimental1.csproj` のパッケージ参照を確認してください。

貢献
----
プルリク歓迎です。変更をローカルでビルドし、テストが通ることを確認してから PR を送ってください。

ライセンス
---------
プロジェクトにライセンスファイルが含まれていない場合、組織の方針に従って適切なライセンスを追加してください。
