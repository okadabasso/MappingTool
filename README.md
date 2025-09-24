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

簡単な使用例
------------
以下は `MapperFactory` / `SimpleMapper` を使った簡単なサンプルです。`SourceData` と `DestinationData` は本リポジトリの `Experimental1/Data` にあるサンプル型を想定しています。

```csharp
// Mapper を作成して単一オブジェクトをマップする
var mapper = MapperFactory.CreateMapper<SourceData, DestinationData>();
var src = new SourceData { Id = 1, Name = "Alice" };
var dst = mapper.Map(src);
Console.WriteLine($"Id={dst.Id}, Name={dst.Name}");

// シーケンスをマップする
var list = new List<SourceData> { src, new SourceData { Id = 2, Name = "Bob" } };
var mapped = mapper.Map(list);
foreach (var item in mapped) Console.WriteLine(item.Name);

// preserveReferences を有効にして参照の保持をテストする場合は
// MapperFactory の作成 API で preserveReferences を有効にしてください（実装により異なります）。
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

EF6 / EF Core 注意事項
---------------------
- トラッキングと参照の保持: EF の ChangeTracker はナビゲーションプロパティの参照を管理します。MapperFactory の `preserveReferences` を使う際、エンティティとそのナビゲーションの参照整合性に注意してください。マッピング時に DB エンティティを直接変更するとトラッキングに影響する可能性があります。
- Lazy-loading / プロキシ: EF Core の Lazy Loading や EF6 のプロキシは型が動的に変わるため、リフレクションや型チェックで想定外の型が渡される場合があります。必要ならマッピング前に `context.Entry(entity).Reference(...).Load()` などで明示的に読み込むか、プロキシを解除して POCO に変換してください。
- シリアライズとナビゲーション: 循環参照を持つナビゲーションをそのままシリアライズするとループします。`preserveReferences` はマッピングレベルで同一参照を保てますが、JSON シリアライズや外部 APIs 向けに出力する際は DTO に切り分けることを推奨します。
- DbContext の寿命: 短命な `DbContext`（1 リクエスト/操作ごと）を推奨します。長寿命のコンテキストを使うと ChangeTracker の情報が拡張し、メモリや参照の扱いに影響します。
- マップ先の更新: エンティティを直接マップ先として使い、そのまま SaveChanges() すると意図しない更新が発生することがあります。変更を明確にするために DTO を経由するか、明示的に EntityState を設定してください。

貢献
----
プルリク歓迎です。変更をローカルでビルドし、テストが通ることを確認してから PR を送ってください。

ライセンス
---------
プロジェクトにライセンスファイルが含まれていない場合、組織の方針に従って適切なライセンスを追加してください。
