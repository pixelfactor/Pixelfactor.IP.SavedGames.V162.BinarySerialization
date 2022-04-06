using Pixelfactor.IP.SavedGames.V162.Model.Helpers;
using Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers.Helpers;
using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.Factions;
using Pixelfactor.IP.SavedGames.V162.Model.Factions.Bounty;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Readers
{
    public class SaveGameReader
    {
        private readonly HeaderReader headerReader;

        public SaveGameReader(
            HeaderReader headerReader)
        {
            this.headerReader = headerReader;
        }

        public static SavedGame ReadFromPath(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                var savedGameReader = new SaveGameReader(new HeaderReader());
                return savedGameReader.Read(reader);
            }
        }

        public SavedGame Read(BinaryReader reader)
        {
            var savedGame = new SavedGame();

            savedGame.Header = this.headerReader.Read(reader);
            PrintStatus("Loaded header", reader);

            savedGame.SecondsElapsed = reader.ReadDouble();
            savedGame.Sectors.AddRange(ReadSectors(reader));
            PrintStatus("Loaded sectors", reader);

            savedGame.Factions.AddRange(ReadFactions(reader, savedGame.Sectors));
            PrintStatus("Loaded factions", reader);

            savedGame.PatrolPaths.AddRange(ReadPatrolPaths(reader, savedGame.Sectors));
            PrintStatus("Loaded patrol paths", reader);

            ReadAllFactionRelations(reader, savedGame.Factions);
            PrintStatus("Loaded faction relations", reader);

            ReadAllFactionOpinions(reader, savedGame.Factions);
            PrintStatus("Loaded faction opinions", reader);

            ReadUnits(reader, savedGame);
            PrintStatus("Loaded units", reader);

            ReadNamedUnits(reader, savedGame.Units);
            PrintStatus("Loaded named units", reader);

            ReadAllComponentUnits(reader, savedGame.Units);
            PrintStatus("Loaded all unit components", reader);

            ReadModdedComponents(reader, savedGame.Units);
            PrintStatus("Loaded modded components", reader);

            ReadUnitCapacitorCharges(reader, savedGame.Units);
            PrintStatus("Loaded unit capacitor charges", reader);

            ReadCloakedUnits(reader, savedGame.Units);
            PrintStatus("Loaded unit cloak states", reader);

            ReadPoweredDownComponents(reader, savedGame.Units);
            PrintStatus("Loaded powered down units", reader);

            ReadUnitEngineThrottles(reader, savedGame.Units);
            PrintStatus("Loaded engine throttle data", reader);

            ReadComponentUnitCargo(reader, savedGame.Units);
            PrintStatus("Loaded cargo", reader);

            ReadAllShieldHealthData(reader, savedGame.Units);
            PrintStatus("Loaded damaged shields", reader);

            ReadAllUnitComponentHealthData(reader, savedGame.Units);
            PrintStatus("Loaded damaged components", reader);

            ReadActiveUnits(reader, savedGame.Units);
            PrintStatus("Loaded active units", reader);

            ReadAllUnitHealthDatas(reader, savedGame.Units);
            PrintStatus("Loaded destructable units", reader);

            ReadAllFactionIntel(reader, savedGame.Factions, savedGame.Sectors, savedGame.Units);
            PrintStatus("Loaded faction intel", reader);

            ReadPassengerGroups(reader, savedGame.Units);
            PrintStatus("Loaded passenger groups", reader);

            ReadWormholes(reader, savedGame.Sectors, savedGame.Units);
            PrintStatus("Loaded wormholes", reader);

            ReadHangars(reader, savedGame.Units);
            PrintStatus("Loaded hangers", reader);

            ReadTraders(reader);
            PrintStatus("Loaded traders (unused)", reader);

            savedGame.Fleets.AddRange(ReadFleets(
                reader,
                savedGame.Factions,
                savedGame.Sectors,
                savedGame.Units,
                savedGame.PatrolPaths,
                savedGame.People));

            PrintStatus("Loaded fleets", reader);

            var people = ReadPeople(reader, savedGame.Factions, savedGame.Units, out Person playerPerson);
            savedGame.People.AddRange(people);

            PrintStatus("Loaded people", reader);

            ReadNpcPilots(reader, savedGame.People, savedGame.Fleets);
            PrintStatus("Loaded NPC pilots", reader);

            ReadFactionLeaders(reader, savedGame.Factions, savedGame.People);
            PrintStatus("Loaded faction leaders", reader);

            ReadJobs(reader, savedGame.Sectors, savedGame.Factions, savedGame.Units);
            PrintStatus("Loaded jobs", reader);

            ReadAllFactionAIsAndBountyBoards(reader, savedGame.Factions, savedGame.Sectors, savedGame.Units, savedGame.People);
            PrintStatus("Loaded faction AIs / bounty boards", reader);

            ReadFactionAIExcludedUnits(reader, savedGame.Factions, savedGame.Units);
            PrintStatus("Loaded faction excluded unit data", reader);

            ReadFactionMercenaryData(reader, savedGame.Factions);
            PrintStatus("Loaded mercenary data", reader);

            savedGame.FleetSpawners.AddRange(ReadFleetSpawners(
                reader,
                savedGame.Factions,
                savedGame.Sectors,
                savedGame.Units,
                savedGame.Fleets,
                savedGame.People,
                savedGame.PatrolPaths));
            PrintStatus("Loaded NPC fleet spawners", reader);

            savedGame.ActiveJobs.AddRange(ReadActiveJobs(reader, savedGame.Sectors, savedGame.Factions, savedGame.Units, savedGame.Fleets));
            PrintStatus("Loaded jobs", reader);

            var havePlayer = reader.ReadBoolean();
            if (havePlayer)
            {
                savedGame.Player = ReadGamePlayer(
                    reader,
                    savedGame.Sectors,
                    savedGame.Units,
                    savedGame.ActiveJobs);

                savedGame.Player.Person = playerPerson;

                PrintStatus("Loaded player data", reader);
            }

            savedGame.CurrentHudTarget = reader.ReadUnit(savedGame.Units);
            PrintStatus("Loaded hud data", reader);

            ReadAllFactionTransactions(reader, savedGame.Factions, savedGame.Units);
            PrintStatus("Loaded all faction transactions", reader);

            savedGame.ScenarioData = ReadScenarioData(reader);
            PrintStatus("Loaded world", reader);

            var fleets = ReadAllPlayerFleets(reader, savedGame.Fleets).ToList();
            if (savedGame.Player != null)
            {
                savedGame.Player.Fleets.AddRange(fleets.Where(e => e != null));
            }

            PrintStatus("Loaded player fleet groups", reader);

            savedGame.PlayerUnitFleetMap.AddRange(ReadPlayerUnitFleetMap(reader, savedGame.Units, savedGame.Fleets));
            PrintStatus("Loaded generic autopilot fleets", reader);

            savedGame.Moons.AddRange(ReadMoons(reader, savedGame.Units));
            PrintStatus("Loaded moons", reader);

            return savedGame;
        }

        /// <summary>
        /// Aka moons
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="units"></param>
        private IEnumerable<Moon> ReadMoons(BinaryReader reader, IEnumerable<Unit> units)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var orbitUnitId = reader.ReadInt32();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                var orbitUnit = units.FirstOrDefault(e => e.Id == orbitUnitId);

                if (unit != null && orbitUnit != null)
                {
                    yield return new Moon
                    {
                        Unit = unit,
                        OrbitUnit = orbitUnit
                    };
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fleets"></param>
        private IEnumerable<PlayerUnitFleetMap> ReadPlayerUnitFleetMap(BinaryReader reader, List<Unit> units, List<Fleet> fleets)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var unit = reader.ReadUnit(units);
                var fleet = reader.ReadFleet(fleets);

                yield return new PlayerUnitFleetMap
                {
                    Unit = unit,
                    Fleet = fleet
                };
            }
        }

        private static IEnumerable<Fleet> ReadAllPlayerFleets(BinaryReader reader, IEnumerable<Fleet> fleets)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                yield return reader.ReadFleet(fleets);
            }
        }

        private ScenarioData ReadScenarioData(BinaryReader reader)
        {
            var scenarioData = new ScenarioData();
            scenarioData.HasRandomEvents = reader.ReadBoolean();
            if (scenarioData.HasRandomEvents)
            {
                scenarioData.NextRandomEventTime = reader.ReadDouble();
            }

            var hasFactionSpawner = reader.ReadBoolean();
            if (hasFactionSpawner)
            {
                scenarioData.FactionSpawner = new FactionSpawner();
                scenarioData.FactionSpawner.NextUpdate = reader.ReadDouble();

                // Not used
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    // No longer used
                    reader.ReadInt32();
                }
            }

            return scenarioData;
        }

        private void ReadAllFactionTransactions(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var factionCount = reader.ReadInt32();

            for (var i = 0; i < factionCount; i++)
            {
                var faction = reader.ReadFaction(factions);
                var transactionCount = reader.ReadInt32();

                for (int j = 0; j < transactionCount; j++)
                {
                    var transaction = ReadFactionTransaction(reader, factions, units);
                    faction.Transactions.Add(transaction);
                }
            }
        }

        private static FactionTransaction ReadFactionTransaction(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var transaction = new FactionTransaction();
            transaction.TransactionType = (FactionTransactionType)reader.ReadInt32();
            transaction.Value = reader.ReadInt32();
            transaction.CurrentBalance = reader.ReadInt32();
            transaction.LocationUnit = reader.ReadUnit(units);
            transaction.OtherFaction = reader.ReadFaction(factions);
            transaction.RelatedCargoClass = (CargoClass)reader.ReadInt32();
            transaction.RelatedUnitClass = (UnitClass)reader.ReadInt32();
            transaction.GameWorldTime = reader.ReadDouble();
            return transaction;
        }

        private Player ReadGamePlayer(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<ActiveJob> activeJobs)
        {
            // Visited docks
            var player = new Player();
            var visitedDockCount = reader.ReadInt32();
            for (var i = 0; i < visitedDockCount; i++)
            {
                var u = reader.ReadUnit(units);
                if (u != null)
                {
                    player.VisitedUnits.Add(u);
                }
            }

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                player.Messages.Add(ReadPlayerMessage(reader, units));
            }

            var delayedMessageCount = reader.ReadInt32();
            for (var i = 0; i < delayedMessageCount; i++)
            {
                var delayedMessage = new PlayerDelayedMessage();
                delayedMessage.ShowTime = reader.ReadDouble();
                delayedMessage.Message = ReadPlayerMessage(reader, units);
                player.DelayedMessages.Add(delayedMessage);
            }

            player.CustomWaypoint = ReadPlayerWaypointIfSet(reader, sectors, units);

            var activeJobId = reader.ReadInt32();
            player.ActiveJob = activeJobs.FirstOrDefault(e => e.Id == activeJobId);

            return player;
        }

        private static PlayerWaypoint ReadPlayerWaypointIfSet(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units)
        {
            var hasWaypoint = reader.ReadBoolean();
            if (hasWaypoint)
            {
                return ReadPlayerWaypoint(reader, sectors, units);
            }

            return null;
        }

        public static PlayerWaypoint ReadPlayerWaypoint(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units)
        {
            var waypoint = new PlayerWaypoint();
            waypoint.Position = reader.ReadVector3();
            waypoint.Sector = reader.ReadSector(sectors);
            waypoint.TargetUnit = reader.ReadUnit(units);
            waypoint.HadTargetObject = reader.ReadBoolean();
            return waypoint;
        }

        public static PlayerMessage ReadPlayerMessage(
            BinaryReader reader,
            IEnumerable<Unit> units)
        {
            var message = new PlayerMessage();
            message.Id = reader.ReadInt32();
            message.EngineTimeStamp = reader.ReadDouble();
            message.AllowDelete = reader.ReadBoolean();
            message.Opened = reader.ReadBoolean();
            message.SenderUnit = reader.ReadUnit(units);
            message.SubjectUnit = reader.ReadUnit(units);

            var hasTemplate = reader.ReadBoolean();
            if (hasTemplate)
            {
                message.MessageTemplateId = reader.ReadInt32();
            }
            else
            {
                message.MessageTemplateId = -1;
                message.ToText = reader.ReadString();
                message.FromText = reader.ReadString();
                message.MessageText = reader.ReadString();
                message.SubjectText = reader.ReadString();
            }

            return message;
        }

        private IEnumerable<ActiveJob> ReadActiveJobs(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                yield return ActiveJobReader.Read(reader, sectors, factions, units, fleets);
            }
        }

        private IEnumerable<FleetSpawner> ReadFleetSpawners(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<Person> people,
            IEnumerable<SectorPatrolPath> patrolPaths)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                yield return FleetSpawnerReader.Read(
                    reader,
                    factions,
                    sectors,
                    units,
                    fleets,
                    people,
                    patrolPaths);
            }
        }

        private void ReadFactionMercenaryData(BinaryReader reader, IEnumerable<Faction> factions)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var factionId = reader.ReadInt32();
                var hiringFactionId = reader.ReadInt32();
                var hireExpiryTime = reader.ReadDouble();

                var faction = factions.FirstOrDefault(e => e.Id == factionId);
                var hiringFaction = factions.FirstOrDefault(e => e.Id == factionId);

                if (faction != null)
                {
                    if (hiringFaction != null)
                    {
                        if (faction.FactionAI != null)
                        {
                            faction.FactionAI.FactionMercenaryHireInfo = new FactionMercenaryHireInfo
                            {
                                HireTimeExpiry = hireExpiryTime,
                                HiringFaction = hiringFaction
                            };
                        }
                        else
                        {
                            Logging.Warning($"Expecting mercenary faction {factionId} to have FactionAI");
                        }
                    }
                    else
                    {
                        Logging.UnknownFactionMessage(hiringFactionId, $"loading mercenary info for faction {factionId}");
                    }
                }
                else
                {
                    Logging.UnknownFactionMessage(factionId, $"loading mercenary info");
                }
            }
        }

        private void ReadFactionAIExcludedUnits(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unit = reader.ReadUnit(units); ;
                var faction = reader.ReadFaction(factions);

                if (faction.FactionAI != null)
                {
                    faction.FactionAI.ExcludedUnits.Add(unit);
                }
            }
        }

        private void ReadAllFactionAIsAndBountyBoards(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Person> people)
        {
            var factionCount = reader.ReadInt32();

            for (var i = 0; i < factionCount; i++)
            {
                var faction = reader.ReadFaction(factions);
                var hasAI = reader.ReadBoolean();

                if (hasAI)
                {
                    var aiType = (FactionAIType)reader.ReadInt32();

                    var factionAI = FactionAIReader.Read(reader, aiType, sectors);
                    faction.FactionAI = factionAI;
                    faction.FactionAI.AIType = aiType;
                }

                bool hasBountyBoard = reader.ReadBoolean();
                if (hasBountyBoard)
                {
                    faction.BountyBoard = ReadFactionBountyBoard(reader, factions, sectors, units, people);
                }
            }
        }

        private static FactionBountyBoard ReadFactionBountyBoard(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Person> people)
        {
            var bountyBoard = new FactionBountyBoard();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = ReadFactionBountyBoardItem(reader, factions, sectors, units, people);
                bountyBoard.Items.Add(item);
            }

            return bountyBoard;
        }

        private static FactionBountyBoardItem ReadFactionBountyBoardItem(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Person> people)
        {
            var item = new FactionBountyBoardItem();
            item.TargetPerson = reader.ReadPerson(people);
            item.Reward = reader.ReadInt32();
            item.LastKnownTargetUnit = reader.ReadUnit(units);
            item.LastKnownTargetSector = reader.ReadSector(sectors);

            var hasLastKnownPosition = reader.ReadBoolean();
            if (hasLastKnownPosition)
            {
                item.LastKnownTargetPosition = reader.ReadVector3();
            }

            item.SourceFaction = reader.ReadFaction(factions);

            return item;
        }

        private void ReadJobs(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var jobType = (JobType)reader.ReadInt32();
                var job = JobReader.Read(reader, jobType, sectors, factions, units);

                if (job.Unit != null)
                {
                    job.Unit.Jobs.Add(job);
                }
            }
        }

        private void ReadFactionLeaders(BinaryReader reader, IEnumerable<Faction> factions, IEnumerable<Person> people)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var factionId = reader.ReadInt32();
                var personId = reader.ReadInt32();

                var faction = factions.FirstOrDefault(e => e.Id == factionId);
                var person = people.FirstOrDefault(e => e.Id == personId);

                if (faction != null)
                {
                    if (person != null)
                    {
                        faction.Leader = person;
                    }
                    else
                    {
                        Logging.UnknownPersonMessage(personId, "loading faction leaders");
                    }
                }
                else
                {
                    Logging.UnknownFactionMessage(factionId, "loading faction leaders");
                }
            }
        }

        private void ReadNpcPilots(BinaryReader reader, List<Person> people, List<Fleet> fleets)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var npcPilot = ReadNpcPilot(reader, people);
                if (npcPilot.Person != null)
                {
                    npcPilot.Person.NpcPilot = npcPilot;
                }

                var fleetId = reader.ReadInt32();
                npcPilot.Fleet = fleets.FirstOrDefault(e => e.Id == fleetId);

                if (npcPilot.Fleet != null)
                {
                    npcPilot.Fleet.Npcs.Add(npcPilot);
                }
            }
        }

        private NpcPilot ReadNpcPilot(BinaryReader reader, IEnumerable<Person> people)
        {
            var npcPilot = new NpcPilot();

            var personId = reader.ReadInt32();
            var person = people.FirstOrDefault(e => e.Id == personId);

            npcPilot.Person = person;
            npcPilot.AllowUseCloak = reader.ReadBoolean();
            npcPilot.DestroyWhenNoUnit = reader.ReadBoolean();
            npcPilot.DestroyWhenNotPilotting = reader.ReadBoolean();
            return npcPilot;
        }

        private IEnumerable<Person> ReadPeople(BinaryReader reader, IEnumerable<Faction> factions, IEnumerable<Unit> units, out Person playerPerson)
        {
            playerPerson = null;

            var people = new List<Person>(100);
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var person = ReadPerson(reader, factions, units, out bool isPlayer);
                if (isPlayer)
                {
                    playerPerson = person;
                }

                people.Add(person);
            }

            return people;
        }

        private static Person ReadPerson(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units,
            out bool isPlayer)
        {
            var id = reader.ReadInt32();

            var person = new Person();
            person.Id = id;

            var generatedName = reader.ReadBoolean();
            if (generatedName)
            {
                person.GeneratedFirstNameId = reader.ReadInt32();
                person.GeneratedLastNameId = reader.ReadInt32();
            }
            else
            {
                person.CustomName = reader.ReadString();
            }

            person.DialogId = reader.ReadInt32();
            person.IsMale = reader.ReadBoolean();

            var factionId = reader.ReadInt32();
            person.Faction = factions.FirstOrDefault(e => e.Id == factionId);
            person.DestroyGameObjectOnKill = reader.ReadBoolean();

            var unitId = reader.ReadInt32();
            var unit = units.FirstOrDefault(e => e.Id == unitId);

            if (unitId > -1 && unit == null)
            {
                Logging.UnknownUnitMessage(unitId, $"loading person id {id}");
            }

            if (unit != null)
            {
                unit.ComponentUnitData.People.Add(person);
                person.CurrentUnit = unit;
            }

            var isPilot = reader.ReadBoolean();
            if (isPilot && unit != null)
            {
                unit.ComponentUnitData.Pilot = person;
                person.IsPilot = isPilot;
            }

            person.Kills = reader.ReadInt32();
            isPlayer = reader.ReadBoolean();

            var hasUnitControllerProfile = reader.ReadBoolean();
            if (hasUnitControllerProfile)
            {
                person.NpcPilotSettings = ReadNpcPilotSettings(reader);
            }

            return person;
        }

        private static NpcPilotSettings ReadNpcPilotSettings(BinaryReader reader)
        {
            var settings = new NpcPilotSettings();
            settings.UsesCloak = reader.ReadBoolean();
            settings.UseCloakPreference = reader.ReadSingle();
            settings.EnterCombatCloakedProbability = reader.ReadSingle();
            settings.RestrictedWeaponPreference = reader.ReadSingle();
            settings.CombatEfficiency = reader.ReadSingle();
            settings.CheatAmmo = reader.ReadBoolean();
            return settings;
        }

        private IEnumerable<Fleet> ReadFleets(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<SectorPatrolPath> patrolPaths,
            IEnumerable<Person> people)
        {
            var count = reader.ReadInt32();
            var fleets = new List<Fleet>();
            for (var i = 0; i < count; i++)
            {
                // TODO: Bug in 1.6.x where some relying on fleets are using the fleet collection that hasn't been fully built yet. Objectives should be loaded after all fleets are loaded
                fleets.Add(ReadFleet(reader, factions, sectors, units, fleets, patrolPaths, people));
            }

            return fleets;
        }

        private Fleet ReadFleet(
            BinaryReader reader,
            IEnumerable<Faction> factions,
            IEnumerable<Sector> sectors,
            IEnumerable<Unit> units,
            IEnumerable<Fleet> fleets,
            IEnumerable<SectorPatrolPath> sectorPatrolPaths,
            IEnumerable<Person> people)
        {
            var fleet = new Fleet();
            fleet.IsActive = reader.ReadBoolean();
            fleet.Id = reader.ReadInt32();

            fleet.Position = reader.ReadVector3();
            fleet.Rotation = reader.ReadVector4();
            var sectorId = reader.ReadInt32();
            fleet.Sector = sectors.FirstOrDefault(e => e.Id == sectorId);
            var factionId = reader.ReadInt32();
            fleet.Faction = factions.FirstOrDefault(e => e.Id == factionId);
            var homeBaseUnitId = reader.ReadInt32();
            fleet.HomeBaseUnit = units.FirstOrDefault(e => e.Id == homeBaseUnitId);
            fleet.ExcludeFromFactionAI = reader.ReadBoolean();

            bool hasSettings = reader.ReadBoolean();
            if (hasSettings)
            {
                fleet.FleetSettings = ReadFleetSettings(reader);
            }

            fleet.Orders = FleetOrdersReader.ReadOrders(
                reader,
                fleet.Id,
                factions,
                sectors,
                units,
                fleets,
                sectorPatrolPaths,
                people);

            return fleet;
        }

        private FleetSettings ReadFleetSettings(BinaryReader reader)
        {
            var settings = new FleetSettings();
            settings.ControllersCanCollectCargo = reader.ReadBoolean();
            settings.ControllersCollectOnlyEquipment = reader.ReadBoolean();
            settings.PreferCloak = reader.ReadBoolean();
            settings.PreferToDock = reader.ReadBoolean();
            settings.NotifyWhenOrderComplete = reader.ReadBoolean();
            settings.AttackTargetScoreThreshold = reader.ReadSingle();
            settings.AllowAttack = reader.ReadBoolean();
            settings.TargetInterceptionLowerDistance = reader.ReadSingle();
            settings.TargetInterceptionUpperDistance = reader.ReadSingle();
            settings.RestrictMaxJumps = reader.ReadBoolean();
            settings.MaxJumpDistance = reader.ReadInt32();
            settings.AllowCombatInterception = reader.ReadBoolean();
            settings.DestroyWhenNoPilots = reader.ReadBoolean();
            return settings;
        }

        /// <summary>
        /// No longer used
        /// </summary>
        /// <param name="reader"></param>
        private void ReadTraders(BinaryReader reader)
        {
            var traderCount = reader.ReadInt32();

            for (var i = 0; i < traderCount; i++)
            {
                // Data no longer used
                var _ = reader.ReadInt32();
            }
        }

        private void ReadHangars(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var hangarItems = ReadHangerItems(reader).ToList();
                var hangarUnit = units.FirstOrDefault(e => e.Id == unitId);

                if (hangarUnit != null)
                {
                    if (hangarUnit.ComponentUnitData.DockData == null)
                    {
                        hangarUnit.ComponentUnitData.DockData = new ComponentUnitDockData();
                    }

                    foreach (var item in hangarItems)
                    {
                        if (item.dockedUnitId > -1)
                        {
                            var dockedUnit = units.FirstOrDefault(e => e.Id == item.dockedUnitId);
                            if (dockedUnit != null)
                            {
                                hangarUnit.ComponentUnitData.DockData.Items.Add(new ComponentUnitDockDataItem
                                {
                                    BayId = item.bayId,
                                    DockedUnit = dockedUnit
                                });
                            }
                            else
                            {
                                Logging.UnknownUnitMessage(item.dockedUnitId, $"loading hangars. Unknown docked unit inside unit {unitId}");
                            }
                        }
                    }
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, "loading hangars. Unknown hangar unit.");
                }
            }
        }

        private IEnumerable<(int bayId, int dockedUnitId)> ReadHangerItems(BinaryReader reader)
        {
            var dockedUnitCount = reader.ReadInt32();

            for (var j = 0; j < dockedUnitCount; j++)
            {
                var bayId = reader.ReadInt32();
                var dockedUnitId = reader.ReadInt32();
                yield return (bayId, dockedUnitId);
            }
        }

        private void ReadWormholes(BinaryReader reader, List<Sector> sectors, List<Unit> units)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var wormholeData = ReadWormholeData(reader, unitId, sectors, units);
                var unit = units.FirstOrDefault(e => e.Id == unitId);

                if (unit != null)
                {
                    unit.WormholeData = wormholeData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, "loading wormhole data");
                }
            }
        }

        private UnitWormholeData ReadWormholeData(BinaryReader reader, int unitId, List<Sector> sectors, List<Unit> units)
        {
            var wormholeData = new UnitWormholeData();

            var targetUnitId = reader.ReadInt32();
            wormholeData.TargetWormholeUnit = units.FirstOrDefault(e => e.Id == targetUnitId);
            if (wormholeData.TargetWormholeUnit == null)
            {
                Logging.UnknownUnitMessage(targetUnitId, $"loading wormhole data for unit {unitId}");
            }

            wormholeData.IsUnstable = reader.ReadBoolean();
            wormholeData.UnstableNextChangeTargetTime = reader.ReadDouble();

            wormholeData.UnstableTargetPosition = reader.ReadVector3();
            wormholeData.UnstableTargetRotation = reader.ReadVector3();

            var targetSectorId = reader.ReadInt32();
            wormholeData.UnstableTargetSector = sectors.FirstOrDefault(e => e.Id == targetSectorId);

            if (wormholeData.IsUnstable && wormholeData.UnstableTargetSector == null)
            {
                Logging.MissingSectorMessage($"loading wormhole data for unit {unitId}. Unstable wormhole must have target");
            }

            return wormholeData;
        }

        private void ReadPassengerGroups(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var passengerGroup = ReadPassengerGroup(reader, units);
                if (passengerGroup.Unit != null)
                {
                    passengerGroup.Unit.PassengerGroups.Add(passengerGroup);
                }
            }
        }

        private PassengerGroup ReadPassengerGroup(BinaryReader reader, IEnumerable<Unit> units)
        {
            var passengerGroup = new PassengerGroup();
            passengerGroup.Id = reader.ReadInt32();

            var currentUnitId = reader.ReadInt32();
            var sourceUnitId = reader.ReadInt32();
            var destinationUnit = reader.ReadInt32();

            passengerGroup.Unit = units.FirstOrDefault(e => e.Id == currentUnitId);
            passengerGroup.SourceUnit = units.FirstOrDefault(e => e.Id == sourceUnitId);
            passengerGroup.DestinationUnit = units.FirstOrDefault(e => e.Id == destinationUnit);

            passengerGroup.PassengerCount = reader.ReadInt32();
            passengerGroup.ExpiryTime = reader.ReadDouble();

            passengerGroup.Revenue = reader.ReadInt32();
            return passengerGroup;
        }

        private void ReadAllFactionIntel(BinaryReader reader, IEnumerable<Faction> factions, IEnumerable<Sector> sectors, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var factionId = reader.ReadInt32();
                var factionIntel = ReadFactionIntel(reader, factionId, sectors, units);

                var faction = factions.FirstOrDefault(e => e.Id == factionId);
                if (faction != null)
                {
                    faction.Intel = factionIntel;
                }
                else
                {
                    Logging.UnknownFactionMessage(factionId, "loading intel");
                }
            }
        }

        private FactionIntel ReadFactionIntel(BinaryReader reader, int factionId, IEnumerable<Sector> sectors, IEnumerable<Unit> units)
        {
            var factionIntel = new FactionIntel();
            var discoveredSceneCount = reader.ReadInt32();
            for (var i = 0; i < discoveredSceneCount; i++)
            {
                var sectorId = reader.ReadInt32();
                var sector = sectors.FirstOrDefault(e => e.Id == sectorId);
                if (sector != null)
                {
                    factionIntel.Sectors.Add(sector);
                }
                else
                {
                    Logging.UnknownSectorMessage(sectorId, $"loading intel for faction {factionId}");
                }
            }

            var discoveredDockCount = reader.ReadInt32();
            for (var i = 0; i < discoveredDockCount; i++)
            {
                var unitId = reader.ReadInt32();
                var unit = units.FirstOrDefault(e => e.Id == unitId);
                if (unit != null)
                {
                    factionIntel.Units.Add(unit);
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading intel for faction {factionId}");
                }
            }

            return factionIntel;
        }

        private void ReadAllUnitHealthDatas(BinaryReader reader, List<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var healthData = ReadUnitHealthData(reader);
                var unit = units.FirstOrDefault(e => e.Id == unitId);

                if (unit != null)
                {
                    unit.HealthData = healthData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading health data for unit {unitId}");
                }
            }
        }

        private UnitHealthData ReadUnitHealthData(BinaryReader reader)
        {
            var healthData = new UnitHealthData();
            healthData.IsDestroyed = reader.ReadBoolean();
            healthData.TotalDamagedReceived = reader.ReadSingle();
            healthData.Health = reader.ReadSingle();
            return healthData;
        }

        private void ReadActiveUnits(BinaryReader reader, List<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var activeData = ReadActiveUnitData(reader);
                var unit = units.FirstOrDefault(e => e.Id == unitId)?.ComponentUnitData;
                if (unit != null)
                {
                    unit.ActiveData = activeData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading active data for unit {unitId}");
                }
            }
        }

        private ComponentUnitActiveData ReadActiveUnitData(BinaryReader reader)
        {
            var activeData = new ComponentUnitActiveData();
            activeData.Velocity = reader.ReadVector3();
            activeData.CurrentTurn = reader.ReadSingle();
            return activeData;
        }

        private void ReadAllUnitComponentHealthData(BinaryReader reader, List<Unit> units)
        {
            var unitCount = reader.ReadInt32();

            for (int i = 0; i < unitCount; i++)
            {
                var unitId = reader.ReadInt32();
                var unit = units.FirstOrDefault(e => e.Id == unitId);

                var componentHealthData = ReadUnitComponentHealthData(reader);

                if (unit?.ComponentUnitData != null)
                {
                    unit.ComponentUnitData.ComponentHealthData = componentHealthData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading component heatlh data for unit {unitId}");
                }
            }
        }

        private ComponentUnitComponentHealthData ReadUnitComponentHealthData(BinaryReader reader)
        {
            var componentHealthData = new ComponentUnitComponentHealthData();
            var damagedComponentCount = reader.ReadInt32();

            for (int i = 0; i < damagedComponentCount; i++)
            {
                var item = new ComponentUnitComponentHealthDataItem();
                item.BayId = reader.ReadInt32();
                item.Health = reader.ReadSingle();
                componentHealthData.Items.Add(item);
            }

            return componentHealthData;
        }

        private void ReadAllShieldHealthData(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var shieldData = ReadShieldHealthData(reader);

                var unit = units.FirstOrDefault(e => e.Id == unitId)?.ComponentUnitData;

                if (unit != null)
                {
                    unit.ShieldData = shieldData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading shield health data for unit {unitId}");
                }
            }
        }

        private ComponentUnitShieldHealthData ReadShieldHealthData(BinaryReader reader)
        {
            var damagedShieldCount = reader.ReadByte();

            var shieldData = new ComponentUnitShieldHealthData();
            for (int j = 0; j < damagedShieldCount; j++)
            {
                var item = new ComponentUnitShieldHealthDataItem();
                item.ShieldPointIndex = reader.ReadByte();
                item.Health = reader.ReadSingle();
                shieldData.Items.Add(item);
            }

            return shieldData;
        }

        private void ReadComponentUnitCargo(BinaryReader reader, List<Unit> units)
        {
            var unitCount = reader.ReadInt32();
            for (int i = 0; i < unitCount; i++)
            {
                var unitId = reader.ReadInt32();
                var unit = units.FirstOrDefault(e => e.Id == unitId)?.ComponentUnitData;

                var cargoData = ReadComponentUnitCargoData(reader);

                if (unit != null)
                {
                    unit.CargoData = cargoData;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading cargo data for unit {unitId}");
                }
            }
        }

        private ComponentUnitCargoData ReadComponentUnitCargoData(BinaryReader reader)
        {
            var cargoData = new ComponentUnitCargoData();
            var itemCount = reader.ReadInt32();

            for (var i = 0; i < itemCount; i++)
            {
                cargoData.Items.Add(ComponentUnitCargoDataItemReader.Read(reader));
            }

            return cargoData;
        }

        private void ReadUnitEngineThrottles(BinaryReader reader, List<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var engineThrottle = reader.ReadSingle();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                if (unit?.ComponentUnitData != null)
                {
                    unit.ComponentUnitData.EngineThrottle = engineThrottle;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading engine throttle data for unit {unitId}");
                }
            }
        }

        private void ReadPoweredDownComponents(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var bayId = reader.ReadInt32();

                var unit = units.FirstOrDefault(e => e.Id == unitId);

                if (unit?.ComponentUnitData != null)
                {
                    unit.ComponentUnitData.PoweredDownBayIds.Add(bayId);
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading powered-down component data for unit {unitId}");
                }
            }
        }

        private void ReadCloakedUnits(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();

                var unit = units.FirstOrDefault(e => e.Id == unitId);

                if (unit?.ComponentUnitData != null)
                {
                    unit.ComponentUnitData.IsCloaked = true;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading cloak state data for unit {unitId}");
                }
            }
        }

        private void ReadUnitCapacitorCharges(BinaryReader reader, List<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var chargeNormalized = reader.ReadSingle();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                if (unit?.ComponentUnitData != null)
                {
                    unit.ComponentUnitData.CapacitorCharge = chargeNormalized;
                }
                else
                {
                    Logging.UnknownUnitMessage(unitId, $"loading capacitor charge data for unit {unitId}");
                }
            }
        }

        private void ReadModdedComponents(BinaryReader reader, List<Unit> units)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var bayId = reader.ReadInt32();
                var componentClassId = reader.ReadInt32();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                if (unit != null)
                {
                    if (unit.ComponentUnitData.ModData == null)
                    {
                        unit.ComponentUnitData.ModData = new ComponentUnitModData();
                    }

                    unit.ComponentUnitData.ModData.Items.Add(new ComponentUnitModDataItem
                    {
                        BayId = bayId,
                        ComponentClass = (ComponentClass)componentClassId
                    });
                }
            }
        }

        private void ReadAllComponentUnits(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                unit.ComponentUnitData = ReadComponentUnitData(reader);
            }
        }

        private ComponentUnitData ReadComponentUnitData(BinaryReader reader)
        {
            var componentUnitData = new ComponentUnitData();
            componentUnitData.ShipNameIndex = reader.ReadInt32();

            if (componentUnitData.ShipNameIndex == -1)
            {
                componentUnitData.CustomShipName = reader.ReadString();
            }

            componentUnitData.CargoCapacity = reader.ReadSingle();

            var hasFactory = reader.ReadBoolean();

            if (hasFactory)
            {
                componentUnitData.FactoryData = ReadComponentUnitFactoryData(reader);
            }

            componentUnitData.IsUnderConstruction = reader.ReadBoolean();
            componentUnitData.ConstructionProgress = reader.ReadSingle();

            componentUnitData.StationUnitClassNumber = reader.ReadInt32();

            return componentUnitData;
        }

        private ComponentUnitFactoryData ReadComponentUnitFactoryData(BinaryReader reader)
        {
            var factoryData = new ComponentUnitFactoryData();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var item = new ComponentUnitFactoryItemData();
                item.State = (ComponentUnitFactoryItemState)reader.ReadInt32();
                item.ProductionElapsed = reader.ReadSingle();
                factoryData.Items.Add(item);
            }

            return factoryData;
        }

        private void ReadNamedUnits(BinaryReader reader, IEnumerable<Unit> units)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unitId = reader.ReadInt32();
                var unitName = reader.ReadString();

                var unit = units.FirstOrDefault(e => e.Id == unitId);
                if (unit != null)
                {
                    unit.Name = unitName;
                }
            }
        }

        private void PrintStatus(string message, BinaryReader reader)
        {
            Console.WriteLine($"{message} - {reader.BaseStream.Position - 1} bytes read");
        }

        private void ReadUnits(
            BinaryReader reader,
            SavedGame savedGame)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var unit = ReadUnit(reader, savedGame.Sectors, savedGame.Factions, savedGame.Units);
                savedGame.Units.Add(unit);
            }
        }

        private Unit ReadUnit(
            BinaryReader reader,
            IEnumerable<Sector> sectors,
            IEnumerable<Faction> factions,
            IEnumerable<Unit> units)
        {
            var unit = new Unit();

            unit.Id = reader.ReadInt32();

            var classId = reader.ReadInt32();

            if (!Enum.IsDefined(typeof(UnitClass), unit.Class))
                throw new Exception($"Unrecognised unit class {unit.Class}");

            unit.Class = (UnitClass)classId;

            var sectorId = reader.ReadInt32();
            unit.Sector = sectors.FirstOrDefault(e => e.Id == sectorId);

            if (sectorId > -1 && unit.Sector == null)
            {
                Logging.UnknownSectorMessage(sectorId, $"loading unit {unit.Id}");
            }

            unit.Position = reader.ReadVector3();
            unit.Rotation = reader.ReadVector4();

            var factionId = reader.ReadInt32();
            unit.Faction = factions.FirstOrDefault(e => e.Id == factionId);

            if (factionId > -1 && unit.Faction == null)
            {
                Logging.UnknownFactionMessage(factionId, $"loading unit {unit.Id}");
            }

            unit.RpProvision = reader.ReadInt32();

            var hasCargo = reader.ReadBoolean();
            if (hasCargo)
            {
                unit.CargoData = ReadUnitCargoData(reader);
            }

            var hasDebris = reader.ReadBoolean();
            if (hasDebris)
            {
                unit.DebrisData = ReadUnitDebrisData(reader);
            }

            var hasShipTrader = reader.ReadBoolean();
            if (hasShipTrader)
            {
                unit.ShipTraderData = ReadShipTrader(reader);
            }

            if (UnitHelper.IsProjectile(unit.Class))
            {
                var hasProjectile = reader.ReadBoolean();
                if (hasProjectile)
                {
                    unit.ProjectileData = ReadUnitProjectileData(reader, units);
                }
            }

            return unit;
        }

        private UnitProjectileData ReadUnitProjectileData(BinaryReader reader, IEnumerable<Unit> units)
        {
            var projectileData = new UnitProjectileData();
            projectileData.SourceUnit = reader.ReadUnit(units);
            projectileData.TargetUnit = reader.ReadUnit(units);
            projectileData.FireTime = reader.ReadDouble();
            projectileData.RemainingMovement = reader.ReadSingle();
            projectileData.DamageType = reader.ReadDamageType();
            return projectileData;
        }

        private UnitShipTraderData ReadShipTrader(BinaryReader reader)
        {
            var shipTraderData = new UnitShipTraderData();

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var item = new UnitShipTraderItem();
                item.SellMultiplier = reader.ReadSingle();
                item.UnitClassId = reader.ReadInt32();
                shipTraderData.Items.Add(item);
            }

            return shipTraderData;
        }

        private UnitDebrisData ReadUnitDebrisData(BinaryReader reader)
        {
            var debrisData = new UnitDebrisData();
            debrisData.ScrapQuantity = reader.ReadInt32();
            debrisData.Expires = reader.ReadBoolean();
            debrisData.ExpiryTime = reader.ReadDouble();
            debrisData.RelatedUnitClassId = reader.ReadInt32();
            return debrisData;
        }

        private UnitCargoData ReadUnitCargoData(BinaryReader reader)
        {
            var unitCargoData = new UnitCargoData();
            unitCargoData.CargoClass = (CargoClass)reader.ReadInt32();
            unitCargoData.Quantity = reader.ReadInt32();
            unitCargoData.Expires = reader.ReadBoolean();
            unitCargoData.ExpiryTime = reader.ReadDouble();
            return unitCargoData;
        }

        private IEnumerable<Faction> ReadFactions(BinaryReader reader, IEnumerable<Sector> sectors)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                yield return ReadFaction(reader, sectors);
            }
        }

        private IEnumerable<Sector> ReadSectors(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                yield return ReadSector(reader);
            }
        }

        private IEnumerable<SectorPatrolPath> ReadPatrolPaths(BinaryReader reader, IEnumerable<Sector> sectors)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                yield return ReadPatrolPath(reader, sectors);
            }
        }

        private void ReadAllFactionRelations(BinaryReader reader, IEnumerable<Faction> factions)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var factionId = reader.ReadInt32();
                var factionRelations = ReadFactionRelationData(reader, factionId, factions);
                var faction = factions.FirstOrDefault(e => e.Id == factionId);

                if (faction != null)
                {
                    faction.Relations = factionRelations;
                }
                else
                {
                    Logging.UnknownFactionMessage(factionId, "loading faction relations");
                }
            }
        }

        private FactionRelationData ReadFactionRelationData(BinaryReader reader, int factionId, IEnumerable<Faction> factions)
        {
            var factionRelationData = new FactionRelationData();

            var relationCount = reader.ReadInt32();

            for (var i = 0; i < relationCount; i++)
            {
                var item = ReadFactionRelationDataItem(reader, factionId, factions);
                if (item.OtherFaction != null)
                {
                    factionRelationData.Items.Add(item);
                }
            }

            return factionRelationData;
        }

        private FactionRelationDataItem ReadFactionRelationDataItem(BinaryReader reader, int factionId, IEnumerable<Faction> factions)
        {
            var relation = new FactionRelationDataItem();

            var otherFactionId = reader.ReadInt32();
            relation.OtherFaction = factions.FirstOrDefault(e => e.Id == otherFactionId);

            if (relation.OtherFaction == null)
            {
                Logging.UnknownFactionMessage(otherFactionId, $"loading faction relation data for faction {factionId}");
            }

            relation.PermanentPeace = reader.ReadBoolean();
            relation.RestrictHostilityTimeout = reader.ReadBoolean();
            relation.Neutrality = (Neutrality)reader.ReadInt32();
            relation.HostilityEndTime = reader.ReadDouble();
            relation.RecentDamageReceived = reader.ReadSingle();
            return relation;
        }

        private void ReadAllFactionOpinions(BinaryReader reader, IEnumerable<Faction> factions)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var factionId = reader.ReadInt32();
                var otherFactionId = reader.ReadInt32();
                var opinion = reader.ReadSingle();

                var faction = factions.FirstOrDefault(e => e.Id == factionId);
                var otherFaction = factions.FirstOrDefault(e => e.Id == otherFactionId);

                if (faction != null)
                {
                    if (otherFaction == faction)
                    {
                        Logging.Warning($"Faction opinion for faction {factionId} is referencing itself. Item will not be loaded.");
                    }
                    else
                    {
                        if (faction.Opinions == null)
                        {
                            faction.Opinions = new FactionOpinionData();
                        }

                        if (otherFaction != null)
                        {
                            faction.Opinions.Items.Add(new FactionOpinionDataItem
                            {
                                OtherFaction = otherFaction,
                                Opinion = opinion
                            });
                        }
                        else
                        {
                            Logging.UnknownFactionMessage(otherFactionId, "loading faction opinions");
                        }
                    }
                }
                else
                {
                    Logging.UnknownFactionMessage(factionId, "loading faction opinions");
                }
            }
        }

        private Sector ReadSector(BinaryReader reader)
        {
            var sector = new Sector();
            sector.Id = reader.ReadInt32();
            sector.Name = reader.ReadString();
            sector.MapPosition = reader.ReadVector3();
            sector.ResourceName = reader.ReadString();
            sector.Description = reader.ReadString();
            sector.GateDistanceMultiplier = reader.ReadSingle();
            sector.RandomSeed = reader.ReadInt32();
            sector.Position = reader.ReadVector3();
            sector.BackgroundRotation = reader.ReadVector3();
            sector.LightRotation = reader.ReadVector3();
            return sector;
        }

        private Faction ReadFaction(BinaryReader reader, IEnumerable<Sector> sectors)
        {
            var faction = new Faction();
            faction.Id = reader.ReadInt32();
            faction.HasGeneratedName = reader.ReadBoolean();
            if (faction.HasGeneratedName)
            {
                faction.GeneratedNameId = reader.ReadInt32();
                faction.GeneratedSuffixId = reader.ReadInt32();
            }
            else
            {
                faction.HasCustomName = reader.ReadBoolean();
                if (faction.HasCustomName)
                {
                    faction.CustomName = reader.ReadString();
                    faction.CustomShortName = reader.ReadString();
                }
            }

            faction.Credits = reader.ReadInt32();
            faction.Description = reader.ReadString();
            faction.IsCivilian = reader.ReadBoolean();
            faction.FactionType = (FactionType)reader.ReadInt32();
            faction.Aggression = reader.ReadSingle();
            faction.Virtue = reader.ReadSingle();
            faction.Greed = reader.ReadSingle();
            faction.TradeEfficiency = reader.ReadSingle();
            faction.DynamicRelations = reader.ReadBoolean();
            faction.ShowJobBoards = reader.ReadBoolean();
            faction.CreateJobs = reader.ReadBoolean();
            faction.RequisitionPointMultiplier = reader.ReadSingle();
            faction.DestroyWhenNoUnits = reader.ReadBoolean();
            faction.MinNpcCombatEfficiency = reader.ReadSingle();
            faction.MaxNpcCombatEfficiency = reader.ReadSingle();
            faction.AdditionalRpProvision = reader.ReadInt32();
            faction.TradeIllegalGoods = reader.ReadBoolean();
            faction.SpawnTime = reader.ReadDouble();
            faction.HighestEverNetWorth = reader.ReadInt32();

            bool hasAISettings = reader.ReadBoolean();

            if (hasAISettings)
            {
                faction.CustomSettings = ReadFactionCustomSettings(reader);
            }

            bool hasStats = reader.ReadBoolean();
            if (hasStats)
            {
                faction.Stats = ReadFactionStats(reader);
            }

            var excludedSectorCount = reader.ReadInt32();
            for (int i = 0; i < excludedSectorCount; i++)
            {
                var sectorId = reader.ReadInt32();
                var sector = sectors.FirstOrDefault(e => e.Id == sectorId);
                faction.AutopilotExcludedSectors.Add(sector);
            }

            return faction;
        }

        private FactionCustomSettings ReadFactionCustomSettings(BinaryReader reader)
        {
            var settings = new FactionCustomSettings();
            settings.PreferSingleShip = reader.ReadBoolean();
            settings.RepairShips = reader.ReadBoolean();
            settings.UpgradeShips = reader.ReadBoolean();
            settings.RepairMinHullDamage = reader.ReadSingle();
            settings.RepairMinCreditsBeforeRepair = reader.ReadInt32();
            settings.PreferenceToPlaceBounty = reader.ReadSingle();
            settings.LargeShipPreference = reader.ReadSingle();
            settings.DailyIncome = reader.ReadInt32();
            settings.HostileWithAll = reader.ReadBoolean();
            settings.MinFleetUnitCount = reader.ReadInt32();
            settings.MaxFleetUnitCount = reader.ReadInt32();
            settings.OffensiveStance = reader.ReadSingle();
            settings.AllowOtherFactionToUseDocks = reader.ReadBoolean();
            settings.PreferenceToBuildTurrets = reader.ReadSingle();
            settings.PreferenceToBuildStations = reader.ReadSingle();
            settings.IgnoreStationCreditsReserve = reader.ReadBoolean();
            return settings;
        }

        private FactionStats ReadFactionStats(BinaryReader reader)
        {
            var stats = new FactionStats();
            stats.TotalShipsClaimed = reader.ReadInt32();
            stats.UnitsDestroyedByClassId = ReadFactionStatsUnitCounts(reader);
            stats.UnitLostByClassId = ReadFactionStatsUnitCounts(reader);
            stats.ScratchcardsScratched = reader.ReadInt32();
            stats.HighestScratchcardWin = reader.ReadInt32();

            return stats;
        }

        private Dictionary<int, int> ReadFactionStatsUnitCounts(BinaryReader reader)
        {
            var itemCount = reader.ReadInt32();
            var items = new Dictionary<int, int>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(reader.ReadInt32(), reader.ReadInt32());
            }

            return items;
        }

        private SectorPatrolPath ReadPatrolPath(BinaryReader reader, IEnumerable<Sector> sectors)
        {
            var path = new SectorPatrolPath();
            path.Id = reader.ReadInt32();
            path.Sector = reader.ReadSector(sectors);
            path.IsLoop = reader.ReadBoolean();

            // nodes
            int nodeCount = reader.ReadInt32();
            for (int i = 0; i < nodeCount; i++)
            {
                path.Nodes.Add(new SectorPatrolPathNode
                {
                    Position = reader.ReadVector3(),
                    Order = reader.ReadInt32()
                });
            }

            return path;
        }
    }
}
