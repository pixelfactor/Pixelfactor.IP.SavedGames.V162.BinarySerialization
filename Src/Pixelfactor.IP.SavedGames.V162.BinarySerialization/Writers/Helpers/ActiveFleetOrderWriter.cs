using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.ActiveOrderTypes;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class ActiveFleetOrderWriter
    {
        public static void Write(BinaryWriter writer, ActiveFleetOrder activeFleetOrder)
        {
            writer.Write(activeFleetOrder.TimeoutTime);
            writer.Write(activeFleetOrder.StartTime);

            switch (activeFleetOrder.Order.OrderType)
            {
                case FleetOrderType.AttackFleet:
                    {
                        var a = (ActiveAttackFleetOrder)activeFleetOrder;
                        writer.WriteFleetId(a.TargetFleet);
                    }
                    break;
                case FleetOrderType.AttackTarget:
                    {
                        var a = (ActiveAttackTargetOrder)activeFleetOrder;
                        writer.WriteUnitId(a.TargetUnit);
                    }
                    break;
                case FleetOrderType.UniverseBountyHunter:
                    {
                        var a = (ActiveUniverseBountyHunterOrder)activeFleetOrder;
                        writer.WritePersonId(a.TargetPerson);
                    }
                    break;
                case FleetOrderType.UniverseRoam:
                    {
                        var a = (ActiveUniverseRoamOrder)activeFleetOrder;
                        writer.WriteSectorId(a.CurrentTargetSector);
                        writer.WriteVector3(a.CurrentTargetPosition);
                    }
                    break;
                case FleetOrderType.Explore:
                    {
                        var a = (ActiveExploreOrder)activeFleetOrder;
                        writer.WriteSectorId(a.CurrentTargetSector);
                        writer.WriteVector3(a.CurrentTargetPosition);
                    }
                    break;
                case FleetOrderType.Trade:
                case FleetOrderType.UniverseTrade:
                case FleetOrderType.ManualTrade:
                    {
                        var a = (ActiveTradeOrder)activeFleetOrder;
                        writer.Write(a.TradeRoute != null);
                        if (a.TradeRoute != null)
                        {
                            CustomTraderRouteWriter.Write(writer, a.TradeRoute);
                        }

                        writer.Write(a.EndBuySellTime);
                        writer.Write(a.LastStateChangeTime);
                        writer.Write((int)a.CurrentState);
                    }
                    break;
                case FleetOrderType.UniversePassengerTransport:
                    {
                        var a = (ActiveUniversePassengerTransportOrder)activeFleetOrder;
                        writer.WritePassengerGroupId(a.PassengerGroup);
                        writer.Write(a.EndBuySellTime);
                        writer.Write(a.LastStateChangeTime);
                        writer.Write((int)a.CurrentState);
                    }
                    break;
                case FleetOrderType.CollectCargo:
                case FleetOrderType.Mine:
                    {
                        var a = (ActiveCollectCargoOrder)activeFleetOrder;
                        writer.WriteUnitId(a.TractorTargetUnit);
                        writer.Write(a.AutoFindCargoEnabled);
                        writer.Write(a.AutoTractorCargoEnabled);
                    }
                    break;
                case FleetOrderType.Scavenge:
                    {
                        var a = (ActiveScavengeOrder)activeFleetOrder;
                        writer.WriteUnitId(a.TractorTargetUnit);
                        writer.Write(a.AutoFindCargoEnabled);
                        writer.Write(a.AutoTractorCargoEnabled);
                        writer.Write(a.IsRoaming);
                        writer.Write(a.RoamExpireTime);
                        writer.WriteVector3(a.LastKnownCargoPosition);
                        writer.Write(a.HadCargoTarget);
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
                        writer.Write(a.PathDirection);
                        writer.Write(a.NodeIndex);
                    }
                    break;
                case FleetOrderType.RepairAtNearest:
                case FleetOrderType.ManualRepair:
                    {
                        var a = (ActiveRepairFleetOrder)activeFleetOrder;
                        writer.Write((int)a.RepairState);
                        writer.WriteUnitId(a.CurrentRepairLocationUnit);
                    }
                    break;
                case FleetOrderType.Wait:
                    {
                        var a = (ActiveWaitOrder)activeFleetOrder;
                        writer.Write(a.WaitExpiryTime);
                    }
                    break;
                case FleetOrderType.SellCargo:
                    {
                        var a = (ActiveSellCargoOrder)activeFleetOrder;
                        writer.Write(a.SellExpireTime);
                        writer.Write((int)a.SellCargoClass);
                        writer.WriteUnitId(a.TraderTargetUnit);
                        writer.Write((int)a.State);
                    }
                    break;
                case FleetOrderType.MoveToNearestFriendlyStation:
                    {
                        var a = (ActiveMoveToNearestFriendlyStationOrder)activeFleetOrder;
                        writer.WriteUnitId(a.TargetStationUnit);
                    }
                    break;
            }
        }
    }
}
