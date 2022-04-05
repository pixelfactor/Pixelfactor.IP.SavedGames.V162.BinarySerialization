using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.Models;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class CustomTraderRouteWriter
    {
        public static void Write(BinaryWriter writer, CustomTradeRoute customTradeRoute)
        {
            writer.Write(customTradeRoute.CargoClassId);
            writer.WriteUnitId(customTradeRoute.BuyLocation);
            writer.WriteUnitId(customTradeRoute.SellLocation);
            writer.Write(customTradeRoute.BuyPriceMultiplier);
        }
    }
}
