using Pixelfactor.IP.SavedGames.V162.Model;
using Pixelfactor.IP.SavedGames.V162.Model.Factions;
using Pixelfactor.IP.SavedGames.V162.Model.Factions.Bounty;
using Pixelfactor.IP.SavedGames.V162.Model.Jobs;
using Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers.Helpers;
using Pixelfactor.IP.SavedGames.V162.Model.Helpers;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixelfactor.IP.SavedGames.V162.BinarySerialization.Writers
{
    public class SaveGameWriter
    {
        private readonly HeaderWriter headerWriter;

        public SaveGameWriter(HeaderWriter headerWriter)
        {
            this.headerWriter = headerWriter;
        }

        public void Write(BinaryWriter writer, SavedGame savedGame)
        {
            this.headerWriter.Write(writer, savedGame.Header);
            PrintStatus("Saved header", writer);

            writer.Write(savedGame.SecondsElapsed);
            WriteSectors(writer, savedGame.Sectors);
            PrintStatus("Saved sectors", writer);

            WriteFactions(writer, savedGame.Factions);
            PrintStatus("Saved factions", writer);

            WritePatrolPaths(writer, savedGame.PatrolPaths);
            PrintStatus("Saved patrol paths", writer);

            WriteAllFactionRelations(writer, savedGame.Factions);
            PrintStatus("Saved faction relations", writer);

            WriteAllFactionOpinions(writer, savedGame.Factions);
            PrintStatus("Saved faction opinions", writer);

            WriteUnits(writer, savedGame.Units);
            PrintStatus("Saved units", writer);

            WriteNamedUnits(writer, savedGame.Units);
            PrintStatus("Saved named units", writer);

            WriteAllComponentUnits(writer, savedGame.Units);
            PrintStatus("Saved all unit components", writer);

            WriteModdedComponents(writer, savedGame.Units);
            PrintStatus("Saved modded components", writer);

            WriteUnitCapacitorCharges(writer, savedGame.Units);
            PrintStatus("Saved unit capacitor charges", writer);

            WriteCloakedUnits(writer, savedGame.Units);
            PrintStatus("Saved unit cloak states", writer);

            WritePoweredDownComponents(writer, savedGame.Units);
            PrintStatus("Saved powered down units", writer);

            WriteUnitEngineThrottles(writer, savedGame.Units);
            PrintStatus("Saved engine throttle data", writer);

            WriteComponentUnitCargo(writer, savedGame.Units);
            PrintStatus("Saved cargo", writer);

            WriteAllShieldHealthData(writer, savedGame.Units);
            PrintStatus("Saved damaged shields", writer);

            WriteAllUnitComponentHealthData(writer, savedGame.Units);
            PrintStatus("Saved damaged components", writer);

            WriteActiveUnits(writer, savedGame.Units);
            PrintStatus("Saved active units", writer);

            WriteAllUnitHealthDatas(writer, savedGame.Units);
            PrintStatus("Saved destructable units", writer);

            WriteAllFactionIntel(writer, savedGame.Factions);
            PrintStatus("Saved faction intel", writer);

            WritePassengerGroups(writer, savedGame.Units);
            PrintStatus("Saved passenger groups", writer);

            WriteWormholes(writer, savedGame.Units);
            PrintStatus("Saved wormholes", writer);

            WriteHangars(writer, savedGame.Units);
            PrintStatus("Saved hangers", writer);

            WriteTraders(writer);
            PrintStatus("Saved traders (unused)", writer);

            WriteFleets(writer, savedGame.Fleets);

            PrintStatus("Saved fleets", writer);

            WritePeople(writer, savedGame.People);
            PrintStatus("Saved people", writer);

            WriteNpcPilots(writer, savedGame.People);
            PrintStatus("Saved NPC pilots", writer);

            WriteFactionLeaders(writer, savedGame.Factions);
            PrintStatus("Saved faction leaders", writer);

            WriteJobs(writer, savedGame.Units);
            PrintStatus("Saved jobs", writer);

            WriteAllFactionAIsAndBountyBoards(writer, savedGame.Factions);
            PrintStatus("Saved faction AIs / bounty boards", writer);

            WriteFactionAIExcludedUnits(writer, savedGame.Factions);
            PrintStatus("Saved faction excluded unit data", writer);

            WriteFactionMercenaryData(writer, savedGame.Factions);
            PrintStatus("Saved mercenary data", writer);

            WriteFleetSpawners(writer, savedGame.FleetSpawners);
            PrintStatus("Saved NPC fleet spawners", writer);

            WriteActiveJobs(writer, savedGame.ActiveJobs);
            PrintStatus("Saved jobs", writer);

            writer.Write(savedGame.Player != null);
            if (savedGame.Player != null)
            {
                WriteGamePlayer(writer, savedGame.Player);
                PrintStatus("Saved player data", writer);
            }

            writer.WriteUnitId(savedGame.CurrentHudTarget);
            PrintStatus("Saved hud data", writer);

            WriteAllFactionTransactions(writer, savedGame.Factions);
            PrintStatus("Saved all faction transactions", writer);

            WriteScenarioData(writer, savedGame.ScenarioData);
            PrintStatus("Saved world", writer);

            WriteAllPlayerFleets(writer, savedGame.Player.Fleets);

            PrintStatus("Saved player fleet groups", writer);

            WritePlayerUnitFleetMap(writer, savedGame.PlayerUnitFleetMap);
            PrintStatus("Saved generic autopilot fleets", writer);

            WriteMoons(writer, savedGame.Moons);
            PrintStatus("Saved moons", writer);
        }

        private void PrintStatus(string message, BinaryWriter writer)
        {
            Console.WriteLine($"{message} - {writer.BaseStream.Position - 1} bytes read");
        }

        /// <summary>
        /// Aka moons
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="moons"></param>
        private void WriteMoons(BinaryWriter writer, IList<Moon> moons)
        {
            writer.Write(moons.Count);
            foreach (var moon in moons)
            {
                writer.WriteUnitId(moon.Unit);
                writer.WriteUnitId(moon.OrbitUnit);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="fleets"></param>
        private void WritePlayerUnitFleetMap(BinaryWriter writer, IList<PlayerUnitFleetMap> playerUnitFleetMaps)
        {
            writer.Write(playerUnitFleetMaps.Count);

            foreach (var map in playerUnitFleetMaps)
            {
                writer.WriteUnitId(map.Unit);
                writer.WriteFleetId(map.Fleet);
            }
        }

        private static void WriteAllPlayerFleets(BinaryWriter writer, IEnumerable<Fleet> playerFleets)
        {
            writer.Write(playerFleets.Count());
            foreach (var fleet in playerFleets)
            {
                writer.WriteFleetId(fleet);
            }
        }

        private void WriteScenarioData(BinaryWriter writer, ScenarioData scenarioData)
        {
            writer.Write(scenarioData.HasRandomEvents);
            if (scenarioData.HasRandomEvents)
            {
                writer.Write(scenarioData.NextRandomEventTime);
            }


            writer.Write(scenarioData.FactionSpawner != null);
            if (scenarioData.FactionSpawner != null)
            {
                writer.Write(scenarioData.FactionSpawner.NextUpdate);

                // Not used
                writer.Write(0);
            }
        }

        private void WriteAllFactionTransactions(
            BinaryWriter writer,
            IEnumerable<Faction> factions)
        {
            // Only player faction should have (or need) these but allow support for others anyway
            var factionsWithTransactions = factions.Where(e => e.Transactions?.Count > 0).ToList();
            writer.Write(factionsWithTransactions.Count);

            foreach (var faction in factionsWithTransactions)
            {
                writer.WriteFactionId(faction);
                writer.Write(faction.Transactions.Count);

                foreach (var transaction in faction.Transactions)
                {
                    WriteFactionTransaction(writer, transaction);
                }
            }
        }

        private static void WriteFactionTransaction(
            BinaryWriter writer,
            FactionTransaction transaction)
        {
            writer.Write((int)transaction.TransactionType);
            writer.Write(transaction.Value);
            writer.Write(transaction.CurrentBalance);
            writer.WriteUnitId(transaction.LocationUnit);
            writer.WriteFactionId(transaction.OtherFaction);
            writer.Write(transaction.RelatedCargoClassId);
            writer.Write((int)transaction.RelatedUnitClass);
            writer.Write(transaction.GameWorldTime);
        }

        private void WriteGamePlayer(
            BinaryWriter writer,
            Player player)
        {
            writer.Write(player.VisitedUnits.Count);
            foreach (var unit in player.VisitedUnits)
            {
                writer.WriteUnitId(unit);
            }

            writer.Write(player.Messages.Count);
            foreach (var message in player.Messages)
            {
                WritePlayerMessage(writer, message);
            }

            writer.Write(player.DelayedMessages.Count);
            foreach (var delayedMessage in player.DelayedMessages)
            {
                writer.Write(delayedMessage.ShowTime);
                WritePlayerMessage(writer, delayedMessage.Message);
            }

            WritePlayerWaypointIfSet(writer, player.CustomWaypoint);

            writer.Write(player.ActiveJob?.Id ?? -1);
        }

        private static void WritePlayerWaypointIfSet(
            BinaryWriter writer,
            PlayerWaypoint playerWaypoint)
        {
            writer.Write(playerWaypoint != null);
            if (playerWaypoint != null)
            {
                WritePlayerWaypoint(writer, playerWaypoint);
            }
        }

        public static void WritePlayerWaypoint(
            BinaryWriter writer,
            PlayerWaypoint waypoint)
        {
            writer.WriteVector3(waypoint.Position);
            writer.WriteSectorId(waypoint.Sector);
            writer.WriteUnitId(waypoint.TargetUnit);
            writer.Write(waypoint.HadTargetObject);
        }

        public static void WritePlayerMessage(
            BinaryWriter writer,
            PlayerMessage message)
        {
            writer.Write(message.Id);
            writer.Write(message.EngineTimeStamp);
            writer.Write(message.AllowDelete);
            writer.Write(message.Opened);
            writer.WriteUnitId(message.SenderUnit);
            writer.WriteUnitId(message.SubjectUnit);

            writer.Write(message.MessageTemplateId > -1);
            if (message.MessageTemplateId > -1)
            {
                writer.Write(message.MessageTemplateId);
            }
            else
            {
                writer.WriteStringOrEmpty(message.ToText);
                writer.WriteStringOrEmpty(message.FromText);
                writer.WriteStringOrEmpty(message.MessageText);
                writer.WriteStringOrEmpty(message.SubjectText);
            }
        }

        private void WriteActiveJobs(
            BinaryWriter writer,
            IList<ActiveJob> activeJobs)
        {
            writer.Write(activeJobs.Count);
            foreach (var activeJob in activeJobs)
            {
                ActiveJobWriter.Write(writer, activeJob);
            }
        }

        private void WriteFleetSpawners(
            BinaryWriter writer,
            IList<FleetSpawner> fleetSpawners)
        {
            writer.Write(fleetSpawners.Count);
            foreach (var fleetSpawner in fleetSpawners)
            {
                FleetSpawnerWriter.Write(
                    writer,
                    fleetSpawner);
            }
        }

        private void WriteFactionMercenaryData(BinaryWriter writer, IEnumerable<Faction> factions)
        {
            var factionsToWrite = factions.Where(e => e.FactionAI?.FactionMercenaryHireInfo != null).ToList();
            writer.Write(factionsToWrite.Count);

            foreach (var faction in factionsToWrite)
            {
                writer.WriteFactionId(faction);
                writer.WriteFactionId(faction.FactionAI.FactionMercenaryHireInfo.HiringFaction);
                writer.Write(faction.FactionAI.FactionMercenaryHireInfo.HireTimeExpiry);
            }
        }

        private void WriteFactionAIExcludedUnits(
            BinaryWriter writer,
            IEnumerable<Faction> factions)
        {
            var factionsToWrite = factions.Where(e => e.FactionAI?.ExcludedUnits?.Count > 0).ToList();
            writer.Write(factionsToWrite.Count);
            foreach (var faction in factionsToWrite)
            {
                foreach (var unit in faction.FactionAI.ExcludedUnits)
                {
                    writer.WriteFactionId(faction);
                    writer.WriteUnitId(unit);
                }
            }
        }

        private void WriteAllFactionAIsAndBountyBoards(
            BinaryWriter writer,
            IList<Faction> factions)
        {
            writer.Write(factions.Count);
            foreach (var faction in factions)
            {
                writer.WriteFactionId(faction);
                writer.Write(faction.FactionAI != null);

                if (faction.FactionAI != null)
                {
                    writer.Write((int)faction.FactionAI.AIType);
                    FactionAIWriter.Write(writer, faction.FactionAI.AIType, faction.FactionAI);
                }

                writer.Write(faction.BountyBoard != null);
                if (faction.BountyBoard != null)
                {
                    WriteFactionBountyBoard(writer, faction.BountyBoard);
                }
            }
        }

        private static void WriteFactionBountyBoard(
            BinaryWriter writer,
            FactionBountyBoard bountyBoard)
        {
            writer.Write(bountyBoard.Items.Count);
            foreach (var item in bountyBoard.Items)
            {
                WriteFactionBountyBoardItem(writer, item);
            }
        }

        private static void WriteFactionBountyBoardItem(
            BinaryWriter writer,
            FactionBountyBoardItem item)
        {
            writer.WritePersonId(item.TargetPerson);
            writer.Write(item.Reward);
            writer.WriteUnitId(item.LastKnownTargetUnit);
            writer.WriteSectorId(item.LastKnownTargetSector);
            writer.WriteNullableVector3(item.LastKnownTargetPosition);
            writer.WriteFactionId(item.SourceFaction);
        }

        private void WriteJobs(
            BinaryWriter writer,
            IEnumerable<Unit> units)
        {
            var jobs = units.SelectMany(e => e.Jobs).ToList();
            writer.Write(jobs.Count);
            foreach (var job in jobs)
            {
                writer.Write((int)job.JobType);
                JobWriter.Write(writer, job);
            }
        }

        private void WriteFactionLeaders(BinaryWriter writer, IEnumerable<Faction> factions)
        {
            var factionsWithLeaders = factions.Where(e => e.Leader != null).ToList();
            writer.Write(factionsWithLeaders.Count);
            foreach (var faction in factionsWithLeaders)
            {
                writer.WriteFactionId(faction);
                writer.WritePersonId(faction.Leader);
            }
        }

        private void WriteNpcPilots(BinaryWriter writer, List<Person> people)
        {
            var npcPilots = people.Where(e => e.NpcPilot != null).ToList();
            writer.Write(npcPilots.Count);
            foreach (var npcPilot in npcPilots)
            {
                writer.WritePersonId(npcPilot);
                WriteNpcPilot(writer, npcPilot.NpcPilot);
            }
        }

        private void WriteNpcPilot(BinaryWriter writer, NpcPilot npcPilot)
        {
            writer.Write(npcPilot.AllowUseCloak);
            writer.Write(npcPilot.DestroyWhenNoUnit);
            writer.Write(npcPilot.DestroyWhenNotPilotting);
            writer.WriteFleetId(npcPilot.Fleet);
        }

        private void WritePeople(BinaryWriter writer, IList<Person> people)
        {
            writer.Write(people.Count);

            foreach (var person in people)
            {
                WritePerson(writer, person);
            }
        }

        private static void WritePerson(
            BinaryWriter writer,
            Person person)
        {
            writer.WritePersonId(person);

            writer.Write(string.IsNullOrWhiteSpace(person.CustomName));
            if (string.IsNullOrWhiteSpace(person.CustomName))
            {
                writer.Write(person.GeneratedFirstNameId);
                writer.Write(person.GeneratedLastNameId);
            }
            else
            {
                writer.WriteStringOrEmpty(person.CustomName);
            }

            writer.Write(person.DialogId);
            writer.Write(person.IsMale);
            writer.WriteFactionId(person.Faction);
            writer.Write(person.DestroyGameObjectOnKill);
            writer.WriteUnitId(person.CurrentUnit);
            writer.Write(person.IsPilot);
            writer.Write(person.Kills);
            writer.Write(person.IsPlayer);
            writer.Write(person.NpcPilotSettings != null);
            if (person.NpcPilotSettings != null)
            {
                WriteNpcPilotSettings(writer, person.NpcPilotSettings);
            }
        }

        private static void WriteNpcPilotSettings(BinaryWriter writer, NpcPilotSettings settings)
        {
            writer.Write(settings.UsesCloak);
            writer.Write(settings.UseCloakPreference);
            writer.Write(settings.EnterCombatCloakedProbability);
            writer.Write(settings.RestrictedWeaponPreference);
            writer.Write(settings.CombatEfficiency);
            writer.Write(settings.CheatAmmo);
        }

        private void WriteFleets(
            BinaryWriter writer,
            IList<Fleet> fleets)
        {
            writer.Write(fleets.Count);
            foreach (var fleet in fleets)
            {
                WriteFleet(writer, fleet);
            }
        }

        private void WriteFleet(
            BinaryWriter writer,
            Fleet fleet)
        {
            writer.Write(fleet.IsActive);
            writer.Write(fleet.Id);

            writer.WriteVector3(fleet.Position);
            writer.WriteVector4(fleet.Rotation);
            writer.WriteSectorId(fleet.Sector);
            writer.WriteFactionId(fleet.Faction);
            writer.WriteUnitId(fleet.HomeBaseUnit);
            writer.Write(fleet.ExcludeFromFactionAI);

            writer.Write(fleet.FleetSettings != null);
            if (fleet.FleetSettings != null)
            {
                WriteFleetSettings(writer, fleet.FleetSettings);
            }

            FleetOrdersWriter.Write(writer, fleet.Orders);
        }

        private void WriteFleetSettings(BinaryWriter writer, FleetSettings settings)
        {
            writer.Write(settings.ControllersCanCollectCargo);
            writer.Write(settings.ControllersCollectOnlyEquipment);
            writer.Write(settings.PreferCloak);
            writer.Write(settings.PreferToDock);
            writer.Write(settings.NotifyWhenOrderComplete);
            writer.Write(settings.AttackTargetScoreThreshold);
            writer.Write(settings.AllowAttack);
            writer.Write(settings.TargetInterceptionLowerDistance);
            writer.Write(settings.TargetInterceptionUpperDistance);
            writer.Write(settings.RestrictMaxJumps);
            writer.Write(settings.MaxJumpDistance);
            writer.Write(settings.AllowCombatInterception);
            writer.Write(settings.DestroyWhenNoPilots);
        }

        /// <summary>
        /// No longer used
        /// </summary>
        /// <param name="writer"></param>
        private void WriteTraders(BinaryWriter writer)
        {
            // No longer used
            writer.Write(0);
        }

        private void WriteHangars(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var unitsWithDockedShips = units
                .Where(e => e.ComponentUnitData?.DockData != null)
                .ToList();

            writer.Write(unitsWithDockedShips.Count);

            foreach (var unit in unitsWithDockedShips)
            {
                WriteHangerItems(writer, unit, unit.ComponentUnitData.DockData);
            }
        }

        private void WriteHangerItems(BinaryWriter writer, Unit unit, ComponentUnitDockData dockData)
        {
            writer.WriteUnitId(unit);
            writer.Write(dockData.Items.Count);

            foreach (var dockedShip in dockData.Items)
            {
                writer.Write(dockedShip.BayId);
                writer.WriteUnitId(dockedShip.DockedUnit);
            }
        }

        private void WriteWormholes(BinaryWriter writer, IEnumerable<Unit> allUnits)
        {
            var wormholes = allUnits.Where(e => e.WormholeData != null).ToList();
            writer.Write(wormholes.Count);
            foreach (var wormhole in wormholes)
            {
                writer.WriteUnitId(wormhole);
                WriteWormholeData(writer, wormhole.WormholeData);
            }
        }

        private void WriteWormholeData(BinaryWriter writer, UnitWormholeData wormholeData)
        {
            writer.WriteUnitId(wormholeData.TargetWormholeUnit);
            writer.Write(wormholeData.IsUnstable);
            writer.Write(wormholeData.UnstableNextChangeTargetTime);
            writer.WriteVector3(wormholeData.UnstableTargetPosition);
            writer.WriteVector3(wormholeData.UnstableTargetRotation);
            writer.WriteSectorId(wormholeData.UnstableTargetSector);
        }

        private void WritePassengerGroups(BinaryWriter writer, IList<Unit> units)
        {
            var passengerGroups = units.SelectMany(e => e.PassengerGroups).ToList();
            writer.Write(passengerGroups.Count);
            foreach (var passengerGroup in passengerGroups)
            {
                WritePassengerGroup(writer, passengerGroup);
            }
        }

        private void WritePassengerGroup(BinaryWriter writer, PassengerGroup passengerGroup)
        {
            writer.Write(passengerGroup.Id);
            writer.WriteUnitId(passengerGroup.Unit);
            writer.WriteUnitId(passengerGroup.SourceUnit);
            writer.WriteUnitId(passengerGroup.DestinationUnit);
            writer.Write(passengerGroup.PassengerCount);
            writer.Write(passengerGroup.ExpiryTime);
            writer.Write(passengerGroup.Revenue);
        }

        private void WriteAllFactionIntel(BinaryWriter writer, IEnumerable<Faction> factions)
        {
            var factionsWithIntel = factions.Where(e => e.Intel != null).ToList();
            writer.Write(factionsWithIntel.Count);
            foreach (var faction in factionsWithIntel)
            {
                writer.WriteFactionId(faction);
                WriteFactionIntel(writer, faction.Intel);
            }
        }

        private void WriteFactionIntel(BinaryWriter writer, FactionIntel factionIntel)
        {
            writer.Write(factionIntel.Sectors.Count);
            foreach (var sector in factionIntel.Sectors)
            {
                writer.WriteSectorId(sector);
            }

            writer.Write(factionIntel.Units.Count);
            foreach (var unit in factionIntel.Units)
            {
                writer.WriteUnitId(unit);
            }
        }

        private void WriteAllUnitHealthDatas(BinaryWriter writer, List<Unit> units)
        {
            var unitsWithHealthData = units.Where(e => e.HealthData != null).ToList();
            writer.Write(unitsWithHealthData.Count);
            foreach (var unit in unitsWithHealthData)
            {
                writer.WriteUnitId(unit);
                WriteUnitHealthData(writer, unit.HealthData);
            }
        }

        private void WriteUnitHealthData(BinaryWriter writer, UnitHealthData healthData)
        {
            writer.Write(healthData.IsDestroyed);
            writer.Write(healthData.TotalDamagedReceived);
            writer.Write(healthData.Health);
        }

        private void WriteActiveUnits(BinaryWriter writer, List<Unit> units)
        {
            var activeUnits = units.Where(e => e.ComponentUnitData?.ActiveData != null).ToList();

            writer.Write(activeUnits.Count);
            foreach (var unit in activeUnits)
            {
                writer.WriteUnitId(unit);
                WriteActiveUnitData(writer, unit.ComponentUnitData.ActiveData);
            }
        }

        private void WriteActiveUnitData(BinaryWriter writer, ComponentUnitActiveData activeData)
        {
            writer.WriteVector3(activeData.Velocity);
            writer.Write(activeData.CurrentTurn);
        }

        private void WriteAllUnitComponentHealthData(BinaryWriter writer, List<Unit> units)
        {
            var unitsToWrite = units.Where(e => e.ComponentUnitData?.ComponentHealthData?.Items.Count > 0).ToList();
            writer.Write(unitsToWrite.Count);
            foreach (var unit in unitsToWrite)
            {
                writer.WriteUnitId(unit);
                WriteUnitComponentHealthData(writer, unit.ComponentUnitData.ComponentHealthData);
            }
        }

        private void WriteUnitComponentHealthData(BinaryWriter writer, ComponentUnitComponentHealthData componentHealthData)
        {
            writer.Write(componentHealthData.Items.Count);
            foreach (var item in componentHealthData.Items)
            {
                writer.Write(item.BayId);
                writer.Write(item.Health);
            }
        }

        private void WriteAllShieldHealthData(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var unitsWithShieldHealthData = units.Where(e => e.ComponentUnitData?.ShieldData?.Items.Count > 0).ToList();
            writer.Write(unitsWithShieldHealthData.Count);
            foreach (var unit in unitsWithShieldHealthData)
            {
                writer.WriteUnitId(unit);
                WriteShieldHealthData(writer, unit.ComponentUnitData.ShieldData);
            }
        }

        private void WriteShieldHealthData(BinaryWriter writer, ComponentUnitShieldHealthData shieldHealthData)
        {
            writer.Write((byte)shieldHealthData.Items.Count);
            foreach (var item in shieldHealthData.Items)
            {
                writer.Write((byte)item.ShieldPointIndex);
                writer.Write(item.Health);
            }
        }

        private void WriteComponentUnitCargo(BinaryWriter writer, List<Unit> units)
        {
            var unitsWithCargo = units.Where(e => e.ComponentUnitData?.CargoData != null).ToList();
            writer.Write(unitsWithCargo.Count);
            foreach (var unit in unitsWithCargo)
            {
                writer.WriteUnitId(unit);
                WriteComponentUnitCargoData(writer, unit.ComponentUnitData.CargoData);
            }
        }

        private void WriteComponentUnitCargoData(BinaryWriter writer, ComponentUnitCargoData cargoData)
        {
            writer.Write(cargoData.Items.Count);
            foreach (var item in cargoData.Items)
            {
                ComponentUnitCargoDataItemWriter.Write(writer, item);
            }
        }

        private void WriteUnitEngineThrottles(BinaryWriter writer, List<Unit> units)
        {
            var unitsWithEngineThrottle = units.Where(e => e.ComponentUnitData?.EngineThrottle.HasValue ?? false).ToList();
            writer.Write(unitsWithEngineThrottle.Count);
            foreach (var unit in unitsWithEngineThrottle)
            {
                writer.WriteUnitId(unit);
                writer.Write(unit.ComponentUnitData.EngineThrottle.Value);
            }
        }

        private void WritePoweredDownComponents(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var poweredDownUnits = units.Where(e => e.ComponentUnitData?.PoweredDownBayIds?.Count > 0).ToList();
            writer.Write(poweredDownUnits.Count);
            foreach (var unit in poweredDownUnits)
            {
                foreach (var bayId in unit.ComponentUnitData.PoweredDownBayIds)
                {
                    writer.WriteUnitId(unit);
                    writer.Write(bayId);
                }
            }
        }

        private void WriteCloakedUnits(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var cloakedUnits = units.Where(e => e.ComponentUnitData?.IsCloaked ?? false).ToList();
            writer.Write(cloakedUnits.Count);
            foreach (var unit in cloakedUnits)
            {
                writer.WriteUnitId(unit);
            }
        }

        private void WriteUnitCapacitorCharges(BinaryWriter writer, List<Unit> units)
        {
            var capactorChargeUnits = units.Where(e => e.ComponentUnitData?.CapacitorCharge.HasValue ?? false).ToList();
            writer.Write(capactorChargeUnits.Count);
            foreach (var capacitorChargeUnit in capactorChargeUnits)
            {
                writer.WriteUnitId(capacitorChargeUnit);
                writer.Write(capacitorChargeUnit.ComponentUnitData.CapacitorCharge.Value);
            }
        }

        private void WriteModdedComponents(BinaryWriter writer, List<Unit> units)
        {
            var moddedUnits = units.Where(e => e.ComponentUnitData?.ModData?.Items.Count > 0).ToList();
            writer.Write(moddedUnits.SelectMany(e => e.ComponentUnitData.ModData.Items).Count());

            foreach (var moddedUnit in moddedUnits)
            {
                foreach (var item in moddedUnit.ComponentUnitData.ModData.Items)
                {
                    writer.WriteUnitId(moddedUnit);
                    writer.Write(item.BayId);
                    writer.Write(item.ComponentClassId);
                }
            }
        }

        private void WriteAllComponentUnits(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var componentUnits = units.Where(e => e.ComponentUnitData != null).ToList();
            writer.Write(componentUnits.Count);

            foreach (var componentUnit in componentUnits)
            {
                writer.Write(componentUnit.Id);
                WriteComponentUnitData(writer, componentUnit.ComponentUnitData);
            }
        }

        private void WriteComponentUnitData(BinaryWriter writer, ComponentUnitData componentUnitData)
        {
            writer.Write(componentUnitData.ShipNameIndex);

            if (componentUnitData.ShipNameIndex == -1)
            {
                writer.WriteStringOrEmpty(componentUnitData.CustomShipName);
            }

            writer.Write(componentUnitData.CargoCapacity);

            writer.Write(componentUnitData.FactoryData != null);
            if (componentUnitData.FactoryData != null)
            {
                WriteComponentUnitFactoryData(writer, componentUnitData.FactoryData);
            }

            writer.Write(componentUnitData.IsUnderConstruction);
            writer.Write(componentUnitData.ConstructionProgress);
            writer.Write(componentUnitData.StationUnitClassNumber);
        }

        private void WriteComponentUnitFactoryData(BinaryWriter writer, ComponentUnitFactoryData factoryData)
        {
            writer.Write(factoryData.Items.Count);
            foreach (var item in factoryData.Items)
            {
                writer.Write((int)item.State);
                writer.Write(item.ProductionElapsed);
            }
        }

        private void WriteNamedUnits(BinaryWriter writer, IEnumerable<Unit> units)
        {
            var namedUnits = units.Where(e => !string.IsNullOrWhiteSpace(e.Name)).ToList();
            writer.Write(namedUnits.Count);
            foreach (var unit in namedUnits)
            {
                writer.WriteUnitId(unit);
                writer.WriteStringOrEmpty(unit.Name);
            }
        }

        private void WriteUnits(BinaryWriter writer, IList<Unit> units)
        {
            writer.Write(units.Count);
            foreach (var unit in units)
            {
                WriteUnit(writer, unit);
            }
        }

        private void WriteUnit(BinaryWriter writer, Unit unit)
        {
            writer.Write(unit.Id);
            writer.Write((int)unit.Class);
            writer.WriteSectorId(unit.Sector);
            writer.WriteVector3(unit.Position);
            writer.WriteVector4(unit.Rotation);
            writer.WriteFactionId(unit.Faction);
            writer.Write(unit.RpProvision);
            writer.Write(unit.CargoData != null);
            if (unit.CargoData != null)
            {
                WriteUnitCargoData(writer, unit.CargoData);
            }

            writer.Write(unit.DebrisData != null);
            if (unit.DebrisData != null)
            {
                WriteUnitDebrisData(writer, unit.DebrisData);
            }

            writer.Write(unit.ShipTraderData != null);
            if (unit.ShipTraderData != null)
            {
                WriteShipTrader(writer, unit.ShipTraderData);
            }

            if (UnitHelper.IsProjectile(unit.Class))
            {
                writer.Write(unit.ProjectileData != null);
                if (unit.ProjectileData != null)
                {
                    WriteUnitProjectileData(writer, unit.ProjectileData);
                }
            }
        }

        private void WriteUnitProjectileData(BinaryWriter writer, UnitProjectileData projectileData)
        {
            writer.WriteUnitId(projectileData.SourceUnit);
            writer.WriteUnitId(projectileData.TargetUnit);
            writer.Write(projectileData.FireTime);
            writer.Write(projectileData.RemainingMovement);
            writer.WriteDamageType(projectileData.DamageType);
        }

        private void WriteShipTrader(BinaryWriter writer, UnitShipTraderData shipTraderData)
        {
            writer.Write(shipTraderData.Items.Count);

            foreach (var item in shipTraderData.Items)
            {
                writer.Write(item.SellMultiplier);
                writer.Write(item.UnitClassId);
            }
        }

        private void WriteUnitDebrisData(BinaryWriter writer, UnitDebrisData debrisData)
        {
            writer.Write(debrisData.ScrapQuantity);
            writer.Write(debrisData.Expires);
            writer.Write(debrisData.ExpiryTime);
            writer.Write(debrisData.RelatedUnitClassId);
        }

        private void WriteUnitCargoData(BinaryWriter writer, UnitCargoData unitCargoData)
        {
            writer.Write(unitCargoData.CargoClassId);
            writer.Write(unitCargoData.Quantity);
            writer.Write(unitCargoData.Expires);
            writer.Write(unitCargoData.ExpiryTime);
        }

        private void WriteFactions(BinaryWriter writer, IList<Faction> factions)
        {
            writer.Write(factions.Count);
            foreach (var faction in factions)
            {
                WriteFaction(writer, faction);
            }
        }

        private void WriteSectors(BinaryWriter writer, List<Sector> sectors)
        {
            writer.Write(sectors.Count);
            foreach (var sector in sectors)
            {
                WriteSector(writer, sector);
            }
        }

        private void WritePatrolPaths(BinaryWriter writer, IList<SectorPatrolPath> patrolPaths)
        {
            writer.Write(patrolPaths.Count);
            foreach (var patrolPath in patrolPaths)
            {
                WritePatrolPath(writer, patrolPath);
            }
        }

        private void WriteAllFactionRelations(BinaryWriter writer, IEnumerable<Faction> factions)
        {
            var factionWithRelations = factions.Where(e => e.Relations?.Items.Count > 0).ToList();

            writer.Write(factionWithRelations.Count);

            foreach (var faction in factionWithRelations)
            {
                writer.WriteFactionId(faction);
                WriteFactionRelationData(writer, faction.Relations);
            }
        }

        private void WriteFactionRelationData(BinaryWriter writer, FactionRelationData relationData)
        {
            writer.Write(relationData.Items.Count);
            foreach (var item in relationData.Items)
            {
                WriteFactionRelationDataItem(writer, item);
            }
        }

        private void WriteFactionRelationDataItem(BinaryWriter writer, FactionRelationDataItem relation)
        {
            writer.WriteFactionId(relation.OtherFaction);
            writer.Write(relation.PermanentPeace);
            writer.Write(relation.RestrictHostilityTimeout);
            writer.Write((int)relation.Neutrality);
            writer.Write(relation.HostilityEndTime);
            writer.Write(relation.RecentDamageReceived);
        }

        private void WriteAllFactionOpinions(BinaryWriter writer, IEnumerable<Faction> factions)
        {
            var factionOpinions = factions.Where(e => e.Opinions?.Items.Count > 0).Select(e => new { Faction = e, Items = e.Opinions.Items }).ToList();
            writer.Write(factionOpinions.SelectMany(e => e.Items).Count());
            foreach (var faction in factionOpinions)
            {
                foreach (var item in faction.Items)
                {
                    writer.WriteFactionId(faction.Faction);
                    writer.WriteFactionId(item.OtherFaction);
                    writer.Write(item.Opinion);
                }
            }
        }

        private void WriteSector(BinaryWriter writer, Sector sector)
        {
            writer.Write(sector.Id);
            writer.WriteStringOrEmpty(sector.Name);
            writer.WriteVector3(sector.MapPosition);
            writer.WriteStringOrEmpty(sector.ResourceName);
            writer.WriteStringOrEmpty(sector.Description);
            writer.Write(sector.GateDistanceMultiplier);
            writer.Write(sector.RandomSeed);
            writer.WriteVector3(sector.Position);
            writer.WriteVector3(sector.BackgroundRotation);
            writer.WriteVector3(sector.LightRotation);
        }

        private void WriteFaction(BinaryWriter writer, Faction faction)
        {
            writer.Write(faction.Id);
            writer.Write(faction.HasGeneratedName);
            if (faction.HasGeneratedName)
            {
                writer.Write(faction.GeneratedNameId);
                writer.Write(faction.GeneratedSuffixId);
            }
            else
            {
                writer.Write(faction.HasCustomName);
                if (faction.HasCustomName)
                {
                    writer.WriteStringOrEmpty(faction.CustomName);
                    writer.WriteStringOrEmpty(faction.CustomShortName);
                }
            }

            writer.Write(faction.Credits);
            writer.WriteStringOrEmpty(faction.Description);
            writer.Write(faction.IsCivilian);
            writer.Write((int)faction.FactionType);
            writer.Write(faction.Aggression);
            writer.Write(faction.Virtue);
            writer.Write(faction.Greed);
            writer.Write(faction.TradeEfficiency);
            writer.Write(faction.DynamicRelations);
            writer.Write(faction.ShowJobBoards);
            writer.Write(faction.CreateJobs);
            writer.Write(faction.RequisitionPointMultiplier);
            writer.Write(faction.DestroyWhenNoUnits);
            writer.Write(faction.MinNpcCombatEfficiency);
            writer.Write(faction.MaxNpcCombatEfficiency);
            writer.Write(faction.AdditionalRpProvision);
            writer.Write(faction.TradeIllegalGoods);
            writer.Write(faction.SpawnTime);
            writer.Write(faction.HighestEverNetWorth);

            writer.Write(faction.CustomSettings != null);
            if (faction.CustomSettings != null)
            {
                WriteFactionCustomSettings(writer, faction.CustomSettings);
            }

            writer.Write(faction.Stats != null);
            if (faction.Stats != null)
            {
                WriteFactionStats(writer, faction.Stats);
            }

            writer.Write(faction.AutopilotExcludedSectors.Count);
            foreach (var sector in faction.AutopilotExcludedSectors)
            {
                writer.WriteSectorId(sector);
            }
        }

        private void WriteFactionCustomSettings(BinaryWriter writer, FactionCustomSettings settings)
        {
            writer.Write(settings.PreferSingleShip);
            writer.Write(settings.RepairShips);
            writer.Write(settings.UpgradeShips);
            writer.Write(settings.RepairMinHullDamage);
            writer.Write(settings.RepairMinCreditsBeforeRepair);
            writer.Write(settings.PreferenceToPlaceBounty);
            writer.Write(settings.LargeShipPreference);
            writer.Write(settings.DailyIncome);
            writer.Write(settings.HostileWithAll);
            writer.Write(settings.MinFleetUnitCount);
            writer.Write(settings.MaxFleetUnitCount);
            writer.Write(settings.OffensiveStance);
            writer.Write(settings.AllowOtherFactionToUseDocks);
            writer.Write(settings.PreferenceToBuildTurrets);
            writer.Write(settings.PreferenceToBuildStations);
            writer.Write(settings.IgnoreStationCreditsReserve);
        }

        private void WriteFactionStats(BinaryWriter writer, FactionStats factionStats)
        {
            writer.Write(factionStats.TotalShipsClaimed);
            WriteFactionStatsUnitCounts(writer, factionStats.UnitsDestroyedByClassId);
            WriteFactionStatsUnitCounts(writer, factionStats.UnitLostByClassId);
            writer.Write(factionStats.ScratchcardsScratched);
            writer.Write(factionStats.HighestScratchcardWin);
        }

        private void WriteFactionStatsUnitCounts(BinaryWriter writer, Dictionary<int, int> unitCountsByClass)
        {
            writer.Write(unitCountsByClass.Count);
            foreach (var item in unitCountsByClass)
            {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        private void WritePatrolPath(BinaryWriter writer, SectorPatrolPath path)
        {
            writer.Write(path.Id);
            writer.WriteSectorId(path.Sector);
            writer.Write(path.IsLoop);
            writer.Write(path.Nodes.Count);
            foreach (var node in path.Nodes)
            {
                writer.WriteVector3(node.Position);
                writer.Write(node.Order);
            }
        }
    }
}
