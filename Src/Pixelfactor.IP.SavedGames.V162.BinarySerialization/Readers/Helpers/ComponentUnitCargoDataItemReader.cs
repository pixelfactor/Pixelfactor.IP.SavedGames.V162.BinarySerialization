using Pixelfactor.IP.SavedGames.V162.Model;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers
{
    public static class ComponentUnitCargoDataItemReader
    {
        public static ComponentUnitCargoDataItem Read(BinaryReader reader)
        {
            var item = new ComponentUnitCargoDataItem();
            item.CargoClass = (CargoClass)reader.ReadInt32();
            item.Quantity = reader.ReadInt32();
            return item;
        }
    }
}
