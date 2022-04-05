using Pixelfactor.IP.SavedGames.V162.Model;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class FleetSpawnerReader
    {
        public static FleetSpawner Read(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<Person> people,
            IEnumerable<SectorPatrolPath> patrolPaths)
        {
            var fleetSpawner = new FleetSpawner();
            fleetSpawner.Name = reader.ReadString();

            fleetSpawner.Position = reader.ReadVector3();
            fleetSpawner.Rotation = reader.ReadVector4();

            fleetSpawner.InitialSpawnTimeRandomness = reader.ReadSingle();
            fleetSpawner.SpawnTimeRandomness = reader.ReadSingle();
            fleetSpawner.ShipDesignation = reader.ReadString();
            fleetSpawner.ShipName = reader.ReadString();
            fleetSpawner.NamePrefix = reader.ReadString();
            fleetSpawner.SpawnCounter = reader.ReadInt32();
            fleetSpawner.RespawnWhenNoObjectives = reader.ReadBoolean();
            fleetSpawner.RespawnWhenNoPilots = reader.ReadBoolean();
            fleetSpawner.AllowRespawnInActiveScene = reader.ReadBoolean();
            fleetSpawner.FleetHomeBase = reader.ReadUnit(units);
            fleetSpawner.FleetHomeSector = reader.ReadSector(sectors);
            fleetSpawner.OwnerFaction = reader.ReadFaction(factions);
            fleetSpawner.Sector = reader.ReadSector(sectors);
            fleetSpawner.SpawnDock = reader.ReadUnit(units);
            fleetSpawner.NextSpawnTime = reader.ReadDouble();
            fleetSpawner.MinTimeBeforeSpawn = reader.ReadSingle();
            fleetSpawner.MaxTimeBeforeSpawn = reader.ReadSingle();
            fleetSpawner.MinGroupUnitCount = reader.ReadInt32();
            fleetSpawner.MaxGroupUnitCount = reader.ReadInt32();
            fleetSpawner.SpawnedFleet = reader.ReadFleet(fleets);

            var unitClassCount = reader.ReadInt32();
            for (var i = 0; i < unitClassCount; i++)
            {
                fleetSpawner.UnitClasses.Add((UnitClass)reader.ReadInt32());
            }

            var pilotCount = reader.ReadInt32();
            for (var i = 0; i < pilotCount; i++)
            {
                fleetSpawner.PilotResourceNames.Add(reader.ReadString());
            }

            fleetSpawner.FleetResourceName = reader.ReadString();

            var orderCount = reader.ReadInt32();
            for (var i = 0; i < orderCount; i++)
            {
                var order = FleetOrdersReader.ReadOrder(
                    reader,
                    factions,
                    sectors,
                    units,
                    fleets,
                    patrolPaths);

                fleetSpawner.Orders.Add(order);
            }

            return fleetSpawner;
        }
    }
}
