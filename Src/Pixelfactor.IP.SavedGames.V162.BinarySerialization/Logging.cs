namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization
{
    /// <summary>
    /// TODO: Don't log to console
    /// </summary>
    public static class Logging
    {
        public static void UnknownUnitMessage(int unitId, string message)
        {
            Warning($"SaveGameReader: Unknown unit ID: {unitId} when {message}");
        }

        public static void UnknownSectorMessage(int sectorId, string message)
        {
            Warning($"SaveGameReader: Unknown sector ID: {sectorId} when {message}");
        }

        public static void MissingSectorMessage(string message)
        {
            Warning($"SaveGameReader: Missing sector when {message}");
        }

        public static void UnknownFactionMessage(int factionId, string message)
        {
            Warning($"SaveGameReader: Unknown faction ID: {factionId} when {message}");
        }

        public static void Warning(string message)
        {
            Console.WriteLine(message);
        }

        internal static void UnknownPersonMessage(int personId, string message)
        {
            Warning($"SaveGameReader: Unknown person ID: {personId} when {message}");
        }
    }
}
