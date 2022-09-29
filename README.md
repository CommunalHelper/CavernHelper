# Cavern Helper

A helper for Celeste. Originally created by Exudias and now maintained by the Communal Helper organization.

The release build can be downloaded [here](https://gamebanana.com/mods/53641).

Head to our [issues page](https://github.com/CommunalHelper/CavernHelper/issues) to leave a bug report or feature request.

## API
This mod uses a [ModInterop API](https://github.com/EverestAPI/Resources/wiki/Cross-Mod-Functionality#modinterop), which allows you to import methods into your own mods without referencing Cavern Helper directly. To use it, add Cavern Helper as an [optional dependency](https://github.com/EverestAPI/Resources/wiki/Mod-Structure#optional-dependencies-for-everestyaml-advanced) and use the version where the methods you need were exported (or later).

Check out the [API code](https://github.com/CommunalHelper/CavernHelper/blob/dev/Code/CavernInterop.cs) for documentation on individual methods.

Basic usage guide and version info:
```csharp
// Add somewhere in your mod. You only need to include delegates that you need.
[ModImportName("CavernHelper")]
public static class CavernHelperImports {
  // Added in v1.3.5
  public static Func<Action<Vector2>, Collider, Component> GetCrystalBombExplosionCollider;
}

// Add to YourModule.Load()
typeof(CavernHelperImports).ModInterop();

// Example usages
Action<Vector2> onExplode = (position) => Logger.Log("MyMod", $"We were hit! Bomb exploded at: {position}");
Component explodeCollider = GetCrystalBombExplosionCollider?.Invoke(onExplode, null);
if (explodeCollider != null) {
  myEntity.Add(explodeCollider);
}
```
