using Pixelfactor.IP.SavedGames.V162.Model.Factions;
using Pixelfactor.IP.SavedGames.V162.Model.Factions.FactionAITypes;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers
{
    public static class FactionAIWriter
    {
        public static void Write(BinaryWriter writer, FactionAIType factionAIType, FactionAI factionAI)
        {
            writer.Write(factionAI.GroupMaxJumpDist);
            writer.Write(factionAI.NextUnitSpawnTime);
            writer.Write(factionAI.NumFleetsSpawned);
            writer.Write(factionAI.NumUnitsSpawned);
            writer.WriteSectorId(factionAI.HomeSector);
            writer.Write(factionAI.SpawnOnlyAtOwnedDocks);
            writer.Write(factionAI.LastBuiltUnitTime);
            writer.Write(factionAI.LastOrderedPatrolTime);

            writer.Write((int)factionAI.SpawnMode);
            writer.Write(factionAI.SpawnSectors.Count);
            foreach (var sector in factionAI.SpawnSectors)
            {
                writer.WriteSectorId(sector);
            }

            switch (factionAIType)
            {
                case FactionAIType.Miner:
                case FactionAIType.Patroller:
                    {
                        // Nothing to read
                    }
                    break;
                case FactionAIType.Trader:
                    {
                        var trader = (FactionAITrader)factionAI;
                        writer.Write(trader.TradeOnlySpecificCargoTypes);

                        writer.Write(trader.TradeSpecificCargoTypes.Count);
                        foreach (var cargoClassId in trader.TradeSpecificCargoTypes)
                        {
                            writer.Write(cargoClassId);
                        }
                    }
                    break;
            }
        }
    }
}
