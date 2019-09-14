Proper detection of supported resolutions under Windows
=======================================================
Author: Max Vollmer
github: https://github.com/maxvollmer/ProperSupportedResolutions

Description:

I noticed that a bunch of games (Anno 1800, everything Unity) detect the supported resolutions of my desktop wrong.

I have a multi-monitor setup where my desktop is duplicated on 2 monitors, one is 4K with a maximum resolution of 3840x2160, the other has a maximum resolution of 3440x1440. However the 2nd monitor is able to receive a full 4K signal and just downsamples it, and my desktop resolution is a full 3840x2160.

Still, these games detect the highest possible resolution as 3440x1440. I have been in contact with Ubisoft Blue Byte (Anno 1800) for months now, and they were unable to find a fix. I also contacted Redbeet Interactive (Raft) and they pointed me towards Unity, which they use for their game. And indeed, Unity does detect the wrong resolution on my system.

Now I don't know why this happens, or what these game engines are doing to get it wrong, but I do know how to get it right. So in this repo I have two files, one C++ and one C# for Unity, which correctly detect the supported desktop resolutions on any Windows system.

Since most games are multi-platform, you'd want to wrap the C++ code in #ifdef WIN32. The C# code for Unity already does this for you; it falls back to the resolutions provided by Unity for non-Windows systems.

License: MIT, see license statements in the source files for details.
