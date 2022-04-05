using Pixelfactor.IP.SavedGames.V162.Model;
using System.Collections.Generic;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class FleetSpawnParamsReader
    {
        public static FleetSpawnParams Read(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units)
        {
            var spawnParams = new FleetSpawnParams();
            spawnParams.TargetSector = reader.ReadSector(sectors);
            spawnParams.TargetPosition = reader.ReadVector3();
            spawnParams.TargetDockUnit = reader.ReadUnit(units);
            spawnParams.FleetResourceName = reader.ReadString();
            spawnParams.Faction = reader.ReadFaction(factions);
            spawnParams.ShipDesignation = reader.ReadString();
            spawnParams.HomeSector = reader.ReadSector(sectors);
            spawnParams.HomeBaseUnit = reader.ReadUnit(units);

            // Read ships
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var shipParams = ReadFleetSpawnParamsItem(reader);
                spawnParams.Items.Add(shipParams);
            }

            return spawnParams;
        }

        public static FleetSpawnParamsItem ReadFleetSpawnParamsItem(BinaryReader reader)
        {
            var item = new FleetSpawnParamsItem();
            item.UnitClass = (UnitClass)reader.ReadInt32();
            item.PilotResourceName = reader.ReadString();
            item.ShipName = reader.ReadString();
            return item;
        }
    }
}
