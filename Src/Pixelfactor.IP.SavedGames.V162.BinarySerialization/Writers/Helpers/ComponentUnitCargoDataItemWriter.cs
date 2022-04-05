using Pixelfactor.IP.SavedGames.V162.Model;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class ComponentUnitCargoDataItemWriter
    {
        public static void Write(BinaryWriter writer, ComponentUnitCargoDataItem cargoItem)
        {
            writer.Write((int)cargoItem.CargoClass);
            writer.Write(cargoItem.Quantity);
        }
    }
}
