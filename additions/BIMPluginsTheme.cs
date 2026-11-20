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
/// Besides MaterialDesign's own brushes, it adds brushes of the BIMPlugins colours, for frames, icons and text of
/// your own: <see cref="PrimaryBrushKey"/>, <see cref="PrimaryTextBrushKey"/> and <see cref="SecondaryBrushKey"/>.
/// They take the colours of the theme, light or dark, so that no add-in writes them by hand:
/// <c>BorderBrush="{StaticResource BIMPlugins.PrimaryBrush}"</c>, or DynamicResource for a window that switches themes.
/// An add-in with colours of its own sets <see cref="PrimaryColor"/> and <see cref="SecondaryColor"/>: MaterialDesign's
/// controls and these brushes then take them together.
/// </remarks>
public class BIMPluginsTheme : ResourceDictionary
{
    /// <summary>The key of the brush in the primary colour: frames, icons, accents.</summary>
    public const string PrimaryBrushKey = "BIMPlugins.PrimaryBrush";

    /// <summary>
    /// The key of the brush for text in the primary colour on the window's background: darker than the primary colour
    /// in the light theme and lighter in the dark one, so that it stays readable.
    /// </summary>
    public const string PrimaryTextBrushKey = "BIMPlugins.PrimaryTextBrush";

    /// <summary>The key of the brush in the secondary colour: secondary actions, links.</summary>
    public const string SecondaryBrushKey = "BIMPlugins.SecondaryBrush";

    private BaseTheme _baseTheme = BaseTheme.Light;
    private Color? _primaryColor;
    private Color? _secondaryColor;

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

    /// <summary>
    /// A primary colour of the add-in's own instead of the BIMPlugins orange, in both themes. MaterialDesign's controls and
    /// <see cref="PrimaryBrushKey"/> take it, and <see cref="PrimaryTextBrushKey"/> is derived from it: darker in the light
    /// theme, lighter in the dark one. Unset, the BIMPlugins colours apply.
    /// </summary>
    public Color? PrimaryColor
    {
        get => _primaryColor;
        set
        {
            if (_primaryColor == value) return;
            _primaryColor = value;
            Apply();
        }
    }

    /// <summary>A secondary colour of the add-in's own instead of the BIMPlugins blue, in both themes.</summary>
    public Color? SecondaryColor
    {
        get => _secondaryColor;
        set
        {
            if (_secondaryColor == value) return;
            _secondaryColor = value;
            Apply();
        }
    }

    private void Apply()
    {
        var dark = _baseTheme == BaseTheme.Dark || _baseTheme == BaseTheme.Inherit && Theme.GetSystemTheme() == BaseTheme.Dark;
        var palette = dark ? BIMPluginsColors.Dark : BIMPluginsColors.Light;

        var primary = _primaryColor ?? palette.Primary;
        var primaryText = _primaryColor is { } own ? Shade(own, dark ? Colors.White : Colors.Black, 0.2) : palette.PrimaryText;
        var secondary = _secondaryColor ?? palette.Secondary;

        var theme = Theme.Create(dark ? BaseTheme.Dark : BaseTheme.Light, primary, secondary);
        theme.Background = palette.Background;
        this.SetTheme(theme);
        this[PrimaryBrushKey] = Frozen(primary);
        this[PrimaryTextBrushKey] = Frozen(primaryText);
        this[SecondaryBrushKey] = Frozen(secondary);

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

    // The colour moved a part of the way to another: to black for text on a light background, to white on a dark one.
    private static Color Shade(Color color, Color to, double amount) => Color.FromRgb(
        (byte)Math.Round(color.R + (to.R - color.R) * amount),
        (byte)Math.Round(color.G + (to.G - color.G) * amount),
        (byte)Math.Round(color.B + (to.B - color.B) * amount));

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
