using Pixelfactor.IP.SavedGames.V162.Model;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class SectorTargetReader
    {
        public static SectorTarget Read(BinaryReader reader, IEnumerable<Sector> sectors, IEnumerable<Unit> units, IEnumerable<Fleet> fleets)
        {
            var sectorTarget = new SectorTarget();
            sectorTarget.Position = reader.ReadVector3();

            var sectorId = reader.ReadInt32();
            var targetUnitId = reader.ReadInt32();
            var targetFleetId = reader.ReadInt32();

            sectorTarget.Sector = sectors.FirstOrDefault(e => e.Id == sectorId);
            sectorTarget.TargetUnit = units.FirstOrDefault(e => e.Id == targetUnitId);
            sectorTarget.TargetFleet = fleets.FirstOrDefault(e => e.Id == targetFleetId);
            sectorTarget.HadValidTarget = reader.ReadBoolean();
            return sectorTarget;
        }
    }
}
