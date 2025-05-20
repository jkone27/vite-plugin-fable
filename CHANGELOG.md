# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) from version [0.0.37] moving forward.

## [0.0.37] - 2025-05-10
### Changed
- Fable.Compiler updated to 5.0.0-alpha.13 ([#38](https://github.com/fable-compiler/vite-plugin-fable/pull/38))
- Added caret range to fable-library-js ([#38](https://github.com/fable-compiler/vite-plugin-fable/pull/38))
- Updated fable-library-js to 2.0.0-beta.3 ([#38](https://github.com/fable-compiler/vite-plugin-fable/pull/38))

## [0.0.36] - 2025-05-07
### Changed
- Updated fable-library-js to ^2.0.0-beta.3 ([#38](https://github.com/fable-compiler/vite-plugin-fable/pull/38))

## [0.0.35] - 2025-04-30
### Added
- Added README for Fable Daemon ([#34](https://github.com/fable-compiler/vite-plugin-fable/pull/34))

## [0.0.34] - 2025-04-20
### Changed
- Upgrade to latest Fable.Compiler and Fable.AST ([#13](https://github.com/fable-compiler/vite-plugin-fable/pull/13))

## [0.0.33] - 2025-04-10
### Changed
- Update node dependencies and bump version

## [0.0.32] - 2025-03-30
### Fixed
- Fix Thoth.Json usage ([#23](https://github.com/fable-compiler/vite-plugin-fable/pull/23))

## [0.0.31] - 2025-03-15
### Added
- Error overlay for development ([#8](https://github.com/fable-compiler/vite-plugin-fable/pull/8))

## [0.0.30] - 2025-03-01
### Added
- Vite 6 support ([#11](https://github.com/fable-compiler/vite-plugin-fable/pull/11))

## [0.0.29] - 2025-02-20
### Changed
- Improved diagnostics ([#3](https://github.com/fable-compiler/vite-plugin-fable/pull/3))

## [0.0.28] - 2025-02-10
### Added
- Debug viewer ([#5](https://github.com/fable-compiler/vite-plugin-fable/pull/5))

## [0.0.27] - 2025-01-30
### Changed
- Combine file changes for faster rebuilds ([#6](https://github.com/fable-compiler/vite-plugin-fable/pull/6))

## [0.0.26] - 2025-01-15
### Added
- Project options cache ([#2](https://github.com/fable-compiler/vite-plugin-fable/pull/2))

## [0.0.25] - 2025-01-05
### Added
- Support for arm64 architecture in postinstall ([#1](https://github.com/fable-compiler/vite-plugin-fable/pull/1))

## [0.0.24] - 2024-03-02
### Changed
- Improved endpoint call control via shared pending changes subscription.
- Various internal improvements and bug fixes.

## [0.0.22] - 2024-02-28
### Changed
- Update Fable.Compiler to 4.0.0-alpha-008.
- Update TypeScript and include debug folder.

## [0.0.20] - 2024-02-26
### Changed
- Handle F# changes via handleHotUpdate callback.
- Improved file change tracking and project cache key logic.

## [0.0.18] - 2024-02-25
### Changed
- Only send sourceFiles list of FSharpProjectOptions to plugin.
- Additional logging and reuse of CrackerOptions.

## [0.0.16] - 2024-02-24
### Added
- Debug documentation and error overlay prototype.
- Initial debug page setup.

## [0.0.7] - 2024-02-13
### Added
- Diagnostics support ([#3](https://github.com/fable-compiler/vite-plugin-fable/pull/3)).
- Use @fable-org/fable-library-js.

## [0.0.3] - 2024-02-05
### Added
- Thoth.Json support.
- Initial cache key setup for project configuration.
- Initial caching for design time build.

## [0.0.1] - 2023-10-28
### Added
- Initial implementation of Vite plugin for Fable.
- Basic F# file compilation and integration with Vite build.
- Early support for project file watching and hot reload.
- Initial project setup, configuration, and documentation.