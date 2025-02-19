﻿/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar
(https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World.
If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BriefingRoom4DCS.Template
{
    public sealed class MissionTemplate
    {
        private static readonly string DEFAULT_TEMPLATE_FILEPATH = $"{BRPaths.ROOT}Default.brt";
        public const int MAX_OBJECTIVES = 5;
        public const int MAX_PLAYER_FLIGHT_GROUPS = 8;
        public const int MAX_OBJECTIVE_DISTANCE = 300;
        public const int MAX_OBJECTIVE_SEPERATION = 100;
        public string BriefingMissionName { get; set; }
        public string BriefingMissionDescription { get; set; }
        public string ContextCoalitionBlue { get; set; }
        public string ContextCoalitionRed { get; set; }
        public Decade ContextDecade { get; set; }
        public Coalition ContextPlayerCoalition { get; set; }
        public string ContextTheater { get; set; }
        public string ContextSituation { get; set; }
        public Season EnvironmentSeason { get; set; }
        public TimeOfDay EnvironmentTimeOfDay { get; set; }
        public string EnvironmentWeatherPreset { get; set; }
        public Wind EnvironmentWind { get; set; }
        public int FlightPlanObjectiveDistance { get { return FlightPlanObjectiveDistance_; } set { FlightPlanObjectiveDistance_ = Toolbox.Clamp(value, 0, MAX_OBJECTIVE_DISTANCE); } }
        private int FlightPlanObjectiveDistance_;
        public int FlightPlanObjectiveSeperation { get { return FlightPlanObjectiveSeperation_; } set { FlightPlanObjectiveSeperation_ = Toolbox.Clamp(value, 0, MAX_OBJECTIVE_SEPERATION); } }
        private int FlightPlanObjectiveSeperation_;
        public string FlightPlanTheaterStartingAirbase { get; set; }
        public List<string> MissionFeatures { get { return MissionFeatures_; } set { MissionFeatures_ = Database.Instance.CheckIDs<DBEntryFeatureMission>(value.ToArray()).ToList(); } }
        private List<string> MissionFeatures_ = new List<string>();
        public List<string> Mods { get { return Mods_; } set { Mods_ = Database.Instance.CheckIDs<DBEntryDCSMod>(value.ToArray()).ToList(); } }
        private List<string> Mods_ = new List<string>();
        public List<MissionTemplateObjective> Objectives { get; set; } = new List<MissionTemplateObjective>();
        public FogOfWar OptionsFogOfWar { get; set; }
        public List<string> OptionsMission { get { return OptionsMission_; } set { OptionsMission_ = Database.Instance.CheckIDs<DBEntryOptionsMission>(value.ToArray()).ToList(); } }
        private List<string> OptionsMission_ = new List<string>();
        public List<RealismOption> OptionsRealism { get { return OptionsRealism_; } set { OptionsRealism_ = value.Distinct().ToList(); } }
        private List<RealismOption> OptionsRealism_ = new List<RealismOption>();
        public List<MissionTemplateFlightGroup> PlayerFlightGroups { get { return PlayerFlightGroups_; } set { PlayerFlightGroups_ = value.Take(MAX_PLAYER_FLIGHT_GROUPS).ToList(); } }
        private List<MissionTemplateFlightGroup> PlayerFlightGroups_ = new List<MissionTemplateFlightGroup>();
        public List<MissionTemplatePackage> AircraftPackages { get { return AircraftPackages_; } set { AircraftPackages_ = value.Take(MAX_PLAYER_FLIGHT_GROUPS).ToList(); } }
        private List<MissionTemplatePackage> AircraftPackages_ = new List<MissionTemplatePackage>();
        public AmountNR SituationEnemySkill { get; set; }
        public AmountNR SituationEnemyAirDefense { get; set; }
        public AmountNR SituationEnemyAirForce { get; set; }
        public AmountNR SituationFriendlySkill { get; set; }
        public AmountNR SituationFriendlyAirDefense { get; set; }
        public AmountNR SituationFriendlyAirForce { get; set; }


        public MissionTemplate()
        {
            Clear();
        }

        public MissionTemplate(string filePath)
        {
            Clear();
            LoadFromFile(filePath);
        }
        public void Clear()
        {
            // If the default template is found, load it.
            if (File.Exists(DEFAULT_TEMPLATE_FILEPATH))
            {
                LoadFromFile(DEFAULT_TEMPLATE_FILEPATH);
                return;
            }

            BriefingMissionName = "";
            BriefingMissionDescription = "";

            ContextCoalitionBlue = "USA";
            ContextCoalitionRed = "Russia";
            ContextDecade = Decade.Decade2000;
            ContextPlayerCoalition = Coalition.Blue;
            ContextTheater = "Caucasus";
            ContextSituation = "";

            EnvironmentSeason = Season.Random;
            EnvironmentTimeOfDay = TimeOfDay.RandomDaytime;
            EnvironmentWeatherPreset = "";
            EnvironmentWind = Wind.Random;

            FlightPlanObjectiveDistance = 80;
            FlightPlanObjectiveSeperation = 30;
            FlightPlanTheaterStartingAirbase = "";

            MissionFeatures = new List<string>{
                "FriendlyAWACS",
                "FriendlyTankerBasket",
                "FriendlyTankerBoom"
            };

            Mods = new List<string>();

            Objectives = new MissionTemplateObjective[] { new MissionTemplateObjective() }.ToList();

            OptionsFogOfWar = FogOfWar.All;
            OptionsMission = new List<string> { "ImperialUnitsForBriefing", "MarkWaypoints" };
            OptionsRealism = new RealismOption[] { RealismOption.DisableDCSRadioAssists, RealismOption.NoBDA }.ToList();

            PlayerFlightGroups = new MissionTemplateFlightGroup[] { new MissionTemplateFlightGroup() }.ToList();
            AircraftPackages = new();

            SituationEnemySkill = AmountNR.Random;
            SituationEnemyAirDefense = AmountNR.Random;
            SituationEnemyAirForce = AmountNR.Random;

            SituationFriendlySkill = AmountNR.Random;
            SituationFriendlyAirDefense = AmountNR.Random;
            SituationFriendlyAirForce = AmountNR.Random;
            AssignAliases();
        }

        public bool LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            return Load(new INIFile(filePath));
        }

        public bool LoadFromString(string data)
        {
            return Load(INIFile.CreateFromRawINIString(data));
        }

        private bool Load(INIFile ini)
        {
            BriefingMissionName = ini.GetValue("Briefing", "MissionName", BriefingMissionName);
            BriefingMissionDescription = ini.GetValue("Briefing", "MissionDescription", BriefingMissionDescription);

            ContextCoalitionBlue = ini.GetValue("Context", "CoalitionBlue", ContextCoalitionBlue);
            ContextCoalitionRed = ini.GetValue("Context", "CoalitionRed", ContextCoalitionRed);
            ContextDecade = ini.GetValue("Context", "Decade", ContextDecade);
            ContextPlayerCoalition = ini.GetValue("Context", "PlayerCoalition", ContextPlayerCoalition);
            ContextTheater = ini.GetValue("Context", "Theater", ContextTheater);
            ContextSituation = ini.GetValue("Context", "Situation", ContextSituation);

            EnvironmentSeason = ini.GetValue("Environment", "Season", EnvironmentSeason);
            EnvironmentTimeOfDay = ini.GetValue("Environment", "TimeOfDay", EnvironmentTimeOfDay);
            EnvironmentWeatherPreset = ini.GetValue("Environment", "WeatherPreset", EnvironmentWeatherPreset);
            EnvironmentWind = ini.GetValue("Environment", "Wind", EnvironmentWind);

            FlightPlanObjectiveDistance = ini.GetValue("FlightPlan", "ObjectiveDistance", FlightPlanObjectiveDistance);
            FlightPlanObjectiveSeperation = ini.GetValue("FlightPlan", "ObjectiveSeperation", FlightPlanObjectiveSeperation);
            FlightPlanTheaterStartingAirbase = ini.GetValue("FlightPlan", "TheaterStartingAirbase", FlightPlanTheaterStartingAirbase);

            MissionFeatures = ini.GetValueDistinctList<string>("MissionFeatures", "MissionFeatures");

            Mods = ini.GetValueArray<string>("Mods", "Mods").ToList();

            Objectives.Clear();
            foreach (string key in ini.GetTopLevelKeysInSection("Objectives"))
                Objectives.Add(new MissionTemplateObjective(ini, "Objectives", key));

            OptionsFogOfWar = ini.GetValue("Options", "FogOfWar", OptionsFogOfWar);
            OptionsMission = ini.GetValueDistinctList<string>("Options", "Mission");
            OptionsRealism = ini.GetValueDistinctList<RealismOption>("Options", "Realism");

            PlayerFlightGroups.Clear();
            foreach (string key in ini.GetTopLevelKeysInSection("PlayerFlightGroups"))
                PlayerFlightGroups.Add(new MissionTemplateFlightGroup(ini, "PlayerFlightGroups", key));

            AircraftPackages.Clear();
            foreach (string key in ini.GetTopLevelKeysInSection("AircraftPackages"))
                AircraftPackages.Add(new MissionTemplatePackage(ini, "AircraftPackages", key));

            SituationEnemySkill = ini.GetValue("Situation", "EnemySkill", SituationEnemySkill);
            SituationEnemyAirDefense = ini.GetValue("Situation", "EnemyAirDefense", SituationEnemyAirDefense);
            SituationEnemyAirForce = ini.GetValue("Situation", "EnemyAirForce", SituationEnemyAirForce);

            SituationFriendlySkill = ini.GetValue("Situation", "FriendlySkill", SituationFriendlySkill);
            SituationFriendlyAirDefense = ini.GetValue("Situation", "FriendlyAirDefense", SituationFriendlyAirDefense);
            SituationFriendlyAirForce = ini.GetValue("Situation", "FriendlyAirForce", SituationFriendlyAirForce);

            AssignAliases();
            return true;
        }

        public void SaveToFile(string filePath)
        {
            var ini = GetAsIni();
            ini.SaveToFile(filePath);
        }

        public byte[] GetIniBytes()
        {
            var ini = GetAsIni();
            return Encoding.ASCII.GetBytes(ini.GetFileData());
        }

        private INIFile GetAsIni()
        {
            int i;
            var ini = new INIFile();

            ini.SetValue("Briefing", "MissionName", BriefingMissionName);
            ini.SetValue("Briefing", "MissionDescription", BriefingMissionDescription);

            ini.SetValue("Context", "CoalitionBlue", ContextCoalitionBlue);
            ini.SetValue("Context", "CoalitionRed", ContextCoalitionRed);
            ini.SetValue("Context", "Decade", ContextDecade);
            ini.SetValue("Context", "PlayerCoalition", ContextPlayerCoalition);
            ini.SetValue("Context", "Theater", ContextTheater);
            ini.SetValue("Context", "Situation", ContextSituation);

            ini.SetValue("Environment", "Season", EnvironmentSeason);
            ini.SetValue("Environment", "TimeOfDay", EnvironmentTimeOfDay);
            ini.SetValue("Environment", "WeatherPreset", EnvironmentWeatherPreset);
            ini.SetValue("Environment", "Wind", EnvironmentWind);

            ini.SetValue("FlightPlan", "ObjectiveDistance", FlightPlanObjectiveDistance);
            ini.SetValue("FlightPlan", "ObjectiveSeperation", FlightPlanObjectiveSeperation);
            ini.SetValue("FlightPlan", "TheaterStartingAirbase", FlightPlanTheaterStartingAirbase);

            ini.SetValueArray("MissionFeatures", "MissionFeatures", MissionFeatures.ToArray());

            ini.SetValueArray("Mods", "Mods", Mods.ToArray());

            for (i = 0; i < Objectives.Count; i++)
                Objectives[i].SaveToFile(ini, "Objectives", $"Objective{i:000}");

            ini.SetValue("Options", "FogOfWar", OptionsFogOfWar);
            ini.SetValueArray("Options", "Mission", OptionsMission.ToArray());
            ini.SetValueArray("Options", "Realism", OptionsRealism.ToArray());

            for (i = 0; i < PlayerFlightGroups.Count; i++)
                PlayerFlightGroups[i].SaveToFile(ini, "PlayerFlightGroups", $"PlayerFlightGroup{i:000}");

            for (i = 0; i < AircraftPackages.Count; i++)
                AircraftPackages[i].SaveToFile(ini, "AircraftPackages", $"AircraftPackage{i:000}");

            ini.SetValue("Situation", "EnemySkill", SituationEnemySkill);
            ini.SetValue("Situation", "EnemyAirDefense", SituationEnemyAirDefense);
            ini.SetValue("Situation", "EnemyAirForce", SituationEnemyAirForce);

            ini.SetValue("Situation", "FriendlySkill", SituationFriendlySkill);
            ini.SetValue("Situation", "FriendlyAirDefense", SituationFriendlyAirDefense);
            ini.SetValue("Situation", "FriendlyAirForce", SituationFriendlyAirForce);

            return ini;
        }

        internal void AssignAliases()
        {
            foreach (var item in PlayerFlightGroups)
                item.AssignAlias(PlayerFlightGroups.IndexOf(item));
            foreach (var item in Objectives)
                item.AssignAlias(Objectives.IndexOf(item));
            foreach (var item in AircraftPackages)
                item.AssignAlias(AircraftPackages.IndexOf(item));
        }


    }
}
