using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Islebound.World
{
    public static class BiomeUtility
    {
        public static BiomeZone[] GetAllBiomeZones()
        {
            return Object.FindObjectsByType<BiomeZone>(FindObjectsSortMode.None);
        }

        public static List<BiomeZone> GetBiomeZonesByTypes(BiomeType[] biomeTypes)
        {
            List<BiomeZone> result = new();
            if (biomeTypes == null || biomeTypes.Length == 0)
                return result;

            BiomeZone[] allZones = GetAllBiomeZones();

            foreach (BiomeZone zone in allZones)
            {
                if (zone == null)
                    continue;

                for (int i = 0; i < biomeTypes.Length; i++)
                {
                    if (zone.BiomeType == biomeTypes[i])
                    {
                        result.Add(zone);
                        break;
                    }
                }
            }

            return result;
        }

        public static BiomeZone GetNearestAllowedBiome(Vector3 position, BiomeType[] biomeTypes)
        {
            List<BiomeZone> zones = GetBiomeZonesByTypes(biomeTypes);

            BiomeZone best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] == null)
                    continue;

                Bounds bounds = zones[i].GetBounds();
                Vector3 center = bounds.center;
                center.y = position.y;

                float sqrDistance = (center - position).sqrMagnitude;
                if (sqrDistance < bestDistance)
                {
                    bestDistance = sqrDistance;
                    best = zones[i];
                }
            }

            return best;
        }

        public static bool IsPointInsideAnyAllowedBiome(Vector3 point, BiomeType[] biomeTypes)
        {
            List<BiomeZone> zones = GetBiomeZonesByTypes(biomeTypes);

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null && zones[i].ContainsPoint(point))
                    return true;
            }

            return false;
        }

        public static bool TryGetRandomNavMeshPointInBiome(
            BiomeZone biomeZone,
            float navMeshSampleRadius,
            int attempts,
            out Vector3 result)
        {
            result = Vector3.zero;

            if (biomeZone == null)
                return false;

            for (int i = 0; i < attempts; i++)
            {
                Vector3 candidate = biomeZone.GetRandomPointInsideBounds(2f);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
                {
                    if (biomeZone.ContainsPoint(hit.position))
                    {
                        result = hit.position;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryGetRandomNavMeshPointInAllowedBiomes(
            BiomeType[] biomeTypes,
            float navMeshSampleRadius,
            int biomePickAttempts,
            int sampleAttemptsPerBiome,
            out Vector3 result,
            out BiomeZone chosenBiome)
        {
            result = Vector3.zero;
            chosenBiome = null;

            List<BiomeZone> possibleBiomes = GetBiomeZonesByTypes(biomeTypes);
            if (possibleBiomes.Count == 0)
                return false;

            for (int i = 0; i < biomePickAttempts; i++)
            {
                BiomeZone biome = possibleBiomes[Random.Range(0, possibleBiomes.Count)];

                if (TryGetRandomNavMeshPointInBiome(
                    biome,
                    navMeshSampleRadius,
                    sampleAttemptsPerBiome,
                    out Vector3 point))
                {
                    result = point;
                    chosenBiome = biome;
                    return true;
                }
            }

            return false;
        }
    }
}