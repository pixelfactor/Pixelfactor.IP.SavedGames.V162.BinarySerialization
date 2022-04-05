using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs.ActiveJobs;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class ActiveJobWriter
    {
        public static void Write(BinaryWriter writer, ActiveJob activeJob)
        {
            writer.Write(activeJob.IsDynamicGenerated);
            writer.Write(activeJob.Id);

            if (activeJob.IsDynamicGenerated)
            {
                writer.Write((int)activeJob.JobDataId);
            }

            writer.Write(activeJob.IsActive);
            writer.Write(activeJob.StageIndex);
            writer.Write(activeJob.IsFinished);
            writer.Write(activeJob.CompletionSuccess);
            writer.Write(activeJob.ShowInJournal);
            writer.WriteFactionId(activeJob.OwnerFaction);
            writer.WriteFactionId(activeJob.MissionGiverFaction);
            writer.Write(activeJob.CompletionOpinionChange);
            writer.Write(activeJob.FailureOpinionChange);
            writer.Write(activeJob.StartTime);
            writer.Write(activeJob.RewardCredits);

            for (int i = 0; i < activeJob.Objectives.Count; i++)
            {
                var objective = activeJob.Objectives[i];
                WriteActiveJobObjective(writer, objective);
            }

            switch (activeJob.JobType)
            {
                case JobType.Courier:
                    {
                        var courierMission = (ActiveCourierJob)activeJob;
                        writer.WriteUnitId(courierMission.PickupUnit);
                        writer.WriteUnitId(courierMission.DestinationUnit);
                        writer.Write((int)courierMission.CargoItem.CargoClass);
                        writer.Write(courierMission.CargoItem.Quantity);
                        writer.Write(courierMission.HasPlayerPickedUpCargo);
                    }
                    break;
                case JobType.DestroyFleet:
                    {
                        var destroyUnitsMission = (ActiveDestroyUnitsJob)activeJob;
                        writer.Write(destroyUnitsMission.TargetUnits.Count);
                        foreach (var targetUnit in destroyUnitsMission.TargetUnits)
                        {
                            writer.WriteUnitId(targetUnit);
                        }

                        writer.Write(destroyUnitsMission.HasSetGroupHostileToPlayer);
                        writer.WriteFactionId(destroyUnitsMission.TargetFaction);
                        writer.WriteSectorId(destroyUnitsMission.TargetSector);
                        writer.WriteFleetId(destroyUnitsMission.TargetFleet);
                    }
                    break;
                case JobType.DeliverShip:
                    {
                        var deliverShipMission = (ActiveDeliverShipJob)activeJob;
                        writer.Write((int)deliverShipMission.UnitClass);
                        writer.WriteUnitId(deliverShipMission.DestinationUnit);
                    }
                    break;
                case JobType.Breakdown:
                    {
                        var breakdownMission = (ActiveBreakdownJob)activeJob;
                        writer.WriteUnitId(breakdownMission.BaseUnit);
                        writer.WriteUnitId(breakdownMission.BreakdownUnit);
                    }
                    break;
            }
        }

        public static void WriteActiveJobObjective(BinaryWriter writer, ActiveJobObjective objective)
        {
            writer.Write(objective.IsActive);
            writer.Write(objective.IsComplete);
            writer.Write(objective.Success);
            writer.Write(objective.ShowInJournal);
        }
    }
}
