/*
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

namespace BriefingRoom4DCS.Generator
{
    internal class MissionGeneratorFeaturesMission : MissionGeneratorFeatures<DBEntryFeatureMission>
    {
        internal MissionGeneratorFeaturesMission(UnitMaker unitMaker, MissionTemplateRecord template) : base(unitMaker, template) { }

        internal void GenerateMissionFeature(DCSMission mission, string featureID, Coordinates initialCoordinates, Coordinates objectivesCenter)
        {
            DBEntryFeatureMission featureDB = Database.Instance.GetEntry<DBEntryFeatureMission>(featureID);
            if (featureDB == null) // Feature doesn't exist
            {
                BriefingRoom.PrintToLog($"Mission feature {featureID} not found.", LogMessageErrorLevel.Warning);
                return;
            }
            Coalition coalition = featureDB.UnitGroupFlags.HasFlag(FeatureUnitGroupFlags.Friendly) ? _template.ContextPlayerCoalition : _template.ContextPlayerCoalition.GetEnemy();

            Coordinates pointSearchCenter = Coordinates.Lerp(initialCoordinates, objectivesCenter, featureDB.UnitGroupSpawnDistance);
            Coordinates? spawnPoint =
                _unitMaker.SpawnPointSelector.GetRandomSpawnPoint(
                    featureDB.UnitGroupValidSpawnPoints, pointSearchCenter,
                    featureDB.UnitGroupFlags.HasFlag(FeatureUnitGroupFlags.AwayFromMissionArea) ? new MinMaxD(50, 100) : new MinMaxD(0, 5),
                    coalition: featureDB.UnitGroupFlags.HasFlag(FeatureUnitGroupFlags.IgnoreBorders) ? null : coalition
                    );
            if (!spawnPoint.HasValue) // No spawn point found
            {
                BriefingRoom.PrintToLog($"No spawn point found for mission feature {featureID}.", LogMessageErrorLevel.Warning);
                return;
            }

            var goPoint = spawnPoint.Value;

            if (featureDB.UnitGroupFlags.HasFlag(FeatureUnitGroupFlags.MoveTowardObjectives))
                goPoint = objectivesCenter;
            else if (featureDB.UnitGroupFlags.HasFlag(FeatureUnitGroupFlags.MoveTowardPlayerBase))
                goPoint = initialCoordinates;

            Coordinates coordinates2 = goPoint + Coordinates.CreateRandom(5, 20) * Toolbox.NM_TO_METERS;
            Dictionary<string, object> extraSettings = new Dictionary<string, object>();
            UnitMakerGroupInfo? groupInfo = AddMissionFeature(featureDB, mission, spawnPoint.Value, coordinates2, ref extraSettings);

            AddBriefingRemarkFromFeature(featureDB, mission, false, groupInfo, extraSettings);
        }
    }
}