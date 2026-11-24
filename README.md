# BIMPlugins.MaterialDesign

[![Revit 2019+](https://img.shields.io/badge/Revit-2019%2B-blue.svg)](https://github.com/Samveyil/BIMPlugins.MaterialDesign)
[![MaterialDesign 5.3.2](https://img.shields.io/badge/MaterialDesign-5.3.2-blue.svg)](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/releases/tag/v5.3.2)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Samveyil/BIMPlugins.MaterialDesign/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/BIMPlugins.MaterialDesign.svg)](https://www.nuget.org/packages/BIMPlugins.MaterialDesign)

[Material Design in XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) for Revit add-ins,
under assembly names of its own, plus the BIMPlugins theme.

## Why

Every add-in runs in the one Revit process. Two add-ins with different versions of MaterialDesign load side by side,
but WPF finds styles by assembly name (`pack://application:,,,/MaterialDesignThemes.Wpf;component/...`), and the copy
loaded last answers for everyone. So one add-in's windows lose their look, or fail with "Cannot locate resource",
as soon as another add-in loads its MaterialDesign.

This package is MaterialDesign built as `BIMPlugins.MaterialDesign` and `BIMPlugins.MaterialDesign.Colors`. No other
add-in has assemblies with these names, so nothing is shared: your windows keep their styles, and other add-ins keep theirs.

Namespaces and XAML are MaterialDesign's, so code and XAML written for MaterialDesign work as they are.

## Installation

```xml
<PackageReference Include="BIMPlugins.MaterialDesign" Version="5.3.2.2" />
```

Replace `MaterialDesignThemes` and `MaterialDesignColors` with it. The package targets `net462` (Revit 2019–2024)
and `net8.0-windows` (Revit 2025+).

## Usage

The BIMPlugins look in one line, light or dark:

```xml
<Window.Resources>
    <materialDesign:BIMPluginsTheme BaseTheme="Light" />
</Window.Resources>
```

with `xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"`. It brings MaterialDesign 3 styles in the
BIMPlugins colours:

| | Light | Dark |
|---|---|---|
| Primary (`BIMPlugins.PrimaryBrush`) | `#F57C00` | `#FFA040` |
| Primary text on the background (`BIMPlugins.PrimaryTextBrush`) | `#C25E00` | `#FFB366` |
| Secondary (`BIMPlugins.SecondaryBrush`) | `#1565C0` | `#64B5F6` |
| Background | `#FAFAFA` | `#1F2329` |

The brushes take the theme's colours, so frames and text of your own need no colour written by hand:
`BorderBrush="{StaticResource BIMPlugins.PrimaryBrush}"` (or `DynamicResource` in a window that switches themes).

An add-in with colours of its own sets them on the theme, and MaterialDesign's controls and these brushes take them
together; the primary text colour is derived, darker in the light theme and lighter in the dark one:

```xml
<materialDesign:BIMPluginsTheme BaseTheme="Light" PrimaryColor="#2E7D32" SecondaryColor="#6A1B9A" />
```

`BaseTheme="Inherit"` follows the Windows app theme. In code the colours are in `BIMPluginsColors.Light` and `BIMPluginsColors.Dark`.

Your own MaterialDesign theme works too, as with MaterialDesign itself. If you merge MaterialDesign dictionaries by URI,
use the new assembly name: `pack://application:,,,/BIMPlugins.MaterialDesign;component/Themes/MaterialDesign3.Defaults.xaml`.

## How it is built

The repository keeps only what BIMPlugins adds. `build.ps1` clones the MaterialDesign release, and `tools/rename.py`
renames the assemblies and the pack URIs that name them, adds `additions/` and signs the assemblies with
`key/BIMPlugins.MaterialDesign.snk`. Then it builds and packs:

```powershell
./build.ps1    # artifacts/BIMPlugins.MaterialDesign.<version>.nupkg
```

To move to another MaterialDesign release, change `$MaterialDesignVersion` and `$PackageVersion` in `build.ps1`.
A `v<version>` tag publishes the package to NuGet.

## License

MIT, as MaterialDesign in XAML Toolkit. See [LICENSE.txt](LICENSE.txt).
