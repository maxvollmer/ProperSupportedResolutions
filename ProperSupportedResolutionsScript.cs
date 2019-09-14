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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

public class ProperSupportedResolutionsScript : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll", EntryPoint = "EnumDisplaySettingsA")]
    private static extern int EnumDisplaySettingsA(IntPtr lpszDeviceName, UInt32 iModeNum, IntPtr lpDevMode);

    private const int SIZEOF_DEVMODE = 124;
    private const int DEVMODE_dmPelsWidth_OFFSET = 108;
    private const int DEVMODE_dmPelsHeight_OFFSET = 112;
    private const int DEVMODE_dmDisplayFrequency_OFFSET = 120;

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = SIZEOF_DEVMODE)]
    private struct Fake_DEVMODE
    {
        [System.Runtime.InteropServices.FieldOffset(36)]
        public UInt16 dmSize;
    }

    public class ResolutionComparer : Comparer<Resolution>
    {
        public override int Compare(Resolution r1, Resolution r2)
        {
            if (r1.width == r2.width)
            {
                if (r1.height == r2.height)
                {
                    return r1.refreshRate.CompareTo(r2.refreshRate);
                }
                else
                {
                    return r1.height.CompareTo(r2.height);
                }
            }
            else
            {
                return r1.width.CompareTo(r2.width);
            }
        }
    }

    private static Resolution[] GetSupportedResolutions()
    {
        HashSet<Resolution> resolutions = new HashSet<Resolution>();

        Fake_DEVMODE devMode = new Fake_DEVMODE() { dmSize = SIZEOF_DEVMODE };
        IntPtr pDevMode = Marshal.AllocHGlobal(SIZEOF_DEVMODE);
        Marshal.StructureToPtr(devMode, pDevMode, true);

        for (UInt32 iModeNum = 0; EnumDisplaySettingsA(IntPtr.Zero, iModeNum, pDevMode) != 0; iModeNum++)
        {
            UInt16 dmPelsWidth = Marshal.PtrToStructure<UInt16>(IntPtr.Add(pDevMode, DEVMODE_dmPelsWidth_OFFSET));
            UInt16 dmPelsHeight = Marshal.PtrToStructure<UInt16>(IntPtr.Add(pDevMode, DEVMODE_dmPelsHeight_OFFSET));
            UInt16 dmDisplayFrequency = Marshal.PtrToStructure<UInt16>(IntPtr.Add(pDevMode, DEVMODE_dmDisplayFrequency_OFFSET));
            resolutions.Add(new Resolution() { width = (int)dmPelsWidth, height = (int)dmPelsHeight, refreshRate = (int)dmDisplayFrequency });
        }

        Marshal.FreeHGlobal(pDevMode);

        var resolutionsList = resolutions.ToList();
        resolutionsList.Sort(new ResolutionComparer());
        return resolutionsList.ToArray();
    }
#else
    private static Resolution[] GetSupportedResolutions()
    {
        return Screen.resolutions;
    }
#endif

    void Start()
    {
        var resolutions = GetSupportedResolutions();
        foreach (var res in resolutions)
        {
            Debug.Log(res.width + "x" + res.height + " : " + res.refreshRate);
        }
    }

    void Update()
    {
        
    }
}
