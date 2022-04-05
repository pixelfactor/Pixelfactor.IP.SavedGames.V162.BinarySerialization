using Pixelfactor.IP.SavedGames.V162.Model;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class FleetSpawnParamsWriter
    {
        public static void Write(BinaryWriter writer, FleetSpawnParams spawnParams)
        {
            writer.WriteSectorId(spawnParams.TargetSector);
            writer.WriteVector3(spawnParams.TargetPosition);
            writer.WriteUnitId(spawnParams.TargetDockUnit);
            writer.WriteStringOrEmpty(spawnParams.FleetResourceName);
            writer.WriteFactionId(spawnParams.Faction);
            writer.WriteStringOrEmpty(spawnParams.ShipDesignation);
            writer.WriteSectorId(spawnParams.HomeSector);
            writer.WriteUnitId(spawnParams.HomeBaseUnit);

            writer.Write(spawnParams.Items.Count);
            foreach (var item in spawnParams.Items)
            {
                WriteFleetSpawnParamsItem(writer, item);
            }
        }

        private static void WriteFleetSpawnParamsItem(BinaryWriter writer, FleetSpawnParamsItem item)
        {
            writer.Write((int)item.UnitClass);
            writer.WriteStringOrEmpty(item.PilotResourceName);
            writer.WriteStringOrEmpty(item.ShipName);
        }
    }
}
