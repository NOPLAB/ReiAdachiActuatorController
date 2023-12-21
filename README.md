# ReiAdachiActuatorController

足立レイのためのアクチュエータ制御プログラム

## Architecture

`Program.cs`がエントリーポイント

## Options

- `-s` or `--settings` 設定ファイルのパス
- `-p` or `--port3` Rcb3のシリアルポート
- `-q` or `--port4` Rcb4のシリアルポート
- `-d` or `--decoy` デコイモード(Rcb3,4を使わないで動作確認する)