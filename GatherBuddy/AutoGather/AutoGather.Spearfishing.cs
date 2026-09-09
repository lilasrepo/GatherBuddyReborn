using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using GatherBuddy.Classes;
using DalamudObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace GatherBuddy.AutoGather
{
    public partial class AutoGather
    {
        private const uint SwimmingShadowsMarkerIconId = 60930;
        private const long SwimmingShadowsMarkerLogCooldown = 5000;

        private long _lastSwimmingShadowsMarkerLog;

        private readonly record struct SpearfishingNodeState(uint RowId, uint BaseRowId, byte RemainingCount);

        internal bool IsSwimmingShadowsAvailable(FishingSpot shadowSpot)
            => shadowSpot.IsShadowNode
            && (HasAvailableSpearfishingNode(shadowSpot) || TryGetSwimmingShadowsMarker(shadowSpot, out _));

        private bool HasAvailableSpearfishingNode(FishingSpot spot)
            => Dalamud.Objects.Any(gameObject => TryGetSpearfishingNodeState(gameObject, out var state)
                && state.RemainingCount > 0
                && MatchesSpearfishingSpot(spot, state));

        // porting-note(api13): FFXIVClientStructs 6966 -- the CS the api13 reference set ships -- has no
        // GatheringType / RowId / BaseRowId / RemainingCount on GatheringPointEventHandler (Cecil-verified:
        // the struct holds only QuickGatheringEnabled / EventObjects / EventSceneModule / Info / IconId /
        // Scene / LuaStatus / UnkString0-1 / GatheringEventHandler / VirtualTable, and the embedded
        // GatheringEventHandler has none of the four either). Upstream builds against a newer CS, and there
        // is no verified TC 7.20 offset for any of them, so the pointer walk is replaced rather than mirrored.
        //
        // Three of the four need no memory at all: the object's BaseId IS the GatheringPoint row -- upstream's
        // own non-spearfishing branch in AutoGather.cs matches exactly that against FishingSpot.WorldPositions
        // keys -- and the sheet yields the GatheringPointBase and the gathering type from there.
        //
        // RUNTIME-VERIFY: RemainingCount is approximated by IsTargetable. A depleted spearfishing node stops
        // being targetable, and every call site already filters on IsTargetable, so the approximation only has
        // to hold for "this node still has fish". If a depleted node is ever observed still targetable, this
        // needs a real remaining-count source.
        private static bool TryGetSpearfishingNodeState(IGameObject gameObject, out SpearfishingNodeState state)
        {
            state = default;
            if (gameObject.ObjectKind != DalamudObjectKind.GatheringPoint || gameObject.Address == nint.Zero)
                return false;

            var rowId = gameObject.DataId;
            if (rowId == 0)
                return false;

            var points = Dalamud.GameData.GetExcelSheet<Lumina.Excel.Sheets.GatheringPoint>();
            if (points == null || !points.TryGetRow(rowId, out var point))
                return false;

            var pointBase = point.GatheringPointBase;
            if (pointBase.RowId == 0 || pointBase.Value.GatheringType.RowId is not (4 or 5))
                return false;

            state = new SpearfishingNodeState(rowId, pointBase.RowId, gameObject.IsTargetable ? (byte)1 : (byte)0);
            return true;
        }

        private static bool MatchesSpearfishingSpot(FishingSpot spot, SpearfishingNodeState state)
        {
            if (state.RowId != 0 && spot.WorldPositions.ContainsKey(state.RowId))
                return true;

            var baseRowId = spot.SpearfishingSpotData?.GatheringPointBase.RowId ?? 0;
            return baseRowId != 0 && state.BaseRowId == baseRowId;
        }

        private bool IsVisitedSpearfishingNode(SpearfishingNodeState state)
            => state.RowId != 0 && VisitedNodes.Contains(state.RowId);

        private unsafe bool TryGetSwimmingShadowsMarker(FishingSpot shadowSpot, out Vector3 position)
        {
            position = default;
            var territoryId = Dalamud.ClientState.TerritoryType;
            if (!shadowSpot.IsShadowNode || shadowSpot.Territory.Id != territoryId)
                return false;

            var agentMap = AgentMap.Instance();
            if (agentMap == null || agentMap->CurrentTerritoryId != territoryId)
                return false;

            var markers = new List<Vector3>(1);
            foreach (var marker in agentMap->MiniMapGatheringMarkers)
            {
                if (marker.ShouldRender == 0
                 || marker.MapMarker.IconId != SwimmingShadowsMarkerIconId
                 || marker.MapMarker.X == 0
                 || marker.MapMarker.Y == 0)
                    continue;

                markers.Add(new Vector3(marker.MapMarker.X / 16f, 0, marker.MapMarker.Y / 16f));
            }

            if (markers.Count != 1)
            {
                if (markers.Count > 1)
                    LogSwimmingShadowsMarkerAmbiguity(shadowSpot);
                return false;
            }

            var markerPosition = markers[0];
            var matches = GatherBuddy.GameData.FishingSpots.Values
                .Where(spot => spot.IsShadowNode && spot.Territory.Id == territoryId)
                .Where(spot => IsMarkerNearSpot(markerPosition, spot))
                .Select(spot => spot.Id)
                .Distinct()
                .ToArray();

            if (matches.Length != 1 || matches[0] != shadowSpot.Id)
            {
                LogSwimmingShadowsMarkerAmbiguity(shadowSpot);
                return false;
            }

            position = markerPosition;
            return true;
        }

        private void LogSwimmingShadowsMarkerAmbiguity(FishingSpot shadowSpot)
        {
            var now = Environment.TickCount64;
            if (now - _lastSwimmingShadowsMarkerLog < SwimmingShadowsMarkerLogCooldown)
                return;

            _lastSwimmingShadowsMarkerLog = now;
            GatherBuddy.Log.Verbose($"Swimming Shadows marker could not be associated uniquely with {shadowSpot.Name}.");
        }

        private static bool IsMarkerNearSpot(Vector3 markerPosition, FishingSpot spot)
        {
            var marker = new Vector2(markerPosition.X, markerPosition.Z);
            var maximumDistanceSquared = NodeVisibilityDistance * NodeVisibilityDistance;
            return spot.WorldPositions.Values
                .SelectMany(positions => positions)
                .Any(position => Vector2.DistanceSquared(marker, new Vector2(position.X, position.Z)) <= maximumDistanceSquared);
        }
    }
}
