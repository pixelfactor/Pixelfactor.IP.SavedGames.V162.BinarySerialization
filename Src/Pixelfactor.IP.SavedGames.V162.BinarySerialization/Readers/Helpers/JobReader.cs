using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs.JobTypes;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class JobReader
    {
        public static Job Read(
            BinaryReader reader,
            JobType jobType,
            IEnumerable<Sector> sectors,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var job = CreateJobFromJobType.CreateJob(jobType);
            job.Id = reader.ReadInt32();
            job.JobType = jobType;

            // TODO: Validate
            var unitId = reader.ReadInt32();
            job.Unit = units.FirstOrDefault(e => e.Id == unitId);
            job.JobDataResourceId = reader.ReadInt32();

            // TODO: Validate
            var factionId = reader.ReadInt32();
            job.Faction = factions.FirstOrDefault(e => e.Id == factionId);

            job.ExpiryTime = reader.ReadDouble();
            job.RewardCredits = reader.ReadInt32();
            job.ProfitCredits = reader.ReadInt32();

            switch (job.JobType)
            {
                case JobType.Courier:
                    {
                        var m = (CourierJob)job;

                        // TODO: Validate
                        var pickupUnitId = reader.ReadInt32();
                        m.PickupUnit = units.FirstOrDefault(e => e.Id == pickupUnitId);

                        var destinationUnitId = reader.ReadInt32();
                        m.DestinationUnit = units.FirstOrDefault(e => e.Id == destinationUnitId);

                        m.Cargo = ComponentUnitCargoDataItemReader.Read(reader);
                    }
                    break;
                case JobType.DeliverShip:
                    {
                        var m = (DeliverShipJob)job;

                        // TODO: Validate
                        m.UnitClass = (UnitClass)reader.ReadInt32();

                        var destinationUnitId = reader.ReadInt32();
                        m.DestinationUnit = units.FirstOrDefault(e => e.Id == destinationUnitId);
                    }
                    break;
                case JobType.Breakdown:
                    {
                        var m = (BreakdownJob)job;
                        m.BreakdownUnitClass = (UnitClass)reader.ReadInt32();

                        var sectorId = reader.ReadInt32();
                        m.BreakdownDestinationSector = sectors.FirstOrDefault(e => e.Id == sectorId);
                        m.BreakdownDestinationPosition = reader.ReadVector3();
                    }
                    break;
                case JobType.DestroyFleet:
                    {
                        var m = (DestroyFleetJob)job;
                        m.FleetSpawnParams = FleetSpawnParamsReader.Read(reader, factions, sectors, units);
                    }
                    break;
            }

            return job;
        }
    }
}
