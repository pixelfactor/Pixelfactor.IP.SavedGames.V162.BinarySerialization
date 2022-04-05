using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs.JobTypes;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class JobWriter
    {
        public static void Write(BinaryWriter writer, Job job)
        {
            writer.Write(job.Id);
            writer.WriteUnitId(job.Unit);
            writer.Write(job.JobDataResourceId);
            writer.WriteFactionId(job.Faction);
            writer.Write(job.ExpiryTime);
            writer.Write(job.RewardCredits);
            writer.Write(job.ProfitCredits);

            switch (job.JobType)
            {
                case JobType.Courier:
                    {
                        var m = (CourierJob)job;
                        writer.WriteUnitId(m.PickupUnit);
                        writer.WriteUnitId(m.DestinationUnit);
                        ComponentUnitCargoDataItemWriter.Write(writer, m.Cargo);
                    }
                    break;
                case JobType.DeliverShip:
                    {
                        var m = (DeliverShipJob)job;
                        writer.Write((int)m.UnitClass);
                        writer.WriteUnitId(m.DestinationUnit);
                    }
                    break;
                case JobType.Breakdown:
                    {
                        var m = (BreakdownJob)job;
                        writer.Write((int)m.BreakdownUnitClass);
                        writer.WriteSectorId(m.BreakdownDestinationSector);
                        writer.WriteVector3(m.BreakdownDestinationPosition);
                    }
                    break;
                case JobType.DestroyFleet:
                    {
                        var m = (DestroyFleetJob)job;
                        FleetSpawnParamsWriter.Write(writer, m.FleetSpawnParams);
                    }
                    break;
            }
        }
    }
}
