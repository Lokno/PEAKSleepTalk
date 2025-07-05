# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Patch to keep mouth animating when a player is passed out
- Patch to continue to allow voice when a player is passed out but at a lower volume

## [0.1.0] - 2025-07-04

- 8 second cooldown before player can speak
- Initial Thunderstore package release

## [0.1.1] - 2025-07-04

- fixed a logical oversight that affected speaking when dead

## [0.2.0] - 2025-07-04

- added configuration file to tweak values
- added option to turn off volume deduction in settings
- added option to turn off quiet time in settings
- simplified code when accessing public members

## [0.2.1] - 2025-07-05

- fix for mistake in 0.2.0 which broke the voice activation logic