local AECIS = {}

AECIS.PORT = 31549

package.path = package.path .. ";./LuaSocket/?.lua"
package.path = package.path .. ";./Scripts/?.lua"
package.cpath = package.cpath .. ";./LuaSocket/?.dll"

AECIS.socket = require("socket")
AECIS.JSON = require("JSON")
require("Vector")
require("Matrix33")

local inspect = require("inspect")

AECIS.client = nil
AECIS.server = nil

------------------------------------------------------------------------------
-- Prev Export functions
local _prevExport = {}
_prevExport.LuaExportActivityNextEvent = LuaExportActivityNextEvent
_prevExport.LuaExportBeforeNextFrame = LuaExportBeforeNextFrame
_prevExport.LuaExportStart = LuaExportStart
_prevExport.LuaExportStop = LuaExportStop



local still_camera = {
	command = 1,
	dX = 0,  -- forward or backward  --> -1 <= value <= 1
	dY = 0,  -- up or down
	dZ = 0,  -- left or right
	joy_raw = { 0, 0 },
	o_f = false,
	
	params = { 0, 0, 0 },
	pit_cam = false,
	
	zoom = 0,
	zoom_raw = 0
}
local inertia_delta = still_camera
local inertia_angle_delta = still_camera.params

-- stop zoom before set new command based on zoom_level?
AECIS.model_iteration_count = 0  -- interval is 0.01 second
AECIS.camera_zoom_count = 0
AECIS.camera_zoom_direction = 0  -- 1 or 0 or -1?

AECIS.cockpit_horizontal_move_count = 0
AECIS.cockpit_vertical_move_count = 0

local cockpit_zoom_value = 0  -- initial value is 0, no zoom

local export_units = {}


AECIS.logFile = io.open(lfs.writedir()..[[Logs\DCS-AECIS.log]], "w")
function AECIS.log(str)
	if AECIS.logFile then
		AECIS.logFile:write(str .. "\n")
		AECIS.logFile:flush()
	end
end


local function isSameCameraDelta(d1, d2)
	if not d1 and not d2 then return true end
	if not d1 and d2 then return false end
	if d1 and not d2 then return false end

	local eX = (d1.dX == d2.dX)
	local eY = (d1.dY == d2.dY)
	local eZ = (d1.dZ == d2.dZ)
	local eCmd = (d1.command == d2.command)
	local eZoom = (d1.zoom == d2.zoom)
	
	local eParams
	
	if d1.params and d2.params then
		local eP1 = (d1.params[1] == d2.params[1])
		local eP2 = (d1.params[2] == d2.params[2])
		local eP3 = (d1.params[3] == d2.params[3])
		
		eParams = eP1 and eP2 and eP3
		
	else  -- params not both nil, must not equal
		return false
	end
	
	return eX and eY and eZ and eCmd and eZoom and eParams
end




--[[
	Input camera_delta example:
	LuaExport::LuaExportStart: 
	{
		command = 1,
		dX = 0,  -- forward or backward  --> -1 <= value <= 1
		dY = 0,  -- up or down
		dZ = 0,  -- left or right
		joy_raw = { 0, 0 },
		o_f = false,
		
		params = { 0.026111111111111, -0.0038888888888889, 0 },
		pit_cam = false,
		
		zoom = 0,
		zoom_raw = 0
	}
	if there is no movement, then all value should be either NaN or zero
	if there is movement, then keep add delta to the current camera for each frame
	
--]]
function setNewCamera(camera_delta)  -- new camera is a table, instruction should be discrete
	if camera_delta then
		-- if params are missing, use default value of 0,0,0
		if not camera_delta.params then
			camera_delta.params = inertia_angle_delta or still_camera.params
		end
	else
		camera_delta = inertia_delta or still_camera  -- if last camera is nil (first data?) then take still camera
	end
	
	if isSameCameraDelta(camera_delta, still_camera) and isSameCameraDelta(inertia_delta, still_camera) then
		return
	end
	
	
	local dX = camera_delta.dX
	local dY = camera_delta.dY
	local dZ = camera_delta.dZ
	
	local follow_orientation = camera_delta.o_f  -- true or false
	
	local cp = LoGetCameraPosition()
	
	-- if to follow orientation, then find current facing direction, and find x y z components and added to P
	-- if not to follow orientation, add value directly to 
	
	local X = Vector(cp.x.x, cp.x.y, cp.x.z)
	local Y = Vector(cp.y.x, cp.y.y, cp.y.z)
	local Z = Vector(cp.z.x, cp.z.y, cp.z.z)
	local P = Vector(cp.p.x, cp.p.y, cp.p.z)  -- Camera Position in LO coordinates

	
	-- TODO: refactoring if block
	local mX, mY, mZ
	if follow_orientation then
		mX = X * dX
		mY = Y * dY
		mZ = Z * dZ
	else  -- only move in x-z plane
		local bX = Vector(X.x, 0, X.z)
		local bY = Vector(0, 1, 0)
		local bZ = Vector(Z.x, 0, Z.z)
	
		mX = bX * dX
		mY = bY * dY
		mZ = bZ * dZ
	end
	
	P = P + mX
	P = P + mY
	P = P + mZ
	
	-- TODO: camera is stuck when pointing along Y axis
	-- TODO: need to observe how the native functions (equivalent to using numpad to rotate) works
	
	local rotM = Matrix33()
	rotM.x = X
	rotM.y = Y
	rotM.z = Z
	
	rotM:RotateY(-camera_delta.params[1])
	rotM:RotateX(-camera_delta.params[2])
	
	local new_camera_pos = {
		x = rotM.x,
		y = rotM.y,
		z = rotM.z,
		p = P
	}

	-- otherwise if code reaches here, either camera_delta is refreshed or camera_delta is inertia_delta
	local p = camera_delta.p
	local x = camera_delta.x
	local y = camera_delta.y
	local z = camera_delta.z
	local cmd = camera_delta.command
	local params = camera_delta.params
	local zoom = camera_delta.zoom
	
	--AECIS.log(AECIS.inspect(camera_delta))
	
	-- add delta to position
	cp.p.x = cp.p.x + p.x
	cp.p.y = cp.p.y + p.y
	cp.p.z = cp.p.z + p.z
	
	inertia_delta = camera_delta  -- save this valid camera delta
	
	-- save params only if it's not null <-- nil
	if params then
		inertia_angle_delta = params
	end
	
	local success, err = pcall(
		function() 
			--LoSetCameraPosition(cp)  -- try set new camera position
			LoSetCameraPosition(new_camera_pos)
			-- command = 2007 - mouse camera rotate left/right  
			-- command = 2008 - mouse camera rotate up/down
			-- command = 2009 - mouse camera zoom 
			
			-- > 2012 iCommandViewZoomAbs -- cockpit zoom, negative decrease FOV, positive increase FOV
			-- get zoom slider raw input
			-- initial value == 0
			-- zoom_value += slider_raw_input * 0.001,
			-- how to control speed?
			
			-- limit zoom min and max, also, zoom value is inverted
			if camera_delta.pit_cam then
				cockpit_zoom_value = cockpit_zoom_value + camera_delta.zoom_raw * -0.01  -- zoom_raw is + or -
				if cockpit_zoom_value > 1 then  -- set to 1
					cockpit_zoom_value = 1
				elseif cockpit_zoom_value < -1 then  -- set to -1
					cockpit_zoom_value = -1
				end
				LoSetCommand(2012, cockpit_zoom_value)
			end
			
			-- induce movement for cockpit view? this also message up free camera
			-- need to disable these inputs when using external free camera
			-- seperate control is a must
			-- different aircraft has different behavior
			if camera_delta.pit_cam then
				LoSetCommand(2048, dZ * 0.001)
				LoSetCommand(2050, dY * 0.001 * -1)
				LoSetCommand(2052, dX * 0.001)
			end
			
			---[[
			-- camera rotation control
			-- camera rotation very sluggish on high movement speed, need to investigate
			-- very sluggish even if vector logic is put in lua
			if cmd == 1 then
				if params and type(params[1]) == 'number' and type(params[2]) == 'number' then
					-- valid, can set angle
					LoSetCommand(2007, params[1] * 0.001) -- 
					LoSetCommand(2008, params[2] * 0.001) -- 
					if camera_delta.pit_cam then
						LoSetCommand(2046, params[3])
					end
					AECIS.cameraZoom(zoom)
					-- zoom
				else  -- params does not exist for some reason?
					if inertia_angle_delta then
						LoSetCommand(2007, inertia_angle_delta[1] * 0.001) -- 
						LoSetCommand(2008, inertia_angle_delta[2] * 0.001) -- 
						if camera_delta.pit_cam then
							LoSetCommand(2046, inertia_angle_delta[3]) -- 
						end
						AECIS.cameraZoom(zoom)
					end
					-- else do nothing
				end
				-- LoSetCommand(2008, 0.0001) -- what?
			elseif cmd == 0 then  -- cmd is 0, but if input is no zero then maintain, if zero stop and maintain
				-- definitely don't have a valid params
				if inertia_angle_delta then
					-- should stop at zero, keep rate
					LoSetCommand(2007, inertia_angle_delta[1] * 0.001) --
					LoSetCommand(2008, inertia_angle_delta[2] * 0.001) --
					if camera_delta.pit_cam then
						LoSetCommand(2046, inertia_angle_delta[3]) --
					end
					AECIS.cameraZoom(zoom)
				end
			end
			--]]
		end
	)
	if not success then
		AECIS.log(err)
	end
	
end



function AECIS.cameraCheckZoomDirection(zoom_level)
	local direction = zoom_level * AECIS.camera_zoom_direction
	if direction == 0 then
	-- no zoom?
	elseif direction > 0 then
	-- same direction, no need to stop
	elseif direction < 0 then 
		LoSetCommand(335) -- differet direction, stop once before zoom
	end
	AECIS.camera_zoom_direction = zoom_level
end

function AECIS.cameraAdjustZoomSpeed(zoom_level)
	AECIS.model_iteration_count = AECIS.model_iteration_count + 1  -- add iteration count
	if AECIS.model_iteration_count > 100 then -- should be equal to 100, so this is the 101st interaction
		AECIS.model_iteration_count = AECIS.model_iteration_count - 100  -- set as first iter
	end
	-- how frequently to stop zoom?
	-- max every iter
	-- mid depends on zoom level? since zoom_level is always from -1 to 1
	-- how many times to block in 100 iterations? from 0 time(s) to 100 time(s)
	--                                               if zoom max     if zoom very small
	
	-- FIXME: very sluggish zoom on third detent? blocking to few? not sync to input rate?
	
	local zoom_step = math.abs(zoom_level) * 100  -- at this step, block?
	if zoom_step > 0 and zoom_step < 20 then  -- low zoom speed, block every 1 iteration
		LoSetCommand(335)  -- block 100 times

	elseif zoom_step >= 20 and zoom_step < 40 then  -- low zoom speed, block every 10 iteration?
		if AECIS.model_iteration_count % 5 == 0 then  -- block 20 times
			LoSetCommand(335)  -- block

		end
	elseif zoom_step >= 40 and zoom_step < 60 then  -- medium zoom speed, block every 25 teration?
		if AECIS.model_iteration_count % 10 == 0 then  -- block 10 times
			LoSetCommand(335)  -- block

		end
	elseif zoom_step >= 60 and zoom_step < 80 then -- high zoom speed, bloack every 50 iteration?
		if AECIS.model_iteration_count % 20 == 0 then  -- block 5 times
			LoSetCommand(335)  -- block

		end
	elseif zoom_step >= 80 and zoom_step <= 100 then  -- max zoom speed, dont block at all?
		-- No block -- block 0 times
	end
	-- so block --> zoom_level * 100
	-- min every 100 iter?
	-- block in this function
	
end

function AECIS.cameraZoom(zoom_level)  -- what is value change direction? very slow if stop zoom
	-- cockpit view zoom
	
	if zoom_level == 0 then
		LoSetCommand(335)  -- Stop zoom in for external views

		AECIS.camera_zoom_direction = 0
	elseif zoom_level > 0 then
		-- check if direction change
		AECIS.cameraCheckZoomDirection(zoom_level)
		AECIS.cameraAdjustZoomSpeed(zoom_level) 
		LoSetCommand(334)  -- Zoom in for external views
		
	elseif zoom_level < 0 then
		AECIS.cameraCheckZoomDirection(zoom_level)
		AECIS.cameraAdjustZoomSpeed(zoom_level)
		LoSetCommand(336)  -- Zoom out for external views
		
	end
end


function AECIS.step()
	if AECIS.server then
		-- AECIS.server:settimeout(0)  -- give up if no connection from a client
		AECIS.client = AECIS.server:accept()  -- if client is nil then connection is not made
		
		if AECIS.client then  -- if client is not nil, connection is made
			-- AECIS.client:settimeout(0.001)
			-- send camera data first
			local current_camera_position = LoGetCameraPosition()
			
			local cam_pos = AECIS.JSON:encode(current_camera_position)  -- encode a table to JSON string
			AECIS.client:send(cam_pos .. "\n")
			
			-- then receive camera delta
			local line, err = AECIS.client:receive()
			if not err then
				-- try parsing json string, JSON:decode(delta_json)
				local success, res = pcall(
					function()
						return AECIS.JSON:decode(line)
					end
				)
				if success then  -- run request
					setNewCamera(res)
					--SetCamera(res)
				else
					AECIS.log(res)  -- print JSON decode error to log
				end
			end
			AECIS.client:close()
		else  -- no connection from client
			-- keep moving camera
			setNewCamera(nil)
			--SetCamera(nil)
			--log("No connection. Keep Inertia")
		end
	end
end


function LuaExportStart()
	AECIS.server = assert(AECIS.socket.bind("127.0.0.1", AECIS.PORT))
	AECIS.server:settimeout(0)  -- give up if not connection from a client is made
	local ip, port = AECIS.server:getsockname()
	AECIS.log("AECIS: Server Started on Port " .. port .. " at " .. ip)
	
	_status,_result = pcall(function()
        -- Call original function if it exists
        if _prevExport.LuaExportStart then
            _prevExport.LuaExportStart()
        end
    end)
	
	if not _status then
        AECIS.log('ERROR Calling other LuaExportStart from another script: ' .. _result)
    end
	
end

function LuaExportStop()
	AECIS.client = nil
	AECIS.server:close()
	AECIS.log("AECIS Server Shutdown")
	
	_status,_result = pcall(function()
        -- Call original function if it exists
        if _prevExport.LuaExportStop then
            _prevExport.LuaExportStop()
        end
    end)
	
	if not _status then
        AECIS.log('ERROR Calling other LuaExportStop from another script: ' .. _result)
    end
end


function LuaExportBeforeNextFrame()
	-- call original
    _status,_result = pcall(function()
        -- Call original function if it exists
        if _prevExport.LuaExportBeforeNextFrame then
            _prevExport.LuaExportBeforeNextFrame()
        end
    end)
	
	if not _status then
        AECIS.log('ERROR Calling other LuaExportBeforeNextFrame from another script: ' .. _result)
    end
end


-- base timer is 0.01 second, step iterates 20 times is 0.2 second, 10 times is 0.1 second
local _srs = false  -- SRS send every 0.2 second  <-- srs handles other exports
local _helios = false  -- HELIOS send every 0.1 second

local limit_step = 0


-- function LuaExportAfterNextFrame()
--  	export_units = LoGetWorldObjects()
-- end


function LuaExportActivityNextEvent(t)
	-- local tNext = tCurrent + 0.1 -- for helios support
    -- we only want to send once every 0.2 seconds 
    -- but helios (and other exports) require data to come much faster
    -- so we just flip a boolean every run through to reduce to 0.2 rather than 0.1 seconds


	local tNext = t
	AECIS.step()
	
	-- call original function once every 20 iterations
	limit_step = limit_step + 1 -- iter
	if limit_step == 200 then
		local _status,_result = pcall(function()
			-- Call original function if it exists
			if _prevExport.LuaExportActivityNextEvent then
				_prevExport.LuaExportActivityNextEvent(t)
			end
		end)
		
		if not _status then
			AECIS.log('ERROR Calling other LuaExportActivityNextEvent from another script: ' .. _result)
		end
		
		-- reset limit_step to 0
		limit_step = 0
	end
    
		
	tNext = tNext + 0.01  -- test interval
	return tNext
end

AECIS.log("DCS-Advanced External Camera Interaction System => Loaded")
