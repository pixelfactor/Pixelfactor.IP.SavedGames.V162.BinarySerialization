using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.FleetOrders.Models;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class CustomTradeRouteReader
    {
        public static CustomTradeRoute Read(BinaryReader reader, IEnumerable<Unit> units)
        {
            var tradeRoute = new CustomTradeRoute();
            var cargoClassId = reader.ReadInt32();
            var buyLocationUnitId = reader.ReadInt32();
            var sellLocationUnitId = reader.ReadInt32();
            var buyPriceMultiplier = reader.ReadSingle();

            // TODO: Verify all this stuff
            tradeRoute.CargoClassId = cargoClassId;
            tradeRoute.BuyLocation = units.FirstOrDefault(e => e.Id == buyLocationUnitId);
            tradeRoute.SellLocation = units.FirstOrDefault(e => e.Id == sellLocationUnitId);
            tradeRoute.BuyPriceMultiplier = buyPriceMultiplier;

            return tradeRoute;
        }
    }
}
