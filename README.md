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

  // Added in v1.3.7
  public static Func<Collider, Component> GetCrystalBombExploderCollider;
}

// Add to YourModule.Load()
typeof(CavernHelperImports).ModInterop();

// Example usages

// GetCrystalBombExplosionCollider (this example removes the custom entity if it is caught in the blast range of the explosion)
public void OnExplode(Vector2 position) {
  myEntity.RemoveSelf();
}

Component explosionCollider = GetCrystalBombExplosionCollider?.Invoke(OnExplode, null);
if (explodeCollider != null) {
  myEntity.Add(explosionCollider);
}

// GetCrystalBombExploderCollider (this example causes the crystal bomb to explode if it touches the custom entity)
Component exploderCollider = GetCrystalBombExploderCollider?.Invoke(myEntity.Collider);
if (exploderCollider != null) {
  myEntity.Add(exploderCollider);
}
```
