# Turns a clean checkout of MaterialDesignInXamlToolkit into BIMPlugins.MaterialDesign:
# - the assemblies get names of their own, so they never share one with another add-in's MaterialDesign
#   (WPF resolves short-name pack URIs by assembly name, and the last loaded copy would take everyone's styles);
# - the pack URIs and assembly= references inside follow the new names;
# - the BIMPlugins theme (additions/) is added as a file of its own;
# - the assemblies are signed with the BIMPlugins key.
# CLR namespaces and the XAML xmlns stay MaterialDesign's, so XAML and code using MaterialDesign need no change.
# Works on bytes, so files keep their encoding. build.ps1 runs it on a clean checkout:
#   python tools/rename.py <checkout> key/BIMPlugins.MaterialDesign.snk
import os
import re
import shutil
import sys

root, key = sys.argv[1], sys.argv[2]
# The repository: additions/ is next to tools/.
here = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
names = {b"MaterialDesignThemes.Wpf": b"BIMPlugins.MaterialDesign",
         b"MaterialDesignColors": b"BIMPlugins.MaterialDesign.Colors"}
themes_dir = os.path.join(root, "src", "MaterialDesignThemes.Wpf")
colors_dir = os.path.join(root, "src", "MaterialDesignColors.Wpf")

changed = 0
for project in (themes_dir, colors_dir):
    for folder, _, files in os.walk(project):
        parts = folder.split(os.sep)
        if "obj" in parts or "bin" in parts:
            continue
        for f in files:
            if not f.endswith((".xaml", ".cs")):
                continue
            path = os.path.join(folder, f)
            with open(path, "rb") as stream:
                data = stream.read()
            new = data
            for old, renamed in names.items():
                # "/MaterialDesignThemes.Wpf;component" and "/MaterialDesignThemes.Wpf;v1.0;key;component"
                new = re.sub(b"/" + re.escape(old) + rb"(;[^;\"'/]*)*;component", b"/" + renamed + b";component", new)
                # xmlns:x="clr-namespace:MaterialDesignColors;assembly=MaterialDesignColors"
                new = re.sub(rb";assembly=" + re.escape(old) + rb"(?=[\"'])", b";assembly=" + renamed, new)
            if new != data:
                changed += 1
                with open(path, "wb") as stream:
                    stream.write(new)


def edit(path, pairs):
    with open(path, "rb") as stream:
        data = stream.read()
    for old, new in pairs:
        if old not in data:
            raise SystemExit("not found in " + path + ": " + old.decode())
        data = data.replace(old, new, 1)
    with open(path, "wb") as stream:
        stream.write(data)


edit(os.path.join(themes_dir, "MaterialDesignThemes.Wpf.csproj"), [
    (b"<UseWPF>true</UseWPF>", b"<UseWPF>true</UseWPF>\r\n    <AssemblyName>BIMPlugins.MaterialDesign</AssemblyName>"),
    (b"MaterialDesignThemes.Wpf.xml", b"BIMPlugins.MaterialDesign.xml"),
])
edit(os.path.join(colors_dir, "MaterialDesignColors.Wpf.csproj"), [
    (b"<AssemblyName>MaterialDesignColors</AssemblyName>", b"<AssemblyName>BIMPlugins.MaterialDesign.Colors</AssemblyName>"),
])

# The tests' InternalsVisibleTo names MaterialDesign's own key; with the BIMPlugins key it would not build.
info = os.path.join(themes_dir, "Properties", "AssemblyInfo.cs")
with open(info, "rb") as stream:
    data = stream.read()
data = re.sub(rb"\[assembly: InternalsVisibleTo\([^\]]*\)\]\s*", b"", data, flags=re.S)
with open(info, "wb") as stream:
    stream.write(data)

shutil.copy(key, os.path.join(root, "build", "bimplugins.snk"))
edit(os.path.join(root, "Directory.Build.props"), [(b"build\\key.snk", b"build\\bimplugins.snk")])

target = os.path.join(themes_dir, "BIMPlugins")
os.makedirs(target, exist_ok=True)
for f in os.listdir(os.path.join(here, "additions")):
    shutil.copy(os.path.join(here, "additions", f), target)


# Compact check boxes and radio buttons, as in MaterialDesign 5.2: since 5.3 their hover halo is laid out 1.6 times the
# box, which pushes the box and its text apart by about 10 px even while the halo is hidden. The halo keeps the box's
# size in the layout and is scaled to 1.6 only when drawn, so it looks the same on hover and takes no room.
def compact_halo(path):
    with open(path, "rb") as stream:
        data = stream.read()
    pattern = re.compile(rb'(<Ellipse x:Name="halo"(?:(?!/>).)*?)ConverterParameter=1\.6\}"((?:(?!/>).)*?)'
                         rb'ConverterParameter=1\.6\}"((?:(?!/>).)*?)RenderTransformOrigin="0\.5,0\.5"\s*/>', re.S)
    new, count = pattern.subn(rb'\1ConverterParameter=1}"\2ConverterParameter=1}"\3RenderTransformOrigin="0.5,0.5">'
                              rb'\r\n              <Ellipse.RenderTransform><ScaleTransform ScaleX="1.6" ScaleY="1.6" /></Ellipse.RenderTransform>'
                              rb'\r\n            </Ellipse>', data)
    if count != 1:
        raise SystemExit("the halo of " + os.path.basename(path) + " has changed: found " + str(count) + ", expected 1")
    with open(path, "wb") as stream:
        stream.write(new)


for name in ("MaterialDesignTheme.CheckBox.xaml", "MaterialDesignTheme.RadioButton.xaml"):
    compact_halo(os.path.join(themes_dir, "Themes", name))


# 5.3 also set the text of check boxes and radio buttons right against the box (Padding 0,2,0,0), the halo's room
# standing in for the gap. With that room gone, the text gets 2 px of its own, as the BIMPlugins add-ins set it.
def text_gap(path):
    with open(path, "rb") as stream:
        data = stream.read()
    pattern = re.compile(rb'(<Trigger Property="HasContent" Value="true">(?:(?!</Trigger>).)*?<Setter Property="Padding" Value=")'
                         rb'0,2,0,0(" />)', re.S)
    new, count = pattern.subn(rb'\g<1>2,2,0,0\2', data)
    if count != 1:
        raise SystemExit("the text padding of " + os.path.basename(path) + " has changed: found " + str(count) + ", expected 1")
    with open(path, "wb") as stream:
        stream.write(new)


for name in ("MaterialDesignTheme.CheckBox.xaml", "MaterialDesignTheme.RadioButton.xaml"):
    text_gap(os.path.join(themes_dir, "Themes", name))

print("files with renamed references:", changed)
