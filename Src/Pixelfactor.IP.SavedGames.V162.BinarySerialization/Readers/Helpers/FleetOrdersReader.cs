using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.Models;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.OrderTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class FleetOrdersReader
    {
        public static FleetOrderCollection ReadOrders(
            BinaryReader reader,
            int fleetId,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<SectorPatrolPath> patrolPaths,
            IEnumerable<Person> people)
        {
            var fleetOrders = new FleetOrderCollection();
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var order = ReadOrder(reader, factions, sectors, units, fleets, patrolPaths);
                fleetOrders.Orders.Add(order);
            }

            var queuedCount = reader.ReadInt32();
            for (int i = 0; i < queuedCount; i++)
            {
                var index = reader.ReadInt32();
                if (index < 0 || index >= fleetOrders.Orders.Count)
                {
                    Logging.Warning($"Fleet {fleetId} contains an invalid order index {index}");
                }
                else
                {
                    fleetOrders.QueuedOrders.Add(fleetOrders.Orders[index]);
                }
            }

            var hasCurrentOrder = reader.ReadBoolean();
            if (hasCurrentOrder)
            {
                var index = reader.ReadInt32();

                if (index < 0 || index >= fleetOrders.Orders.Count)
                {
                    throw new Exception($"Fleet {fleetId} contains an invalid order index {index}");
                }

                var currentOrder = fleetOrders.Orders[index];

                var activeOrder = ReadActiveOrder(
                    reader,
                    currentOrder,
                    fleetId,
                    factions,
                    sectors,
                    units,
                    fleets,
                    patrolPaths,
                    people);

                fleetOrders.CurrentOrder = activeOrder;
            }

            return fleetOrders;
        }

        public static ActiveFleetOrder ReadActiveOrder(
            BinaryReader reader,
            FleetOrder fleetOrder,
            int fleetId,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<SectorPatrolPath> patrolPaths,
            IEnumerable<Person> people)
        {
            var fleetOrderType = fleetOrder.OrderType;
            var activeOrder = CreateActiveFleetOrderFromType.Create(fleetOrderType);
            ActiveFleetOrderReader.Read(
                reader,
                fleetOrderType,
                fleetId,
                factions,
                sectors,
                units,
                fleets,
                patrolPaths,
                people);
            activeOrder.Order = fleetOrder;

            return activeOrder;
        }

        public static FleetOrder ReadOrder(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<SectorPatrolPath> patrolPaths)
        {
            var orderType = (FleetOrderType)reader.ReadInt32();

            var order = CreateFleetOrderFromType.Create(orderType);
            order.Id = reader.ReadInt32();
            order.CompletionMode = (FleetOrderCompletionMode)reader.ReadInt32();
            order.AllowCombatInterception = reader.ReadBoolean();
            order.CloakPreference = (FleetOrderCloakPreference)reader.ReadInt32();
            order.MaxJumpDistance = reader.ReadInt32();
            order.AllowTimeout = reader.ReadBoolean();
            order.TimeoutTime = reader.ReadSingle();

            switch (order.OrderType)
            {
                case FleetOrderType.AttackFleet:
                    {
                        var o = (AttackFleetOrder)order;
                        var targetFleetId = reader.ReadInt32();
                        o.Target = fleets.FirstOrDefault(e => e.Id == targetFleetId);
                        o.AttackPriority = reader.ReadSingle();
                    }
                    break;
                case FleetOrderType.CollectCargo:
                    {
                        var o = (CollectCargoOrder)order;
                        o.MaxCargoDistance = reader.ReadSingle();
                        o.CompleteWhenCargoFull = reader.ReadBoolean();
                        o.CollectOwnerMode = (CollectCargoOwnerMode)reader.ReadInt32();
                        o.OresOnly = reader.ReadBoolean();
                    }
                    break;

                case FleetOrderType.Scavenge:
                    {
                        var o = (ScavengeOrder)order;
                        o.MaxCargoDistance = reader.ReadSingle();
                        o.CompleteWhenCargoFull = reader.ReadBoolean();
                        o.CollectOwnerMode = (CollectCargoOwnerMode)reader.ReadInt32();

                        o.RoamMaxTime = reader.ReadSingle();
                    }
                    break;
                case FleetOrderType.Mine:
                    {
                        var o = (MineOrder)order;
                        o.MaxCargoDistance = reader.ReadSingle();
                        o.CompleteWhenCargoFull = reader.ReadBoolean();
                        o.CollectOwnerMode = (CollectCargoOwnerMode)reader.ReadInt32();

                        var manualMineTargetUnitId = reader.ReadInt32();
                        var manualMineTargetUnit = units.FirstOrDefault(e => e.Id == manualMineTargetUnitId);
                        if (manualMineTargetUnit != null)
                        {
                            o.ManualMineTarget = manualMineTargetUnit;
                        }
                    }
                    break;
                case FleetOrderType.Dock:
                    {
                        var o = (DockOrder)order;
                        var unitId = reader.ReadInt32();
                        o.TargetDock = units.FirstOrDefault(e => e.Id == unitId);
                    }
                    break;
                case FleetOrderType.Patrol:
                    {
                        var o = (PatrolOrder)order;

                        o.PathDirection = reader.ReadInt32();
                        o.IsLooping = reader.ReadBoolean();

                        var nodeCount = reader.ReadInt32();
                        for (var i = 0; i < nodeCount; i++)
                        {
                            var node = new PatrolPathNode();
                            var sectorId = reader.ReadInt32();
                            node.Sector = sectors.FirstOrDefault(e => e.Id == sectorId);
                            node.Position = reader.ReadVector3();
                            o.Nodes.Add(node);
                        }

                        o.IsLoop = reader.ReadBoolean();
                    }
                    break;
                case FleetOrderType.PatrolPath:
                    {
                        var o = (PatrolPathOrder)order;

                        o.PathDirection = reader.ReadInt32();
                        o.IsLooping = reader.ReadBoolean();

                        var patrolPathId = reader.ReadInt32();
                        o.PatrolPath = patrolPaths.FirstOrDefault(e => e.Id == patrolPathId);
                    }
                    break;

                case FleetOrderType.Wait:
                    {
                        var o = (WaitOrder)order;
                        o.WaitTime = reader.ReadSingle();
                    }
                    break;

                case FleetOrderType.AttackTarget:
                    {
                        var o = (AttackTargetOrder)order;
                        var targetUnitId = reader.ReadInt32();
                        o.TargetUnit = units.FirstOrDefault(e => e.Id == targetUnitId);
                        o.AttackPriority = reader.ReadSingle();
                    }
                    break;
                case FleetOrderType.Trade:
                    {
                        var o = (TradeOrder)order;
                        o.MinBuyQuantity = reader.ReadInt32();
                        o.MinBuyCargoPercentage = reader.ReadSingle();
                    }
                    break;
                case FleetOrderType.ManualTrade:
                    {
                        var o = (ManualTradeOrder)order;
                        o.MinBuyQuantity = reader.ReadInt32();
                        o.MinBuyCargoPercentage = reader.ReadSingle();

                        var hasTradeRoute = reader.ReadBoolean();
                        if (hasTradeRoute)
                        {
                            o.CustomTradeRoute = CustomTradeRouteReader.Read(reader, units);
                        }
                    }
                    break;
                case FleetOrderType.UniverseTrade:
                    {
                        var o = (UniverseTradeOrder)order;
                        o.MinBuyQuantity = reader.ReadInt32();
                        o.MinBuyCargoPercentage = reader.ReadSingle();

                        o.TradeOnlySpecificCargoClasses = reader.ReadBoolean();

                        o.TradeSpecificCargoClasses.Clear();
                        var cargoCount = reader.ReadInt32();
                        for (var i = 0; i < cargoCount; i++)
                        {
                            // TODO: Change cargo class id to enum
                            o.TradeSpecificCargoClasses.Add((CargoClass)reader.ReadInt32());
                        }
                    }
                    break;
                case FleetOrderType.JoinFleet:
                    {
                        var o = (JoinFleetOrder)order;
                        var fleetId = reader.ReadInt32();
                        var fleet = fleets.FirstOrDefault(e => e.Id == fleetId);

                        // TODO: Verify fleet valid
                        o.TargetFleet = fleet;
                    }
                    break;

                case FleetOrderType.MoveTo:
                    {
                        var o = (MoveToOrder)order;
                        o.CompleteOnReachTarget = reader.ReadBoolean();
                        o.ArrivalThreshold = reader.ReadSingle();
                        o.MatchTargetOrientation = reader.ReadBoolean();

                        var hasTarget = reader.ReadBoolean();
                        if (hasTarget)
                        {
                            o.Target = SectorTargetReader.Read(reader, sectors, units, fleets);
                        }
                    }
                    break;

                case FleetOrderType.Protect:
                    {
                        var o = (ProtectOrder)order;
                        o.CompleteOnReachTarget = reader.ReadBoolean();
                        o.ArrivalThreshold = reader.ReadSingle();
                        o.MatchTargetOrientation = reader.ReadBoolean();

                        var hasTarget = reader.ReadBoolean();
                        if (hasTarget)
                        {
                            o.Target = SectorTargetReader.Read(reader, sectors, units, fleets);
                        }
                    }
                    break;

                case FleetOrderType.SellCargo:
                    {
                        var o = (SellCargoOrder)order;
                        o.FreeUnitsCompleteThreshold = reader.ReadInt32();
                        o.MinBuyPriceMultiplier = reader.ReadSingle();
                        o.SellOnlyListedCargos = reader.ReadBoolean();
                        o.CompleteWhenNoBuyerFound = reader.ReadBoolean();
                        o.CompleteWhenNoCargoToSell = reader.ReadBoolean();

                        var manualBuyerUnitId = reader.ReadInt32();
                        // TODO: Validate
                        o.ManualBuyerUnit = units.FirstOrDefault(e => e.Id == manualBuyerUnitId);
                        o.CustomSellCargoTime = reader.ReadSingle();

                        var sellCargoClassCount = reader.ReadInt32();
                        for (var i = 0; i < sellCargoClassCount; i++)
                        {
                            // TODO: Validate
                            var cargoClassId = reader.ReadInt32();
                            o.SellCargoClasses.Add((CargoClass)cargoClassId);
                        }

                        o.SellEquipment = reader.ReadBoolean();
                    }
                    break;
                case FleetOrderType.RTB:
                case FleetOrderType.DisposeCargo:
                case FleetOrderType.UniversePassengerTransport:
                case FleetOrderType.UniverseBountyHunter:
                case FleetOrderType.UniverseRoam:
                case FleetOrderType.Explore:
                    {
                        // nothing to read/write
                    }
                    break;
                case FleetOrderType.ManualRepair:
                    {
                        var o = (ManualRepairFleetOrder)order;
                        o.InsufficientCreditsMode = (RepairFleetInsufficientCreditsMode)reader.ReadInt32();

                        var repairLocationUnitId = reader.ReadInt32();

                        // TODO: Validate
                        o.RepairLocationUnit = units.FirstOrDefault(e => e.Id == repairLocationUnitId);
                    }
                    break;
                case FleetOrderType.RepairAtNearest:
                    {
                        var o = (RepairAtNearestStationOrder)order;
                        o.InsufficientCreditsMode = (RepairFleetInsufficientCreditsMode)reader.ReadInt32();
                    }
                    break;
                case FleetOrderType.MoveToNearestFriendlyStation:
                    {
                        var o = (MoveToNearestFriendlyStationOrder)order;
                        o.CompleteOnReachTarget = reader.ReadBoolean();
                    }
                    break;
                default:
                    {
                        throw new Exception($"Unable to read data for objective of type {order.OrderType}. Unknown type");
                    }
            }

            return order;
        }
    }
}
