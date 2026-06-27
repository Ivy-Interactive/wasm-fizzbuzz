# Ivy.Tendril.Plugin.Doom

DOOM (1993) running in WebAssembly as an Ivy Tendril plugin.

## Features

- Dynamic WAD loading — upload and switch between WAD files at runtime
- Supports doom1.wad (shareware), doom.wad (registered), doom2.wad, plutonia.wad, tnt.wad
- Pause/play/restart controls
- Game state events piped to C#: `OnStateChanged`, `OnWeaponFired`, `OnShotLanded`, `OnEnemyKilled`
- Optional annoying popup mode (disabled by default)

## Installation

Add the plugin by extracting the Nuget package into your plugins folder, or clone this repo and point your `plugin-references.yaml` to `doom/plugin`.

### WAD files

The plugin does not ship with WAD data. Place `.wad` files in `{TendrilHome}/doom-wads/` or upload them through the UI.

## Building from source

### Prerequisites

- .NET 10 SDK
- Node.js + pnpm (for frontend build)
- `vp` (vite-plus) CLI

### Build

```sh
cd doom/plugin
dotnet build
```

### Rebuilding doom.wasm

Requires Homebrew LLVM (macOS) or clang with wasm32 target + `wasm-ld`:

```sh
brew install llvm lld
export CC=/opt/homebrew/opt/llvm/bin/clang
export AR=/opt/homebrew/opt/llvm/bin/llvm-ar
export RANLIB=/opt/homebrew/opt/llvm/bin/llvm-ranlib
export PATH="/opt/homebrew/bin:/opt/homebrew/opt/llvm/bin:$PATH"

cd doom
cargo build --release
cp target/wasm32-unknown-unknown/release/doom.wasm plugin/frontend/public/
```

## Publishing

```sh
NUGET_KEY=... ./publish.sh 1.0.0
```

## Configuration

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| AnnoyingPopups | boolean | false | Enable popups that interrupt gameplay at the worst moments |

## License

Plugin code is MIT. See THIRD-PARTY-LICENSES for bundled third-party component licenses (DOOM engine, musl, compiler-rt).