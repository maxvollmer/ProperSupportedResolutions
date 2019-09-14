/*
Copyright (c) 2019 Max Vollmer
 
 Permission is hereby granted, free of charge, to any person obtaining
 a copy of this software and associated documentation files (the
 "Software"), to deal in the Software without restriction, including
 without limitation the rights to use, copy, modify, merge, publish,
 distribute, sublicense, and/or sell copies of the Software, and to
 permit persons to whom the Software is furnished to do so, subject to
 the following conditions:
 
 The above copyright notice and this permission notice shall be included
 in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

/*
Proper detection of supported resolutions under Windows
=======================================================
Author: Max Vollmer
github: https://github.com/maxvollmer/ProperSupportedResolutions

Description:

I noticed that a bunch of games (Anno 1800, everything Unity) detect the supported resolutions of my desktop wrong.

I have a multi-monitor setup where my desktop is duplicated on 2 monitors, one is 4K with a maximum resolution of 3840x2160, the other has a maximum resolution of 3440x1440. However the 2nd monitor is able to receive a full 4K signal and just downsamples it, and my desktop resolution is a full 3840x2160.

Still, these games detect the highest possible resolution as 3440x1440. I have been in contact with Ubisoft Blue Byte (Anno 1800) for months now, and they were unable to find a fix. I also contacted Redbeet Interactive (Raft) and they pointed towards Unity, which they use. And indeed, Unity does detect the wrong resolution on my system.

Now I don't know why this happens, or what these game engines are doing to get it wrong, but I do know how to get it right. So in this repo I have two files, one C++ and one C# for Unity, which correctly detect the supported desktop resolutions on any Windows system.

Since most games are multi-platform, you'd want to wrap the C++ code in #ifdef WIN32. The C# code for Unity already does this for you; it falls back to the resolutions provided by Unity for non-Windows systems.

License: MIT, see top of file
*/

#include <Windows.h>
#include <iostream>
#include <set>
#include <vector>

struct Resolution
{
	uint64_t width;
	uint64_t height;
	uint64_t refreshRate;

	struct Compare
	{
	public:
		bool operator()(const Resolution& r1, const Resolution& r2) const
		{
			if (r1.width == r2.width)
			{
				if (r1.height == r2.height)
				{
					return r1.refreshRate < r2.refreshRate;
				}
				else
				{
					return r1.height < r2.height;
				}
			}
			else
			{
				return r1.width < r2.width;
			}
			
		}
	};
};

std::vector<Resolution> GetSupportedResolutions()
{
	std::set<Resolution, Resolution::Compare> resolutions;

	DEVMODEA dm{ 0 };
	dm.dmSize = sizeof(dm);
	for (DWORD iModeNum = 0; EnumDisplaySettingsA(NULL, iModeNum, &dm); iModeNum++)
	{
		resolutions.insert({ dm.dmPelsWidth, dm.dmPelsHeight, dm.dmDisplayFrequency });
	}

	return std::vector<Resolution>{ resolutions.begin(), resolutions.end() };
}
