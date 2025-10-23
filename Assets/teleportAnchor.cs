using System.Collections.Generic;
using UnityEngine;

public class teleportAnchor : MonoBehaviour
{
    [Tooltip("Link this to the TeleportLocationAsset that identifies this spot.")]
    public TeleportLocationAsset asset;

    // Simple in-memory registry (scene-lifetime)
    private static readonly Dictionary<TeleportLocationAsset, teleportAnchor> Registry
        = new Dictionary<TeleportLocationAsset, teleportAnchor>();

    private TeleportLocation location; // sibling component

    void Awake()
    {
        // Ensure we can always compute a position
        location = GetComponent<TeleportLocation>();
        if (location == null) location = gameObject.AddComponent<TeleportLocation>();
    }

    void OnEnable()
    {

        if (asset != null)
        {
            Registry[asset] = this;
            Debug.Log($"[TeleportAnchor] Registered '{asset.name}' at {transform.position}");
        }
        else
        {
            Debug.LogWarning($"[TeleportAnchor] No asset assigned on {gameObject.name}.");
        }
    }

    void OnDisable()
    {
        if (asset != null && Registry.TryGetValue(asset, out var a) && a == this)
            Registry.Remove(asset);
    }

    // Lookup API (VN will use this)
    public static bool TryGet(TeleportLocationAsset key, out teleportAnchor anchor)
        => Registry.TryGetValue(key, out anchor);

    public Vector3 GetPosition() => location != null ? location.GetPosition() : transform.position;

    public static void DebugDumpRegistry()
    {
        if (Registry.Count == 0)
        {
            Debug.Log("[TeleportAnchor] Registry is EMPTY.");
            return;
        }

        foreach (var kv in Registry)
        {
            var asset = kv.Key ? kv.Key.name : "(null)";
            var anchor = kv.Value ? kv.Value.name : "(null)";
            var pos = kv.Value ? kv.Value.transform.position.ToString("F2") : "(null)";
            Debug.Log($"[TeleportAnchor] {asset} -> {anchor} at {pos}");
        }
    }
}