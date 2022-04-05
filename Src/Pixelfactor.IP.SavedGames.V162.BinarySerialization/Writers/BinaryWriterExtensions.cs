using Pixelfactor.IP.SavedGames.V162.Model;
using System;
using System.IO;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers
{
    public static class BinaryWriterExtensions
    {
        public static void WriteVector3(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }

        public static void WriteVector4(this BinaryWriter writer, Vector4 vector4)
        {
            writer.Write(vector4.X);
            writer.Write(vector4.Y);
            writer.Write(vector4.Z);
            writer.Write(vector4.Z);
        }

        public static void WriteNullableVector3(this BinaryWriter writer, Vector3? vector3)
        {
            writer.Write(vector3.HasValue);
            if (vector3.HasValue)
            {
                WriteVector3(writer, vector3.Value);
            }
        }

        public static void WriteStringOrEmpty(this BinaryWriter writer, string str)
        {
            if (str != null)
            {
                writer.Write(str);
            }
            else
            {
                writer.Write(string.Empty);
            }
        }

        public static void WriteDamageType(this BinaryWriter writer, DamageType damageType)
        {
            writer.Write(damageType.Damage);
            writer.Write(damageType.MiningDamage);
            writer.Write((int)damageType.ShieldDamageType);
        }

        public static void WriteUnitId(this BinaryWriter writer, Unit unit)
        {
            writer.Write(unit != null ? unit.Id : -1);
        }

        public static void WriteFactionId(this BinaryWriter writer, Faction faction)
        {
            writer.Write(faction != null ? faction.Id : -1);
        }

        public static void WriteSectorId(this BinaryWriter writer, Sector sector)
        {
            writer.Write(sector != null ? sector.Id : -1);
        }

        public static void WritePersonId(this BinaryWriter writer, Person person)
        {
            writer.Write(person != null ? person.Id : -1);
        }

        public static void WriteFleetId(this BinaryWriter writer, Fleet fleet)
        {
            writer.Write(fleet != null ? fleet.Id : -1);
        }

        public static void WritePassengerGroupId(this BinaryWriter writer, PassengerGroup passengerGroup)
        {
            writer.Write(passengerGroup != null ? passengerGroup.Id : -1);
        }

        public static void WriteVersion(this BinaryWriter writer, Version version)
        {
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Build);
        }

        public static void WriteSectorPatrolPathId(this BinaryWriter writer, SectorPatrolPath patrolPath)
        {
            writer.Write(patrolPath != null ? patrolPath.Id : -1);
        }
    }
}
