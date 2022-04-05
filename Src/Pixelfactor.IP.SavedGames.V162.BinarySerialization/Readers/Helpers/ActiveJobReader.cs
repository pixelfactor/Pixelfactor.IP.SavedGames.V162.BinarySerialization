using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs.ActiveJobs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class ActiveJobReader
    {
        public static ActiveJob Read(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets
            )
        {
            var isDynamicGenerated = reader.ReadBoolean();
            var id = reader.ReadInt32();
            var jobDataId = JobDataIds.None;

            if (isDynamicGenerated)
            {
                // For dynamic missions, the mission object is created from a prefab.
                jobDataId = (JobDataIds)reader.ReadInt32();
            }
            else
            {
                // Unfortunately the save file makes it impossible to work out how many objectives the job might have
                throw new NotImplementedException("This library does not support saves created from the built-in scenarios");
            }

            var jobType = GetJobTypeFromJobDataId(jobDataId);
            var job = CreateActiveJobFromJobDataType(jobType);
            job.Id = id;
            job.IsDynamicGenerated = isDynamicGenerated;
            job.JobType = jobType;
            job.JobDataId = jobDataId;
            job.IsActive = reader.ReadBoolean();
            job.StageIndex = reader.ReadInt32();
            job.IsFinished = reader.ReadBoolean();
            job.CompletionSuccess = reader.ReadBoolean();
            job.ShowInJournal = reader.ReadBoolean();
            job.OwnerFaction = reader.ReadFaction(factions);
            job.MissionGiverFaction = reader.ReadFaction(factions);
            job.CompletionOpinionChange = reader.ReadSingle();
            job.FailureOpinionChange = reader.ReadSingle();
            job.StartTime = reader.ReadDouble();
            job.RewardCredits = reader.ReadInt32();

            var objectiveCount = GetJobObjectiveCount(job.JobDataId);
            for (var i = 0; i < objectiveCount; i++)
            {
                job.Objectives.Add(ReadActiveJobObjective(reader));
            }

            switch (job.JobType)
            {
                case JobType.Courier:
                    {
                        var courierMission = (ActiveCourierJob)job;
                        courierMission.PickupUnit = reader.ReadUnit(units);
                        courierMission.DestinationUnit = reader.ReadUnit(units);

                        var item = new ComponentUnitCargoDataItem();
                        item.CargoClassId = reader.ReadInt32();
                        item.Quantity = reader.ReadInt32();
                        courierMission.CargoItem = item;
                        courierMission.HasPlayerPickedUpCargo = reader.ReadBoolean();
                    }
                    break;
                case JobType.DestroyFleet:
                    {
                        var destroyUnitsMission = (ActiveDestroyUnitsJob)job;
                        var unitCount = reader.ReadInt32();
                        for (var i = 0; i < unitCount; i++)
                        {
                            destroyUnitsMission.TargetUnits.Add(reader.ReadUnit(units));
                        }

                        destroyUnitsMission.HasSetGroupHostileToPlayer = reader.ReadBoolean();
                        destroyUnitsMission.TargetFaction = reader.ReadFaction(factions);
                        destroyUnitsMission.TargetSector = reader.ReadSector(sectors);
                        destroyUnitsMission.TargetFleet = reader.ReadFleet(fleets);
                    }
                    break;
                case JobType.DeliverShip:
                    {
                        var deliverShipMission = (ActiveDeliverShipJob)job;
                        deliverShipMission.UnitClass = (UnitClass)reader.ReadInt32();
                        deliverShipMission.DestinationUnit = reader.ReadUnit(units);
                    }
                    break;
                case JobType.Breakdown:
                    {
                        var breakdownMission = (ActiveBreakdownJob)job;
                        breakdownMission.BaseUnit = reader.ReadUnit(units);
                        breakdownMission.BreakdownUnit = reader.ReadUnit(units);
                    }
                    break;
            }

            return job;
        }

        private static ActiveJobObjective ReadActiveJobObjective(
            BinaryReader reader)
        {
            var objective = new ActiveJobObjective();
            objective.IsActive = reader.ReadBoolean();
            objective.IsComplete = reader.ReadBoolean();
            objective.Success = reader.ReadBoolean();
            objective.ShowInJournal = reader.ReadBoolean();
            return objective;
        }

        private static JobType GetJobTypeFromJobDataId(JobDataIds jobDataId)
        {
            switch (jobDataId)
            {
                case JobDataIds.DeliverShip:
                    return JobType.DeliverShip;
                case JobDataIds.Courier:
                    return JobType.Courier;
                case JobDataIds.DestroyFleet:
                    return JobType.DestroyFleet;
                case JobDataIds.Breakdown:
                    return JobType.Breakdown;
                default:
                    throw new NotImplementedException($"Unknown job data id {(int)jobDataId}");
            }
        }

        private static ActiveJob CreateActiveJobFromJobDataType(JobType jobType)
        {
            switch (jobType)
            {
                case JobType.DeliverShip:
                    return new ActiveDeliverShipJob();
                case JobType.Courier:
                    return new ActiveCourierJob();
                case JobType.DestroyFleet:
                    return new ActiveDestroyUnitsJob();
                case JobType.Breakdown:
                    return new ActiveBreakdownJob();
                default:
                    throw new NotImplementedException($"Unknown job type: {(int)jobType}");
            }
        }

        private static int GetJobObjectiveCount(JobDataIds jobDataType)
        {
            switch (jobDataType)
            {
                case JobDataIds.DeliverShip:
                case JobDataIds.DestroyFleet:
                case JobDataIds.DestroyUnits:
                    return 1;
                case JobDataIds.Breakdown:
                case JobDataIds.Courier:
                    return 2;

            }

            return 0;
        }
    }
}
