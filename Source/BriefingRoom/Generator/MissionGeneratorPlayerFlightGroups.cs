﻿/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCS.Data;
using BriefingRoom4DCS.Mission;
using BriefingRoom4DCS.Template;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BriefingRoom4DCS.Generator
{
    internal class MissionGeneratorPlayerFlightGroups
    {

        internal static void GeneratePlayerFlightGroup(
            UnitMaker unitMaker,
            DCSMission mission,
            MissionTemplateRecord template,
            MissionTemplateFlightGroupRecord flightGroup,
            DBEntryAirbase playerAirbase,
            List<Waypoint> waypoints,
            Dictionary<string, UnitMakerGroupInfo> carrierDictionary,
            Coordinates averageInitialLocation,
            Coordinates objectivesCenter)
        {
            DBEntryAirbase airbase = playerAirbase;
            List<Waypoint> flightWaypoints = new List<Waypoint>(waypoints);
            Coordinates groupStartingCoords = playerAirbase.Coordinates;

            var package = template.AircraftPackages.FirstOrDefault(x => x.FlightGroupIndexes.Contains(template.PlayerFlightGroups.IndexOf(flightGroup)));
            if (package is not null)
            {
                var missionPackage = mission.MissionPackages.First(x => x.RecordIndex == template.AircraftPackages.IndexOf(package));
                flightWaypoints = missionPackage.Waypoints;
                airbase = missionPackage.Airbase;
                groupStartingCoords = missionPackage.Airbase.Coordinates;
            }
            DBEntryUnit unitDB = Database.Instance.GetEntry<DBEntryUnit>(flightGroup.Aircraft);

            // Not an unit, or not a player-controllable unit, abort.
            if ((unitDB == null) || !unitDB.AircraftData.PlayerControllable)
                throw new BriefingRoomException($"Player flight group unit {flightGroup.Aircraft} does not exist or is not player-controllable.");
            if (unitDB.AircraftData.MinimumRunwayLengthFt > 0 && airbase.RunwayLengthFt < unitDB.AircraftData.MinimumRunwayLengthFt)
                BriefingRoom.PrintToLog($"Runway at {airbase.Name}({airbase.RunwayLengthFt}ft) is shorter than {unitDB.UIDisplayName}({unitDB.AircraftData.MinimumRunwayLengthFt}ft) required runway length.", LogMessageErrorLevel.Warning);

            List<int> parkingSpotIDsList = new List<int>();
            List<Coordinates> parkingSpotCoordinatesList = new List<Coordinates>();
            var groupLuaFile = "GroupAircraftPlayer";
            var carrierUnitID = 0;
            string carrierName = null;
            var side = flightGroup.Hostile ? Side.Enemy : Side.Ally;
            var country = flightGroup.Country;
            var payload = flightGroup.Payload;
            var extraSettings = new Dictionary<string, object>();
            UnitMakerGroupFlags unitMakerGroupFlags = flightGroup.AIWingmen ? UnitMakerGroupFlags.FirstUnitIsClient : 0;
            DCSSkillLevel skillLevel = flightGroup.AIWingmen ? Toolbox.RandomFrom(DCSSkillLevel.High, DCSSkillLevel.Excellent) : DCSSkillLevel.Client;

            if (!string.IsNullOrEmpty(flightGroup.Carrier) && carrierDictionary.ContainsKey(flightGroup.Carrier) && !flightGroup.Hostile) // Carrier take off
            {
                var carrier = carrierDictionary[flightGroup.Carrier];
                if(carrier.UnitDB.Families.Contains(UnitFamily.ShipCarrierSTOVL) && flightGroup.Carrier != "LHA_Tarawa")
                {   
                    extraSettings.AddIfKeyUnused("Speed", 0);
                    unitMakerGroupFlags =  0;
                    skillLevel =  DCSSkillLevel.Client;
                    if(flightGroup.Aircraft == "AV8BNA")
                        payload = "EMPTY";
                }
                groupLuaFile = "GroupAircraftPlayerCarrier";
                carrierUnitID = carrier.UnitsID[0];
                carrierName = carrier.UnitDB.UIDisplayName;

                for (int i = 0; i < flightGroup.Count; i++)
                {
                    parkingSpotIDsList.Add(i + 1);
                    parkingSpotCoordinatesList.Add(carrier.Coordinates);
                }
                groupStartingCoords = carrier.Coordinates;

            }
            else if (flightGroup.Hostile)
            {
                var coalition = GeneratorTools.GetSpawnPointCoalition(template, side, true);
                var (hostileAirbase, hostileParkingSpotIDsList, hostileParkingSpotCoordinatesList) = unitMaker.SpawnPointSelector.GetAirbaseAndParking(template, objectivesCenter, flightGroup.Count, coalition.Value, unitDB.Families.First());
                parkingSpotIDsList = hostileParkingSpotIDsList;
                parkingSpotCoordinatesList = hostileParkingSpotCoordinatesList;
                groupStartingCoords = hostileParkingSpotCoordinatesList.First();
                airbase = hostileAirbase;

                if (country == Country.CJTFBlue || country == Country.CJTFRed)
                    country = coalition == Coalition.Blue ? Country.CJTFBlue : Country.CJTFRed;

            }
            else // Land airbase take off
            {
                var parkingSpots = unitMaker.SpawnPointSelector.GetFreeParkingSpots(airbase.DCSID, flightGroup.Count, unitDB.Families[0]);
                parkingSpotIDsList = parkingSpots.Select(x => x.DCSID).ToList();
                parkingSpotCoordinatesList = parkingSpots.Select(x => x.Coordinates).ToList();
                groupStartingCoords = parkingSpotCoordinatesList.First();
            }



           

            extraSettings.AddIfKeyUnused("Payload",payload);
            extraSettings.AddIfKeyUnused("Skill",skillLevel);
            extraSettings.AddIfKeyUnused("PlayerStartingAction",GeneratorTools.GetPlayerStartingAction(flightGroup.StartLocation));
            extraSettings.AddIfKeyUnused("PlayerStartingType",GeneratorTools.GetPlayerStartingType(flightGroup.StartLocation));
            extraSettings.AddIfKeyUnused("Country",country);
            extraSettings.AddIfKeyUnused("InitialWPName",Database.Instance.Common.Names.WPInitialName);
            extraSettings.AddIfKeyUnused("FinalWPName",Database.Instance.Common.Names.WPFinalName);
            extraSettings.AddIfKeyUnused("ParkingID",parkingSpotIDsList.ToArray());
            extraSettings.AddIfKeyUnused("PlayerWaypoints",GenerateFlightPlanLua(flightWaypoints));
            extraSettings.AddIfKeyUnused("LastPlayerWaypointIndex",flightWaypoints.Count + 2);
            extraSettings.AddIfKeyUnused("LinkUnit",carrierUnitID);
            extraSettings.AddIfKeyUnused("UnitX",(from Coordinates coordinates in parkingSpotCoordinatesList select coordinates.X).ToArray());
            extraSettings.AddIfKeyUnused("UnitY",(from Coordinates coordinates in parkingSpotCoordinatesList select coordinates.Y).ToArray());
            extraSettings.AddIfKeyUnused("MissionAirbaseX",groupStartingCoords.X);
            extraSettings.AddIfKeyUnused("MissionAirbaseY",groupStartingCoords.Y);
            extraSettings.AddIfKeyUnused("MissionAirbaseID",airbase.DCSID);
            extraSettings.AddIfKeyUnused("Livery",flightGroup.Livery);

            UnitMakerGroupInfo? groupInfo = unitMaker.AddUnitGroup(
                Enumerable.Repeat(flightGroup.Aircraft, flightGroup.Count).ToArray(), side, unitDB.Families[0],
                groupLuaFile, "UnitAircraftParked", groupStartingCoords,
                unitMakerGroupFlags,
                extraSettings.ToArray()
                );

            if (!groupInfo.HasValue)
            {
                BriefingRoom.PrintToLog("Failed to generate player flight group.", LogMessageErrorLevel.Warning);
                return;
            }

            SaveFlightGroup(mission, groupInfo, flightGroup, unitDB, carrierName ?? airbase.Name);
            SaveWaypointsToBriefing(
                mission,
                groupStartingCoords,
                flightWaypoints,
                template.OptionsMission.Contains("ImperialUnitsForBriefing"),
                groupInfo);
        }

        private static string GenerateFlightPlanLua(List<Waypoint> waypoints)
        {
            string flightPlanLua = "";
            string waypointLuaTemplate = File.ReadAllText($"{BRPaths.INCLUDE_LUA_MISSION}WaypointPlayer.lua");

            for (int i = 0; i < waypoints.Count; i++)
            {
                string waypointLua = waypointLuaTemplate;

                GeneratorTools.ReplaceKey(ref waypointLua, "Index", i + 2);
                GeneratorTools.ReplaceKey(ref waypointLua, "Name", waypoints[i].Name);
                GeneratorTools.ReplaceKey(ref waypointLua, "X", waypoints[i].Coordinates.X);
                GeneratorTools.ReplaceKey(ref waypointLua, "Y", waypoints[i].Coordinates.Y);
                if (waypoints[i].OnGround) GeneratorTools.ReplaceKey(ref waypointLua, "Altitude", "0");

                flightPlanLua += waypointLua + "\n";
            }

            return flightPlanLua;
        }

        private static void SaveFlightGroup(DCSMission mission, UnitMakerGroupInfo? groupInfo, MissionTemplateFlightGroupRecord flightGroup, DBEntryUnit unitDB, string homeBase)
        {
            mission.Briefing.AddItem(DCSMissionBriefingItemType.FlightGroup,
                $"{groupInfo.Value.Name}(P)\t" +
                $"{flightGroup.Count}× {unitDB.UIDisplayName}\t" +
                $"{GeneratorTools.FormatRadioFrequency(unitDB.AircraftData.RadioFrequency)}\t" +
                $"{Toolbox.FormatPayload(flightGroup.Payload)}\t" +
                $"{homeBase}");
            for (int i = 0; i < flightGroup.Count; i++)
                mission.AppendValue("SCRIPTCLIENTPILOTNAMES", $"\"{groupInfo.Value.Name} {i + 2}\",");
        }

        private static void SaveWaypointsToBriefing(DCSMission mission, Coordinates initialCoordinates, List<Waypoint> waypoints, bool useImperialSystem, UnitMakerGroupInfo? groupInfo)
        {
            double totalDistance = 0;
            Coordinates lastWP = initialCoordinates;

            // Add first (takeoff) and last (landing) waypoints to get a complete list of all waypoints
            List<Waypoint> allWaypoints = new List<Waypoint>(waypoints);
            allWaypoints.Insert(0, new Waypoint(Database.Instance.Common.Names.WPInitialName, initialCoordinates));
            allWaypoints.Add(new Waypoint(Database.Instance.Common.Names.WPFinalName, initialCoordinates));
            mission.Briefing.AddItem(DCSMissionBriefingItemType.Waypoint, $"\t{groupInfo.Value.Name}\t");
            
            List<string> waypointTextRows = new List<string>();
            foreach (Waypoint waypoint in allWaypoints)
            {
                double distanceFromLast = waypoint.Coordinates.GetDistanceFrom(lastWP);
                totalDistance += distanceFromLast;
                lastWP = waypoint.Coordinates;

                string waypointText =
                    waypoint.Name + "\t" +
                    (useImperialSystem ? $"{distanceFromLast * Toolbox.METERS_TO_NM:F0} nm" : $"{distanceFromLast / 1000.0:F0} Km") + "\t" +
                    (useImperialSystem ? $"{totalDistance * Toolbox.METERS_TO_NM:F0} nm" : $"{totalDistance / 1000.0:F0} Km");

                mission.Briefing.AddItem(DCSMissionBriefingItemType.Waypoint, waypointText);
                waypointTextRows.Add(waypointText);
            }
            mission.Briefing.FlightBriefings.Add(new DCSMissionFlightBriefing {
                Name = groupInfo.Value.Name,
                Type = groupInfo.Value.UnitDB.DCSIDs.First(),
                Waypoints = waypointTextRows
             });
        }
    }
}
