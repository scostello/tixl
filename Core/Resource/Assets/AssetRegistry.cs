#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using T3.Core.Logging;
using T3.Core.Model;

namespace T3.Core.Resource.Assets;

internal static class AssetRegistry
{
    public static bool TryGetAsset(string address, out Asset? asset) 
    {
        return _assetsByAddress.TryGetValue(address, out asset);
    }
    
    /// <summary>
    /// Scans a package's resource folder and registers all found files.
    /// </summary>
    internal static void RegisterAssetsFromPackage(SymbolPackage package)
    {
        var root = package.ResourcesFolder;
        if (!Directory.Exists(root)) return;

        // Use standard .NET EnumerateFiles for performance
        var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
        var packageId = package.Id;
        var packageAlias = package.Name;

        foreach (var filePath in files)
        {
            var relativePath = Path.GetRelativePath(root, filePath).Replace("\\", "/");
            var uri = $"{packageAlias}:{relativePath}"; // Mandatory format

            var asset = new Asset
                            {
                                Address = uri,
                                PackageId = packageId,
                                FileInfo = new FileInfo(filePath),
                                AssetType = AssetType.Unknown // To be determined by extension
                            };

            _assetsByAddress[uri] = asset;
        }
        
        Log.Debug($"{packageAlias}: Registered {_assetsByAddress.Count(a => a.Value.PackageId == packageId)} assets.");
    }

    public static void UnregisterPackage(Guid packageId)
    {
        var urisToRemove = _assetsByAddress.Values
                                       .Where(a => a.PackageId == packageId)
                                       .Select(a => a.Address)
                                       .ToList();

        foreach (var uri in urisToRemove)
        {
            _assetsByAddress.TryRemove(uri, out _);
            _usagesByAddress.TryRemove(uri, out _);
        }
    }


    // Private fields moved to the end per style guidelines
    private static readonly ConcurrentDictionary<string, Asset> _assetsByAddress = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, List<AssetReference>> _usagesByAddress = new();

}