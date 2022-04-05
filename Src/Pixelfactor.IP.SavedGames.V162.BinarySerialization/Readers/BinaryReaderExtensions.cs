using Pixelfactor.IP.SavedGames.V162.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers
{
    public static class BinaryReaderExtensions
    {
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            var v = new Vector3();
            v.X = reader.ReadSingle();
            v.Y = reader.ReadSingle();
            v.Z = reader.ReadSingle();
            return v;
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            var v = new Vector4();
            v.X = reader.ReadSingle();
            v.Y = reader.ReadSingle();
            v.Z = reader.ReadSingle();
            v.W = reader.ReadSingle();
            return v;
        }

        public static DamageType ReadDamageType(this BinaryReader reader)
        {
            var damageType = new DamageType();
            damageType.Damage = reader.ReadSingle();
            damageType.MiningDamage = reader.ReadSingle();
            damageType.ShieldDamageType = (ShieldDamageType)reader.ReadInt32();
            return damageType;
        }

        public static Unit ReadUnit(this BinaryReader reader, IEnumerable<Unit> units)
        {
            var unitId = reader.ReadInt32();
            return units.FirstOrDefault(e => e.Id == unitId);
        }

        public static Faction ReadFaction(this BinaryReader reader, IEnumerable<Faction> factions)
        {
            var factionId = reader.ReadInt32();
            return factions.FirstOrDefault(e => e.Id == factionId);
        }

        public static Sector ReadSector(this BinaryReader reader, IEnumerable<Sector> sectors)
        {
            var sectorId = reader.ReadInt32();
            return sectors.FirstOrDefault(e => e.Id == sectorId);
        }

        public static Person ReadPerson(this BinaryReader reader, IEnumerable<Person> people)
        {
            var personId = reader.ReadInt32();
            return people.FirstOrDefault(e => e.Id == personId);
        }

        public static Fleet ReadFleet(this BinaryReader reader, IEnumerable<Fleet> fleets)
        {
            var fleetId = reader.ReadInt32();
            return fleets.FirstOrDefault(e => e.Id == fleetId);
        }

        public static Version ReadVersion(this BinaryReader reader)
        {
            var versionMajor = reader.ReadInt32();
            var versionMinor = reader.ReadInt32();
            var versionBuild = reader.ReadInt32();
            return new Version(versionMajor, versionMinor, versionBuild);
        }
    }
}
