# BepInEx.AutoPlugin 

Source generator that turns

```cs
[BepInAutoPlugin("com.example.ExamplePlugin")]
public partial class ExamplePlugin : BaseUnityPlugin
{
}
```

into

```cs
[BepInEx.BepInPlugin(ExamplePlugin.Id, "ExamplePlugin", "0.1.0")]
public class ExamplePlugin : BaseUnityPlugin
{
    public const string Id = "com.example.ExamplePlugin";
    public static string Name => "ExamplePlugin";
    public static string Version => "0.1.0";
}
```
