using Pixelfactor.IP.SavedGames.V162.Model;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class SectorTargetWriter
    {
        public static void Write(BinaryWriter writer, SectorTarget sectorTarget)
        {
            writer.WriteVector3(sectorTarget.Position);
            writer.WriteSectorId(sectorTarget.Sector);
            writer.WriteUnitId(sectorTarget.TargetUnit);
            writer.WriteFleetId(sectorTarget.TargetFleet);
            writer.Write(sectorTarget.HadValidTarget);
        }
    }
}
