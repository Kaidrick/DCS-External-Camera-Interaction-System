local AECIS = {}

AECIS.PORT = 3012

package.path = package.path .. ";./LuaSocket/?.lua"
package.path = package.path .. ";./Scripts/?.lua"
package.cpath = package.cpath .. ";./LuaSocket/?.dll"

AECIS.socket = require("socket")
AECIS.JSON = require("JSON")

AECIS.client = nil
AECIS.server = nil

------------------------------------------------------------------------------
-- Prev Export functions
local _prevExport = {}
_prevExport.LuaExportActivityNextEvent = LuaExportActivityNextEvent
_prevExport.LuaExportBeforeNextFrame = LuaExportBeforeNextFrame
_prevExport.LuaExportStart = LuaExportStart
_prevExport.LuaExportStop = LuaExportStop


local inertia_delta = nil
local inertia_angle_delta = nil

-- stop zoom before set new command based on zoom_level?
AECIS.model_iteration_count = 0  -- interval is 0.01 second
AECIS.camera_zoom_count = 0
AECIS.camera_zoom_direction = 0  -- 1 or 0 or -1?


AECIS.logFile = io.open(lfs.writedir()..[[Logs\DCS-AECIS.log]], "w")
function AECIS.log(str)
	if AECIS.logFile then
		AECIS.logFile:write(str .. "\n")
		AECIS.logFile:flush()
	end
end

function setNewCamera(camera_delta)  -- new camera is a table, instruction should be discrete
	--[[
	camera_delta contains the camera movement delta
	camera_delta = {
		p = {
			x = dx,
			y = dy,
			z = dz
		},
		x = {},
		y = {},
		z = {},  -- maybe useless, can't make xyz work, use set command to rotate camera instead
		
		camera_command,  -- int
		camera_params,  -- list of doubles
	}
	
	if there is no movement, then all value should be either NaN or zero
	if there is movement, then keep add delta to the current camera for each frame
	
	--]]
	-- rate control
	
	-- get current camera position
	local cp = LoGetCameraPosition()
	
	-- check if data is valid, especially position --> p
	if not camera_delta or camera_delta.p.x == 'NaN' or camera_delta.p.y == 'NaN' or camera_delta.p.z == 'NaN' then
		-- no input or invalid input from client, keep moving using last valid instruction
		if inertia_delta then
			--AECIS.log("camera_delta is nil or input is invalid, use inertia_delta")
			camera_delta = inertia_delta  -- retrieve last valid delta
		else  -- inertia_delta is also nil, meaning no data available, no need to set new camera
			--AECIS.log("inertia_delta does not exist. keep camera still")
			return
		end
	end
	
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
			LoSetCameraPosition(cp)  -- try set new camera position
			-- command = 2007 - mouse camera rotate left/right  
			-- command = 2008 - mouse camera rotate up/down
			-- command = 2009 - mouse camera zoom 
			
			---[[
			-- camera rotation control
			-- camera rotation very sluggish on high movement speed, need to investigate
			-- very sluggish even if vector logic is put in lua
			if cmd == 1 then
				if params and type(params[1]) == 'number' and type(params[2]) == 'number' then
					-- valid, can set angle
					LoSetCommand(2007, params[1] * 0.001) -- 
					LoSetCommand(2008, params[2] * 0.001) -- 
					AECIS.cameraZoom(zoom)
					-- zoom
				else  -- params does not exist for some reason?
					if inertia_angle_delta then
						LoSetCommand(2007, inertia_angle_delta[1] * 0.001) -- 
						LoSetCommand(2008, inertia_angle_delta[2] * 0.001) -- 
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
		AECIS.server:settimeout(0.001)  -- give up if no connection from a client
		AECIS.client = AECIS.server:accept()  -- if client is nil then connection is not made
		
		if AECIS.client then  -- if client is not nil, connection is made
			AECIS.client:settimeout(0.001)
			-- send camera data first
			current_camera_position = LoGetCameraPosition()
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
	AECIS.server:settimeout(0.001)  -- give up if not connection from a client is made
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


function LuaExportActivityNextEvent(t)
	local tNext = t
	AECIS.step()
	
	-- call
    local _status,_result = pcall(function()
        -- Call original function if it exists
        if _prevExport.LuaExportActivityNextEvent then
            _prevExport.LuaExportActivityNextEvent(t)
        end
    end)
	
	if not _status then
        AECIS.log('ERROR Calling other LuaExportActivityNextEvent from another script: ' .. _result)
    end
		
	tNext = tNext + 0.01  -- test interval
	return tNext
end

AECIS.log("DCS-Advanced External Camera Interaction System => Loaded")
