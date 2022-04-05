using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.ActiveOrderTypes;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.ActiveOrderTypes.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class ActiveFleetOrderReader
    {
        public static ActiveFleetOrder Read(
            BinaryReader reader,
            FleetOrderType fleetOrderType,
            int fleetId,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<SectorPatrolPath> patrolPaths,
            IEnumerable<Person> people)
        {
            var activeFleetOrder = CreateActiveFleetOrderFromType.Create(fleetOrderType);
            activeFleetOrder.TimeoutTime = reader.ReadDouble();
            activeFleetOrder.StartTime = reader.ReadDouble();

            switch (fleetOrderType)
            {
                case FleetOrderType.AttackFleet:
                    {
                        var a = (ActiveAttackFleetOrder)activeFleetOrder;
                        var targetFleetId = reader.ReadInt32();

                        // TODO: Validate
                        a.TargetFleet = fleets.FirstOrDefault(e => e.Id == targetFleetId);
                    }
                    break;
                case FleetOrderType.AttackTarget:
                    {
                        var a = (ActiveAttackTargetOrder)activeFleetOrder;

                        // TODO: Validate
                        var targetUnitId = reader.ReadInt32();
                        a.TargetUnit = units.FirstOrDefault(e => e.Id == targetUnitId);
                    }
                    break;
                case FleetOrderType.UniverseBountyHunter:
                    {
                        var a = (ActiveUniverseBountyHunterOrder)activeFleetOrder;

                        // TOOD: Validate
                        var targetPersonId = reader.ReadInt32();

                        // NOTE: Bug with the 1.6.x game version file reader - 'People' have not been populated yet so the following does nothing
                        // Bounty hunters forced to find a new target after loading save
                        a.TargetPerson = people.FirstOrDefault(e => e.Id == targetPersonId);
                    }
                    break;
                case FleetOrderType.UniverseRoam:
                    {
                        var a = (ActiveUniverseRoamOrder)activeFleetOrder;

                        // TODO: Validate
                        var targetSectorId = reader.ReadInt32();
                        a.CurrentTargetSector = sectors.FirstOrDefault(e => e.Id == targetSectorId);
                        a.CurrentTargetPosition = reader.ReadVector3();
                    }
                    break;
                case FleetOrderType.Explore:
                    {
                        var a = (ActiveExploreOrder)activeFleetOrder;

                        // TODO: Validate
                        var targetSectorId = reader.ReadInt32();
                        a.CurrentTargetSector = sectors.FirstOrDefault(e => e.Id == targetSectorId);
                        a.CurrentTargetPosition = reader.ReadVector3();
                    }
                    break;
                case FleetOrderType.Trade:
                case FleetOrderType.UniverseTrade:
                case FleetOrderType.ManualTrade:
                    {
                        var a = (ActiveTradeOrder)activeFleetOrder;
                        var hasTradeRoute = reader.ReadBoolean();
                        if (hasTradeRoute)
                        {
                            a.TradeRoute = CustomTradeRouteReader.Read(reader, units);
                        }

                        a.EndBuySellTime = reader.ReadDouble();
                        a.LastStateChangeTime = reader.ReadDouble();

                        var state = (ActiveTradeOrderState)reader.ReadInt32();

                        if (a.TradeRoute != null)
                        {
                            a.CurrentState = state;
                        }
                    }
                    break;
                case FleetOrderType.UniversePassengerTransport:
                    {
                        var a = (ActiveUniversePassengerTransportOrder)activeFleetOrder;

                        // TODO: Validate passenger group
                        var passengerGroupId = reader.ReadInt32();
                        var passengerGroup = units.SelectMany(e => e.PassengerGroups).FirstOrDefault(e => e.Id == passengerGroupId);

                        a.PassengerGroup = passengerGroup;
                        a.EndBuySellTime = reader.ReadDouble();
                        a.LastStateChangeTime = reader.ReadDouble();

                        var state = (ActiveTransportPassengerOrderState)reader.ReadInt32();

                        if (a.PassengerGroup != null)
                        {
                            a.CurrentState = state;
                        }
                    }
                    break;
                case FleetOrderType.CollectCargo:
                case FleetOrderType.Mine:
                    {
                        var a = (ActiveCollectCargoOrder)activeFleetOrder;

                        // TODO: Validate
                        var tractorTargetId = reader.ReadInt32();
                        a.TractorTargetUnit = units.FirstOrDefault(e => e.Id == tractorTargetId);

                        a.AutoFindCargoEnabled = reader.ReadBoolean();
                        a.AutoTractorCargoEnabled = reader.ReadBoolean();
                    }
                    break;
                case FleetOrderType.Scavenge:
                    {
                        var a = (ActiveScavengeOrder)activeFleetOrder;

                        // TODO: Validate
                        var tractorTargetId = reader.ReadInt32();
                        a.TractorTargetUnit = units.FirstOrDefault(e => e.Id == tractorTargetId);

                        a.AutoFindCargoEnabled = reader.ReadBoolean();
                        a.AutoTractorCargoEnabled = reader.ReadBoolean();

                        a.IsRoaming = reader.ReadBoolean();
                        a.RoamExpireTime = reader.ReadDouble();
                        a.LastKnownCargoPosition = reader.ReadVector3();
                        a.HadCargoTarget = reader.ReadBoolean();
                    }
                    break;
                case FleetOrderType.Dock:
                case FleetOrderType.DisposeCargo:
                case FleetOrderType.JoinFleet:
                case FleetOrderType.MoveTo:
                case FleetOrderType.Protect:
                case FleetOrderType.RTB:
                    {
                        // Nothing to write
                    }
                    break;
                case FleetOrderType.Patrol:
                case FleetOrderType.PatrolPath:
                    {
                        var a = (ActivePatrolOrder)activeFleetOrder;
                        a.PathDirection = reader.ReadInt32();
                        a.NodeIndex = reader.ReadInt32();
                    }
                    break;
                case FleetOrderType.RepairAtNearest:
                case FleetOrderType.ManualRepair:
                    {
                        var a = (ActiveRepairFleetOrder)activeFleetOrder;
                        a.RepairState = (ActiveRepairFleetOrderState)reader.ReadInt32();

                        // TODO: Validate
                        var repairLocationUnitId = reader.ReadInt32();
                        a.CurrentRepairLocationUnit = units.FirstOrDefault(e => e.Id == repairLocationUnitId);
                    }
                    break;
                case FleetOrderType.Wait:
                    {
                        var a = (ActiveWaitOrder)activeFleetOrder;
                        a.WaitExpiryTime = reader.ReadDouble();
                    }
                    break;
                case FleetOrderType.SellCargo:
                    {
                        var a = (ActiveSellCargoOrder)activeFleetOrder;

                        a.SellExpireTime = reader.ReadDouble();

                        // TODO: Validate cargo class.
                        var cargoClassId = reader.ReadInt32();
                        a.SellCargoClass = (CargoClass)cargoClassId;

                        // TODO: Validate
                        var targetUnitId = reader.ReadInt32();
                        a.TraderTargetUnit = units.FirstOrDefault(e => e.Id == targetUnitId);

                        a.State = (ActiveSellCargoOrderState)reader.ReadInt32();
                    }
                    break;
                case FleetOrderType.MoveToNearestFriendlyStation:
                    {
                        var a = (ActiveMoveToNearestFriendlyStationOrder)activeFleetOrder;

                        // TODO: Validate
                        var targetStationId = reader.ReadInt32();

                        a.TargetStationUnit = units.FirstOrDefault(e => e.Id == targetStationId);
                    }
                    break;
            }

            return activeFleetOrder;
        }
    }
}
