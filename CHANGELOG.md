# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).
The package version is the MaterialDesign in XAML Toolkit release it is built from, with a fourth number
for BIMPlugins' own changes to the same release.

## [5.3.2.1] / 2026-11-20
### Added
- `BIMPlugins.PrimaryBrush` and `BIMPlugins.SecondaryBrush` in `BIMPluginsTheme`, next to `BIMPlugins.PrimaryTextBrush`:
  the theme's colours, light or dark, as resources, so that add-ins do not write them by hand.
- `PrimaryColor` and `SecondaryColor` on `BIMPluginsTheme`: colours of the add-in's own instead of the BIMPlugins ones,
  taken by MaterialDesign's controls and the BIMPlugins brushes together.

### Changed
- Check boxes and radio buttons are compact again, as in MaterialDesign 5.2: their hover halo no longer takes room
  in the layout and pushes the box and its text apart; it looks the same on hover. Their text stands 2 px
  from the box by default.

## [5.3.2] / 2026-11-20
### Added
- Material Design in XAML Toolkit 5.3.2 as `BIMPlugins.MaterialDesign` and `BIMPlugins.MaterialDesign.Colors`,
  signed with the BIMPlugins key, for .NET Framework 4.6.2 and .NET 8.
- `BIMPluginsTheme`: MaterialDesign 3 styles in the BIMPlugins colours, light and dark, and `BIMPluginsColors`.
