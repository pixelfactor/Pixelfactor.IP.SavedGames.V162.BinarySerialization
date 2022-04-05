using Pixelfactor.IP.SavedGames.V162.Model;
using System;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers
{
    public class HeaderWriter
    {
        public void Write(BinaryWriter writer, Header header)
        {
            writer.WriteVersion(header.Version);
            writer.Write(header.IsAutoSave);
            writer.
                Write(DateTime.Now.ToString(Constants.HeaderDateFormat));
            writer.Write(header.ScenarioInfoId);
            writer.Write(header.GlobalSaveNumber);
            writer.Write(header.SaveNumber);
            writer.Write(header.HavePlayer);
            if (header.HavePlayer)
            {
                writer.WriteStringOrEmpty(header.PlayerSectorName);
                writer.WriteStringOrEmpty(header.PlayerName);
                writer.Write(header.Credits);
            }
        }
    }
}
