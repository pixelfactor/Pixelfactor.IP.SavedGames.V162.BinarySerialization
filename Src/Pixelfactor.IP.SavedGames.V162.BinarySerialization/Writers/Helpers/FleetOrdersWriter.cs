using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.OrderTypes;
using System;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class FleetOrdersWriter
    {
        public static void Write(BinaryWriter writer, FleetOrderCollection fleetOrders)
        {
            writer.Write(fleetOrders.Orders.Count);

            foreach (var order in fleetOrders.Orders)
            {
                WriteOrder(writer, order);
            }

            writer.Write(fleetOrders.QueuedOrders.Count);
            foreach (var queuedOrder in fleetOrders.QueuedOrders)
            {
                writer.Write(fleetOrders.Orders.IndexOf(queuedOrder));
            }

            writer.Write(fleetOrders.CurrentOrder != null);
            if (fleetOrders.CurrentOrder != null)
            {
                writer.Write(fleetOrders.Orders.IndexOf(fleetOrders.CurrentOrder.Order));
                ActiveFleetOrderWriter.Write(writer, fleetOrders.CurrentOrder);
            }
        }

        public static void WriteOrder(BinaryWriter writer, FleetOrder order)
        {
            writer.Write((int)order.OrderType);

            writer.Write(order.Id);
            writer.Write((int)order.CompletionMode);
            writer.Write(order.AllowCombatInterception);
            writer.Write((int)order.CloakPreference);
            writer.Write(order.MaxJumpDistance);
            writer.Write(order.AllowTimeout);
            writer.Write(order.TimeoutTime);

            switch (order.OrderType)
            {
                case FleetOrderType.AttackFleet:
                    {
                        var o = (AttackFleetOrder)order;
                        writer.WriteFleetId(o.Target);
                        writer.Write(o.AttackPriority);
                    }
                    break;
                case FleetOrderType.CollectCargo:
                    {
                        var o = (CollectCargoOrder)order;
                        writer.Write(o.MaxCargoDistance);
                        writer.Write(o.CompleteWhenCargoFull);
                        writer.Write((int)o.CollectOwnerMode);
                        writer.Write(o.OresOnly);
                    }
                    break;

                case FleetOrderType.Scavenge:
                    {
                        var o = (ScavengeOrder)order;
                        writer.Write(o.MaxCargoDistance);
                        writer.Write(o.CompleteWhenCargoFull);
                        writer.Write((int)o.CollectOwnerMode);
                        writer.Write(o.RoamMaxTime);
                    }
                    break;
                case FleetOrderType.Mine:
                    {
                        var o = (MineOrder)order;
                        writer.Write(o.MaxCargoDistance);
                        writer.Write(o.CompleteWhenCargoFull);
                        writer.Write((int)o.CollectOwnerMode);
                        writer.WriteUnitId(o.ManualMineTarget);
                    }
                    break;
                case FleetOrderType.Dock:
                    {
                        var o = (DockOrder)order;
                        writer.WriteUnitId(o.TargetDock);
                    }
                    break;
                case FleetOrderType.Patrol:
                    {
                        var o = (PatrolOrder)order;

                        writer.Write(o.PathDirection);
                        writer.Write(o.IsLooping);

                        writer.Write(o.Nodes.Count);
                        foreach (var node in o.Nodes)
                        {
                            writer.WriteSectorId(node.Sector);
                            writer.WriteVector3(node.Position);
                        }

                        writer.Write(o.IsLoop);
                    }
                    break;
                case FleetOrderType.PatrolPath:
                    {
                        var o = (PatrolPathOrder)order;

                        writer.Write(o.PathDirection);
                        writer.Write(o.IsLooping);
                        writer.WriteSectorPatrolPathId(o.PatrolPath);
                    }
                    break;

                case FleetOrderType.Wait:
                    {
                        var o = (WaitOrder)order;
                        writer.Write(o.WaitTime);
                    }
                    break;

                case FleetOrderType.AttackTarget:
                    {
                        var o = (AttackTargetOrder)order;
                        writer.WriteUnitId(o.TargetUnit);
                        writer.Write(o.AttackPriority);
                    }
                    break;
                case FleetOrderType.Trade:
                    {
                        var o = (TradeOrder)order;
                        writer.Write(o.MinBuyQuantity);
                        writer.Write(o.MinBuyCargoPercentage);
                    }
                    break;
                case FleetOrderType.ManualTrade:
                    {
                        var o = (ManualTradeOrder)order;
                        writer.Write(o.MinBuyQuantity);
                        writer.Write(o.MinBuyCargoPercentage);

                        writer.Write(o.CustomTradeRoute != null);
                        if (o.CustomTradeRoute != null)
                        {
                            CustomTraderRouteWriter.Write(writer, o.CustomTradeRoute);
                        }
                    }
                    break;
                case FleetOrderType.UniverseTrade:
                    {
                        var o = (UniverseTradeOrder)order;
                        writer.Write(o.MinBuyQuantity);
                        writer.Write(o.MinBuyCargoPercentage);

                        writer.Write(o.TradeOnlySpecificCargoClasses);

                        writer.Write(o.TradeSpecificCargoClasses.Count);
                        foreach (var cargoClass in o.TradeSpecificCargoClasses)
                        {
                            writer.Write((int)cargoClass);
                        }
                    }
                    break;
                case FleetOrderType.JoinFleet:
                    {
                        var o = (JoinFleetOrder)order;
                        writer.WriteFleetId(o.TargetFleet);
                    }
                    break;

                case FleetOrderType.MoveTo:
                    {
                        var o = (MoveToOrder)order;
                        writer.Write(o.CompleteOnReachTarget);
                        writer.Write(o.ArrivalThreshold);
                        writer.Write(o.MatchTargetOrientation);
                        writer.Write(o.Target != null);
                        if (o.Target != null)
                        {
                            SectorTargetWriter.Write(writer, o.Target);
                        }
                    }
                    break;

                case FleetOrderType.Protect:
                    {
                        var o = (ProtectOrder)order;
                        writer.Write(o.CompleteOnReachTarget);
                        writer.Write(o.ArrivalThreshold);
                        writer.Write(o.MatchTargetOrientation);

                        writer.Write(o.Target != null);
                        if (o.Target != null)
                        {
                            SectorTargetWriter.Write(writer, o.Target);
                        }
                    }
                    break;

                case FleetOrderType.SellCargo:
                    {
                        var o = (SellCargoOrder)order;
                        writer.Write(o.FreeUnitsCompleteThreshold);
                        writer.Write(o.MinBuyPriceMultiplier);
                        writer.Write(o.SellOnlyListedCargos);
                        writer.Write(o.CompleteWhenNoBuyerFound);
                        writer.Write(o.CompleteWhenNoCargoToSell);
                        writer.WriteUnitId(o.ManualBuyerUnit);
                        writer.Write(o.CustomSellCargoTime);

                        writer.Write(o.SellCargoClasses.Count);
                        foreach (var cargoClass in o.SellCargoClasses)
                        {
                            writer.Write((int)cargoClass);
                        }

                        writer.Write(o.SellEquipment);
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
                        writer.Write((int)o.InsufficientCreditsMode);
                        writer.WriteUnitId(o.RepairLocationUnit);
                    }
                    break;
                case FleetOrderType.RepairAtNearest:
                    {
                        var o = (RepairAtNearestStationOrder)order;
                        writer.Write((int)o.InsufficientCreditsMode);
                    }
                    break;
                case FleetOrderType.MoveToNearestFriendlyStation:
                    {
                        var o = (MoveToNearestFriendlyStationOrder)order;
                        writer.Write(o.CompleteOnReachTarget);
                    }
                    break;
                default:
                    {
                        throw new Exception($"Unable to read data for objective of type {order.OrderType}. Unknown type");
                    }
            }
        }
    }
}
