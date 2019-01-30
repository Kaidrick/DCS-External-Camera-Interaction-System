# DCS-External-Camera-Interaction-System
Very early implementation of a camera control concept for DCS World, built in C# WPF.

**How to intall to DCS**
It is necessary to add the following files to your ..\Users\\\<Your Username>\Saved Games\DCS\.\<Branch\>\Scripts
* DCS-AECIS.lua

If you do not have an Export.lua yet in the same folder, you should create your own.
In your file explorer, right click and choose New > Text Document. Then rename this file to Export.lua; make sure you change the extension name to .lua rather than Export.lua.txt

Add this line of code in your Export.lua, preferably at the end.

`-- AECIS

local dcsAECIS=require('lfs');dofile(dcsAECIS.writedir()..[[Scripts\DCS-AECIS.lua]])`

This told DCS to load AECIS in its own namespace/table.
After you start a mission, press F11 or LCtrl+F11 to switch to external camera. Press 'Connect' to try connecting to DCS. If the button changes to 'Disconnect', it means the TCP connection has been establish the Export function are working properly.
