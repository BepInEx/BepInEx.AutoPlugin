# BepInEx.AutoPlugin

[![CI](https://github.com/BepInEx/BepInEx.AutoPlugin/workflows/CI/badge.svg)](https://github.com/BepInEx/BepInEx.AutoPlugin/actions)
[![NuGet](https://img.shields.io/endpoint?color=blue&logo=NuGet&label=NuGet&url=https://shields.kzu.io/v/BepInEx.AutoPlugin?feed=nuget.bepinex.dev/v3/index.json)](https://nuget.bepinex.dev/packages/BepInEx.AutoPlugin)

Source generator that turns

```cs
[BepInAutoPlugin("com.example.ExamplePlugin")]
public partial class ExamplePlugin : BaseUnityPlugin
{
}
```

into

```cs
[BepInEx.BepInPlugin(ExamplePlugin.Id, ExamplePlugin.Name, ExamplePlugin.Version)]
public class ExamplePlugin : BaseUnityPlugin
{
    public const string Id = "com.example.ExamplePlugin";
    public const string Name = "ExamplePlugin";
    public const string Version = "0.1.0";
}
```
