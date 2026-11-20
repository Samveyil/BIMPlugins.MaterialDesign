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

print("files with renamed references:", changed)
