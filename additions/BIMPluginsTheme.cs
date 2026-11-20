using System.Windows.Media;
using MaterialDesignThemes.Wpf;

// In XAML BIMPluginsTheme takes the usual MaterialDesign prefix: <materialDesign:BIMPluginsTheme BaseTheme="Dark" />.
[assembly: XmlnsDefinition("http://materialdesigninxaml.net/winfx/xaml/themes", "BIMPlugins.MaterialDesign")]

namespace BIMPlugins.MaterialDesign;

/// <summary>
/// The look of BIMPlugins add-ins in one line: the MaterialDesign 3 styles in the BIMPlugins colours, light or dark.
/// </summary>
/// <example>
/// <code>
/// &lt;Window.Resources&gt;
///     &lt;materialDesign:BIMPluginsTheme BaseTheme="Light" /&gt;
/// &lt;/Window.Resources&gt;
/// </code>
/// </example>
/// <remarks>
/// Besides MaterialDesign's own brushes, it adds <see cref="PrimaryTextBrushKey"/>: the primary colour for text on the
/// window's background, darker in the light theme and lighter in the dark one, so that it stays readable.
/// </remarks>
public class BIMPluginsTheme : ResourceDictionary
{
    /// <summary>The key of the brush for text in the primary colour on the window's background.</summary>
    public const string PrimaryTextBrushKey = "BIMPlugins.PrimaryTextBrush";

    private BaseTheme _baseTheme = BaseTheme.Light;

    public BIMPluginsTheme()
    {
        Apply();
    }

    /// <summary>Light or dark. <see cref="MaterialDesignThemes.Wpf.BaseTheme.Inherit"/> follows the Windows app theme.</summary>
    public BaseTheme BaseTheme
    {
        get => _baseTheme;
        set
        {
            if (_baseTheme == value) return;
            _baseTheme = value;
            Apply();
        }
    }

    private void Apply()
    {
        var dark = _baseTheme == BaseTheme.Dark || _baseTheme == BaseTheme.Inherit && Theme.GetSystemTheme() == BaseTheme.Dark;
        var palette = dark ? BIMPluginsColors.Dark : BIMPluginsColors.Light;

        var theme = Theme.Create(dark ? BaseTheme.Dark : BaseTheme.Light, palette.Primary, palette.Secondary);
        theme.Background = palette.Background;
        this.SetTheme(theme);
        this[PrimaryTextBrushKey] = Frozen(palette.PrimaryText);

        if (MergedDictionaries.Count == 0)
        {
            // MaterialDesign 3 styles of this very assembly, named in full so that no other MaterialDesign answers for it.
            var name = typeof(BIMPluginsTheme).Assembly.GetName().Name;
            MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/" + name + ";component/Themes/MaterialDesign3.Defaults.xaml")
            });
        }
    }

    private static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}

/// <summary>The BIMPlugins colours, for code that needs them outside the theme.</summary>
public static class BIMPluginsColors
{
    /// <summary>Orange #F57C00 and blue #1565C0 on a light background.</summary>
    public static readonly Palette Light = new(
        Primary: Color.FromRgb(0xF5, 0x7C, 0x00),
        PrimaryText: Color.FromRgb(0xC2, 0x5E, 0x00),
        Secondary: Color.FromRgb(0x15, 0x65, 0xC0),
        Background: Color.FromRgb(0xFA, 0xFA, 0xFA));

    /// <summary>Lighter orange #FFA040 and blue #64B5F6 on a dark, slightly cool background, close to Revit's dark theme.</summary>
    public static readonly Palette Dark = new(
        Primary: Color.FromRgb(0xFF, 0xA0, 0x40),
        PrimaryText: Color.FromRgb(0xFF, 0xB3, 0x66),
        Secondary: Color.FromRgb(0x64, 0xB5, 0xF6),
        Background: Color.FromRgb(0x1F, 0x23, 0x29));

    /// <summary>The colours of one theme.</summary>
    /// <param name="Primary">Buttons, frames, checkboxes.</param>
    /// <param name="PrimaryText">Text in the primary colour on the background.</param>
    /// <param name="Secondary">Secondary actions, links, toggles.</param>
    /// <param name="Background">The window's background.</param>
    public sealed record Palette(Color Primary, Color PrimaryText, Color Secondary, Color Background);
}
