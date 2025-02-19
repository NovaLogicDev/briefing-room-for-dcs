briefingRoom.f10MenuCommands.debug = { } -- Create F10 debug sub-menu
briefingRoom.printDebugMessages = true -- Enable debug messages

function briefingRoom.f10MenuCommands.debug.activateAllAircraft()
  if #briefingRoom.aircraftActivator.reserveQueue == 0 then
    briefingRoom.debugPrint("No groups in the reserve queue")
    return
  end

  while #briefingRoom.aircraftActivator.reserveQueue > 0 do
    briefingRoom.aircraftActivator.pushFromReserveQueue()
  end
end

-- Destroys the next active target unit
function briefingRoom.f10MenuCommands.debug.destroyTargetUnit()
  for index,objective in ipairs(briefingRoom.mission.objectives) do
    if (#objective.unitsID > 0) then
      local u = dcsExtensions.getUnitByID(objective.unitsID[1])
      if u == nil then
        u = dcsExtensions.getStaticByID(objective.unitsID[1])
      end
      if u ~= nil then
        trigger.action.outText("Destroyed "..u:getName(), 2)
        trigger.action.explosion(u:getPoint(), 100)
        return
      end
    end
  end

  trigger.action.outText("No target units found", 2)
end


function  briefingRoom.f10MenuCommands.debug.dumpAirbaseDataType(o)
 if o == 16 then return "Runway" end
 if o == 40 then return "HelicopterOnly" end
 if o == 68 then return "HardenedAirShelter" end
 if o == 72 then return "AirplaneOnly" end
 if o == 104 then return "OpenAirSpawn" end
 return "UNKNOWN"
end

function briefingRoom.f10MenuCommands.debug.dumpAirbaseData()
  briefingRoom.debugPrint("STARTING AIRBASE DUMP");
  local base = world.getAirbases()
   for i = 1, #base do
      briefingRoom.debugPrint(base[i]:getID()..":\n")
      local parkingData = base[i]:getParking()
      local parkingString = ""
      for j = 1, #parkingData do
        if parkingData[j].Term_Type ~= 16 then
          parkingString = parkingString.."\nSpot"..j..".Coordinates="..parkingData[j].vTerminalPos.x..","..parkingData[j].vTerminalPos.z
          parkingString = parkingString.."\nSpot"..j..".DCSID="..parkingData[j].Term_Index
          parkingString = parkingString.."\nSpot"..j..".Type="..briefingRoom.f10MenuCommands.debug.dumpAirbaseDataType(parkingData[j].Term_Type)
        end
        if j % 10 == 5 then
          briefingRoom.debugPrint(parkingString)
          parkingString = ""
        end
      end
      briefingRoom.debugPrint(parkingString..";")
   end
   briefingRoom.debugPrint("DONE AIRBASE DUMP");
end


-- Create the debug menu
do
  briefingRoom.f10Menu.debug = missionCommands.addSubMenu("(DEBUG MENU)", nil)
  missionCommands.addCommand("Destroy target unit", briefingRoom.f10Menu.debug, briefingRoom.f10MenuCommands.debug.destroyTargetUnit)
  missionCommands.addCommand("Simulate player takeoff (start mission)", briefingRoom.f10Menu.debug, briefingRoom.mission.coreFunctions.beginMission)
  missionCommands.addCommand("Activate all aircraft groups", briefingRoom.f10Menu.debug, briefingRoom.f10MenuCommands.debug.activateAllAircraft)
  missionCommands.addCommand("Dump All Airbase Data", briefingRoom.f10Menu.debug, briefingRoom.f10MenuCommands.debug.dumpAirbaseData)

end
