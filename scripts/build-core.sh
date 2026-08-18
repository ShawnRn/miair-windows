#!/usr/bin/env bash
set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CORE_DIR="$DIR/core"
BIN_DIR="$CORE_DIR/bin"

VERSION=$(cat "$DIR/core/VERSION" 2>/dev/null || echo "1.1.2")

echo "=== Building MiAir Core for Windows (Dual-Architecture) ==="
mkdir -p "$BIN_DIR/win-arm64" "$BIN_DIR/win-x64"

echo "-> Building windows/arm64 (Windows on ARM / Parallels)..."
cd "$CORE_DIR"
CGO_ENABLED=0 GOOS=windows GOARCH=arm64 go build -trimpath -ldflags="-s -w -X main.version=$VERSION" -o "$BIN_DIR/win-arm64/miair-core.exe" .

echo "-> Building windows/amd64 (x64 Windows)..."
CGO_ENABLED=0 GOOS=windows GOARCH=amd64 go build -trimpath -ldflags="-s -w -X main.version=$VERSION" -o "$BIN_DIR/win-x64/miair-core.exe" .

echo "=== Go Core binaries built successfully: ==="
ls -lh "$BIN_DIR/win-arm64/miair-core.exe" "$BIN_DIR/win-x64/miair-core.exe"
