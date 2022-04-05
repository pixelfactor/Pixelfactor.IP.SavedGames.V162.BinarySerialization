using Pixelfactor.IP.SavedGames.V162.Model;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class FleetSpawnerWriter
    {
        public static void Write(BinaryWriter writer, FleetSpawner fleetSpawner)
        {
            writer.WriteStringOrEmpty(fleetSpawner.Name);
            writer.WriteVector3(fleetSpawner.Position);
            writer.WriteVector4(fleetSpawner.Rotation);
            writer.Write(fleetSpawner.InitialSpawnTimeRandomness);
            writer.Write(fleetSpawner.SpawnTimeRandomness);
            writer.WriteStringOrEmpty(fleetSpawner.ShipDesignation);
            writer.WriteStringOrEmpty(fleetSpawner.ShipName);
            writer.WriteStringOrEmpty(fleetSpawner.NamePrefix);
            writer.Write(fleetSpawner.SpawnCounter);
            writer.Write(fleetSpawner.RespawnWhenNoObjectives);
            writer.Write(fleetSpawner.RespawnWhenNoPilots);
            writer.Write(fleetSpawner.AllowRespawnInActiveScene);
            writer.WriteUnitId(fleetSpawner.FleetHomeBase);
            writer.WriteSectorId(fleetSpawner.FleetHomeSector);
            writer.WriteFactionId(fleetSpawner.OwnerFaction);
            writer.WriteSectorId(fleetSpawner.Sector);
            writer.WriteUnitId(fleetSpawner.SpawnDock);
            writer.Write(fleetSpawner.NextSpawnTime);
            writer.Write(fleetSpawner.MinTimeBeforeSpawn);
            writer.Write(fleetSpawner.MaxTimeBeforeSpawn);
            writer.Write(fleetSpawner.MinGroupUnitCount);
            writer.Write(fleetSpawner.MaxGroupUnitCount);
            writer.WriteFleetId(fleetSpawner.SpawnedFleet);

            writer.Write(fleetSpawner.UnitClasses.Count);
            foreach (var unitClassId in fleetSpawner.UnitClasses)
            {
                writer.Write((int)unitClassId);
            }

            writer.Write(fleetSpawner.PilotResourceNames.Count);
            foreach (var pilotResourceName in fleetSpawner.PilotResourceNames)
            {
                writer.WriteStringOrEmpty(pilotResourceName);
            }

            writer.WriteStringOrEmpty(fleetSpawner.FleetResourceName);

            writer.Write(fleetSpawner.Orders.Count);
            foreach (var order in fleetSpawner.Orders)
            {
                FleetOrdersWriter.WriteOrder(writer, order);
            }
        }
    }
}
