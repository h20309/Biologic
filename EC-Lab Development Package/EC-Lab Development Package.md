# EC-Lab® Development Package

User's Guide Version 6.11 – May 2024 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f716c73a0eb9f2e3291f3b25a7906308a70a7bb7daa9afba9540531b8152160d.jpg)



History


<table><tr><td>Version</td><td>Description</td></tr><tr><td>V6.11</td><td>Support for VMP-3 board L version</td></tr><tr><td>V6.08</td><td>Support for VMP300-series P board.</td></tr><tr><td>V6.06</td><td>Support new VMP-300 boards VU version 3</td></tr><tr><td>V6.05</td><td>Support new VMP-300 boards VU version 2</td></tr><tr><td>V6.04</td><td>SP-50e / SP-150e support added
1A48VP booster support added
MP-MEA (MuxPad) option support removed
fixed USB for 64b apps</td></tr><tr><td>V6.03</td><td>VMP-3e / VSP-3e support
open-in support for VMP3
BL_GetOptErr function description</td></tr><tr><td>V6.02</td><td>Bug fix on 100pA range update for VMP-300 series.
Adding of BP300 device</td></tr><tr><td>V6.01</td><td>Restructured examples code.</td></tr><tr><td>V6.00</td><td>64-bits compatibility added for LABVIEW to resolve the problem of alignment.</td></tr><tr><td>V5.39</td><td>Extended techniques memory</td></tr><tr><td>V5.38</td><td>Optimized the loop technique for mandatory goto use</td></tr><tr><td>V5.37</td><td>Fixed xilinx version comparison at the end of LoadXilinx step.</td></tr><tr><td>V5.36</td><td>64-bits compatibility added. New DLLs (EClib64.dll, blfind64.dll) and new Delphi 64-bits examples. New form in Delphi examples showing how to search and select a device in a broadcast list (uses blfind.dll).</td></tr><tr><td>V5.35</td><td>Support for HCP-1005 and MPG-2xx</td></tr><tr><td>V5.34</td><td>Updated the kernel4.bin to enable new amplifiers (2A and 10A) for VMP-300
Updated DLL for the new Kernel
Updated VMP-300 techniques for new record mode (see section 8)
Updated xlx firmware to accommodate new kernel
Updated C and C# examples for new kernel
Timebase parameter added to some LabVIEW examples</td></tr><tr><td></td><td>Updated Techniques description according to the new record mode</td></tr><tr><td>V5.33</td><td>Clarifications about BL_UpdateParameters usage
Fixed references to non-existent section 4.1
Added C/C++ and C# code examples</td></tr><tr><td>V5.32</td><td>New USB driver for Windows 8
LabVIEW examples are now provided for LabVIEW version 12 and 8.5
User guide corrections: Reference to "section 4.2" replaced by "section 5.3. Constants"; TchannelInfos.NbAmps, TdataInfos.MuxPad, TCurrentValuesOPTErr and OptPos fields were missing in the structure declaration</td></tr><tr><td>V5.31</td><td>Allows connection to SP-240 device. External control bug correction for VMP-300 series.</td></tr><tr><td>V5.29</td><td>Modular Pulse technique : add records mode and step index ECP v5.26</td></tr><tr><td>v5.28</td><td>Add BL_UpdateParameters_LV</td></tr><tr><td>v5.27</td><td>Add Modular Pulse technique
Add CASG technique
Add CASP technique
kernel v5.25
Kernel4 v5.27
Add blfind.dll v1.1
Structure TChannelInfos modified</td></tr><tr><td>v5.26</td><td>Record and external control options
Windows Installer
New USB driver for Windows XP/Vista/Seven 32bits/64bits
New fields: TdataInfos.MuxPad, TCurrentValuesOPTErr and OptPos</td></tr><tr><td>v5.23</td><td>Add techniques with limits to VMP-300 series : vscanlimit, iscanlimit, calimit, cplimit.
Option 4A amplifier for VMP-300
Allows connection to MPG2 device
Add Floating / grounded mode to VMP-300 series
Change name of constant</td></tr><tr><td>v5.22</td><td>Update LASV parameters (Record_every_dI, Record_every_dT) 
Kernel VMP3 v5.22 (Update change mode Potentio-Galvano with linked techniques)</td></tr><tr><td>v5.21</td><td>LASV (Large Amplitude sinusoidal voltammetry) Technique</td></tr><tr><td>V5.20</td><td>Control SP-300 and SP-200</td></tr><tr><td>v5.19</td><td>Description image with all technique 
Correction of parameter range</td></tr><tr><td>v5.18</td><td>First version</td></tr></table>

Overview ................... 13 

2. Files package .. 14 

3. General information ..................... 16 

3.1. Calling conventions ............ . 16 

3.2. Data alignment ............. . 16 

3.3. Multi-thread applications .......... . 16 

3.4. Data types ...... 16 

3.5. Variables description ............ 18 

4. Using the library .......... 19 

5. S tructures and C onstants .... . 20 

5.1. Structures . . 20 

5.2. Other structures ................ . 25 

5.2.1. Labview Structures .. . 25 

5.3. Constants .... . 26 

5.4. Error codes ......... . 33 

6. Functions reference ............. . 36 

6.1. Functions overview ......... . 36 

6.2. General functions . . 39 

6.3. Communication functions .... . 41 

6.4. Firmware functions ................. . 45 

6.5. Channel information functions . .. 47 

6.6. Technique functions ................ . 54 

.7. Start/stop functions .. . 61 

6.8. Data functions ............ . 65 

6.9. Miscellaneous functions . . 71 

7. Techniques .... . 75 

7.1. Notes .. . 75 

7.1.1 Recording additional values and XCTR changes from v5.34 . . 75 

7.1.2 Timebase calculation. . 76 

7.2. Open Circuit Voltage technique ................ .. 77 

7.2.1. Description .. . 77 

7.2.2. Technique parameters . . 77 

7.2.3. Data format . . 77 

7.2.4. Data conversion . . 78 

7.3. Cyclic Voltammetry technique ....... . 79 

7.3.1. Description . . 79 

7.3.2. Technique parameters . . 79 

7.3.3. Data format. . 80 

7.3.4. Data conversion . . 80 

7.4. Cyclic Voltammetry Advanced technique ......... . 81 

7.4.1. Description. . 81 

7.4.1. Technique parameters . . 81 

7.4.2. Data format . . 82 

7.4.3. Data conversion . . 82 

7.5. Chrono-Potentiometry technique ........... ... 83 

7.5.1. Description. . 83 

7.5.1. Technique parameters . . 83 

7.5.2. Data format . . 84 

7.5.3. Data conversion . . 84 

7.6. Chrono-Amperometry technique ...... . 85 

7.6.1. Description. . 85 

7.6.2. Technique parameters .. . 85 

7.6.3. Data format . . 86 

7.6.4. Data conversion . .. 86 

7.7. Voltage Scan technique .. . 87 

7.7.1. Description. . 87 

7.7.2. Technique parameters . . 87 

7.7.3. Data format . . 88 

7.7.4. Data conversion . . 88 

7.8. Current Scan technique .. . 89 

7.8.1. Description. . 89 

7.8.2. Technique parameters . . 89 

7.8.3. Data format . . 90 

7.8.4. Data conversion . . 90 

7.9. Constant Power technique .. . 91 

7.9.1. Description. . 91 

7.9.2. Technique parameters . . 91 

7.9.3. Data format . . 92 

7.9.4. Data conversion . . 92 

7.10. Constant Load technique ............ . 93 

7.10.1. Description . . 93 

7.10.2. Technique parameters .. . 93 

7.10.3. Data format . . 94 

7.10.4. Data conversion. . 94 

7.11. Potentio Electrochemical Impedance Spectroscopy technique ....... . 95 

7.11.1. Description . . 95 

7.11.2. Technique parameters .. . 95 

7.11.3. Data format .. . 96 

7.11.4. Data conversion. . 97 

7.12. Staircase Potentio Electrochemical Impedance Spectroscopy technique .......... . 99 

7.12.1. Description . . 99 

7.12.2. Technique parameters . . 99 

7.12.3. Data format . . 100 

7.12.4. Data conversion .. . 102 

7.13. Galvano Electrochemical Impedance Spectroscopy technique . . 103 

7.13.1. Description . . 103 

7.13.2. Technique parameters. . 103 

7.13.3. Data format .. . 104 

7.13.4. Data conversion. . 104 

7.14. Staircase Galvano Electrochemical Impedance Spectroscopy technique . . 105 

7.14.1. Description . 105 

7.14.2. Technique parameters .. . 105 

7.14.3. Data format . . 106 

7.14.4. Data conversion. . 106 

7.15. Differential Pulse Voltammetry technique ............ . 107 

7.15.1. Description . . 107 

7.15.2. Technique parameters . . 107 

7.15.3. Data format . . 108 

7.15.4. Data conversion .. . 108 

7.16. Square Wave Voltammetry technique ........ . 109 

7.16.1. Description . . 109 

7.16.2. Technique parameters .. . 109 

7.16.3. Data format .. 110 

7.16.4. Data conversion. . 110 

7.17. Normal Pulse Voltammetry technique ... . 112 

7.17.1. Description . . 112 

7.17.2. Technique parameters ... 112 

7.17.3. Data format . 13 

7.17.4. Data conversion .. 113 

7.18. Reverse Normal Pulse Voltammetry technique ......... . 115 

7.18.1. Description . . 115 

7.18.2. Technique parameters . . 115 

7.18.3. Data format . . 116 

7.18.4. Data conversion. . 116 

7.19. Differential Normal Pulse Voltammetry technique ............ . 117 

7.19.1. Description . 117 

7.19.2. Technique parameters . 117 

7.19.3. Data format . . 118 

7.19.4. Data conversion .. . 118 

7.20. Differential Pulse Amperometry technique .. . 120 

7.20.1. Description . . 120 

7.20.2. Technique parameters . . 120 

7.20.3. Data format . . 121 

7.20.4. Data conversion .. . 121 

7.21. Ecorr. Vs Time technique ..... . 122 

7.21.1. Description . . 122 

7.21.2. Technique parameters. . 122 

7.21.3. Data format . . 122 

7.21.4. Data conversion. . 123 

7.22. Linear Polarization technique . . 124 

7.22.1. Description . . 124 

7.22.2. Technique parameters . . 124 

7.22.3. Data format . . 125 

7.22.4. Data conversion .. . 126 

7.23. Generalized Corrosion technique ......... . 127 

7.23.1. Description . . 127 

7.23.2. Technique parameters . . 127 

7.23.3. Data format . . 128 

7.23.4. Data conversion. . 129 

7.24. Cyclic PotentioDynamic Polarization technique . . 130 

7.24.1. Description . . 130 

7.24.2. Technique parameters .. . 130 

7.24.3. Data format .. . 131 

7.24.4. Data conversion. . 132 

7.25. PotentioDynamic Pitting technique ..... . 133 

7.25.1. Description . . 133 

7.25.2. Technique parameters ... . 133 

7.25.3. Data format . . 134 

7.25.4. Data conversion. . 135 

7.26. PotentioStatic Pitting technique .......... . 136 

7.26.1. Description . . 136 

7.26.2. Technique parameters ... .. 136 

7.26.3. Data format . . 137 

7.26.4. Data conversion .. . 137 

7.27. Zero Resistance Ammeter technique ....... . 138 

7.27.1. Description . . 138 

7.27.2. Technique parameters .. . 138 

7.27.3. Data format . . 139 

7.27.4. Data conversion ....... .. 140 

7.28. Manual IR technique .......... . 141 

7.28.1. Description . . 141 

7.28.2. Technique parameters .. .. 141 

7.28.3. Data format . . 141 

7.28.4. Data conversion. . 141 

7.29. IR Determination with PotentioStatic Impedance technique .. .. 142 

7.29.1. Description . . 142 

7.29.2. Technique parameters .. . 142 

7.29.3. Data format . . 143 

7.29.4. Data conversion .. . 144 

7.30. IR Determination with GalvanoStatic Impedance technique . . 145 

7.30.1. Description . .. 145 

7.30.2. Technique parameters .. . 145 

7.30.3. Data format . . 146 

7.30.4. Data conversion .. . 147 

7.31. Loop technique .. . 149 

7.31.1. Description . . 149 

7.31.2. Technique parameters . .. 149 

7.31.3. Data format . . 149 

7.31.4. Data conversion .. .. 149 

7.32. Trigger Out technique .. . 150 

7.32.1. Description . . 150 

7.32.2. Technique parameters . . 150 

7.32.3. Data format .. . 150 

7.32.4. Data conversion .. . 150 

7.33. Trigger In technique ..... . 151 

7.33.1. Description . . 151 

7.33.2. Technique parameters . . 151 

7.33.3. Data format . . 151 

7.33.4. Data conversion. . 151 

7.34. Trigger Out Set technique .. . 152 

7.34.1. Description . . 152 

7.34.2. Technique parameters .. . 152 

7.34.3. Data format .. . 152 

7.34.4. Data conversion. . 152 

7.35. Large Amplitude Sinusoidal Voltammetry technique . . 153 

7.35.1. Description . . 153 

7.35.2. Technique parameters .. . 153 

7.35.3. Data format . . 154 

7.35.4. Data conversion. . 154 

7.36. Chrono-Potentiometry technique with limits ............ . 155 

7.36.1. Description . . 155 

7.36.1. Technique parameters .. . 155 

7.36.2. Data format . . 157 

7.36.3. Data conversion. . 157 

7.37. Chrono-Amperometry technique with limits .... . 158 

7.37.1. Description . . 158 

7.37.2. Technique parameters .. . 158 

7.37.3. Data format . . 160 

7.37.4. Data conversion . . 160 

7.38. Voltage Scan technique with limits. . 161 

7.38.1. Description. . 161 

7.38.2. Technique parameters .. . 161 

7.38.3. Data format . . 163 

7.38.4. Data conversion . . 163 

7.38.5. Delphi Example . . 163 

7.39. Current Scan technique with limits . . 165 

7.39.1. Description . . 165 

7.39.2. Technique parameters . . 165 

7.39.3. Data format . . 167 

7.39.4. Data conversion. . 167 

7.40. Modular Pulse technique ...... . 168 

7.40.1. Description . . 168 

7.40.2. Technique parameters . . 168 

7.40.3. Data format . . 169 

7.40.4. Data conversion. . 169 

7.41. Constant Amplitude Sinusoidal micro Galvano polarization technique . .. 170 

7.41.1. Description . . 170 

7.41.2. Technique parameters . . 170 

7.41.3. Data format .. . 171 

7.41.4. Data conversion .. . 171 

7.42. Constant Amplitude Sinusoidal micro Potentio polarization technique .. . 172 

7.42.1. Description . . 172 

7.42.2. Technique parameters .. .. 172 

7.42.3. Data format .. . 173 

7.42.4. Data conversion. . 173 

8. Global parameters for hardware configuration ... . 174 

8.1 Electrode connection ................. . 174 

8.2 Instrument ground .. . 174 

8.3 Record and external control options .. . 174 

APPENDIX A. Find instruments ...... . 177 

Calling conventions ..... . 177 

2. Multi-thread applications .... . 177 

3. Data types ... . 177 

4. Functions reference .... . 177 

5. Serializations format ... .. 181 

6. Error codes .. . 183 

APPENDIX B. Version Compatibility . . 184 

This document is a part of the BioLogic OEM Package and is protected by the terms of the OEM Package licence as well as other intellectual property rights owned by BioLogic SAS. This document may only be used for non-commercial purposes such as for the integration of BioLogic equipment to larger technical solutions manufactured and/or delivered to end-users. 

# Overview

The EC-Lab® Development Package is intended for software developers who need to integrate Bio-Logic potentiostats / galvanostats in OEM applications. This package supports the following Biologic instruments: 

<table><tr><td>VMP-3 series</td><td>VMP-300 series</td></tr><tr><td>• SP-50</td><td>• SP-200</td></tr><tr><td>• SP-50e</td><td>• SP-240</td></tr><tr><td>• SP-150</td><td>• SP-300</td></tr><tr><td>• SP-150e</td><td></td></tr><tr><td></td><td>• BP-300</td></tr><tr><td>• VSP</td><td>• VSP-300</td></tr><tr><td>• VSP-3e</td><td>• VMP-300</td></tr><tr><td>• VMP2</td><td></td></tr><tr><td>• VMP3</td><td></td></tr><tr><td>• VMP-3e</td><td></td></tr><tr><td>• BiStat</td><td></td></tr><tr><td>• HCP-803</td><td></td></tr><tr><td>• HCP-1005</td><td></td></tr><tr><td>• MPG2</td><td></td></tr></table>

The library accommodates some functionality offered by EC-Lab® software: 

Detection of connected instruments, 

Connection / disconnection to the instrument through Ethernet/USB, 

Channels initialization (firmware loading), 

Load techniques on selected channel(s) (OCV, CA, CP...), 

Start/stop selected channel(s), 

Retrieving data... 

LabVIEW® VIs examples are also provided to help the user in the integration of the library in his application. Other examples are provided, in C, C++, C#, Delphi and Python, nevertheless they may not be updated to the latest features. 

# Notes:

Theoretically, any software development tool able to call a DLL is suitable for using the $\pmb { \mathrm { E C - L a b } } \textcircled { 8 }$ Development Package $( C + + ,$ Pascal, LabVIEW®...). However limitations of some compilers may prevent a proper calling of some or all the functions. 

The EC-Lab® Development Package is provided "AS IS". No specific support is provided to this free package. There is no warranty for damages incurred using it. We strongly recommend you to test your techniques on a dummy cell before using them on a real cell. 

Use of the $\pmb { \mathrm { E C - L a b } } \textcircled { 8 }$ Development Package in a commercial software is forbidden without a written authorization of Bio-Logic SAS. 

• The software includes a LEPMI/ENSEEG/INPG license. 

# 2. Files package

The EC-Lab® Development Package is composed with the following files: 

blfind.dll: library used to find available instruments (Ethernet, USB) 

blfind64.dll: same as blfind.dll provided for 64-bits compatibility 

EClib.dll: library used to communicate with the instrument 

EClib64.dll: same as Eclib.dll provided for 64-bits compatibility 

kernel.bin: channel firmware for VMP-3 devices 

kernel4.bin: channel firmware for VMP-300 series 

Kernel5.bin: channel firmware for VMP-300 series P boards. 

Vmp_ii_0437_a4.xlx: channel firmware for VMP-3 L devices 

Vmp_ii_0437_a6.xlx: channel firmware for VMP-3 devices 

Vmp_iv_0395_aa.xlx: channel firmware for VMP-300 series 

ocv.ecc: open circuit voltage technique 

cv.ecc: cyclic voltammetry technique 

biovscan.ecc: cyclic voltammetry advanced technique 

ca.ecc: chrono-amperometry technique 

cp.ecc: chrono-potentiommetry technique 

iscan.ecc: current scan technique 

vscan.ecc: voltage scan technique 

lasv.ecc: large amplitude sinusoidal voltammetry 

load.ecc: constant load technique 

pow.ecc: constant power technique 

peis.ecc: potentio electrochemical impedance spectroscopy technique 

geis.ecc: galvano electrochemical impedance spectroscopy technique 

seisp.ecc: staircase potentiostatic impedance technique 

seisg.ecc: staircase galvanostatic Impedance technique 

dpv.ecc: differential pulse voltammetry technique 

swv.ecc: square wave voltammetry technique 

npv.ecc: normal pulse voltammetry technique 

rnpv.ecc: reverse normal pulse voltammetry technique 

dnpv.ecc: differential normal pulse voltammetry technique 

<table><tr><td>dpa.ecc:</td><td>differential pulse amperometry technique</td></tr><tr><td>evt.ecc:</td><td>ecorr. vs time technique</td></tr><tr><td>lp.ecc:</td><td>linear polarization technique</td></tr><tr><td>gc.ecc:</td><td>generalized corrosion technique</td></tr><tr><td>cpp.ecc:</td><td>cyclic potentiodynamic polarization technique</td></tr><tr><td>pdp.ecc:</td><td>potentiodynamic pitting technique</td></tr><tr><td>psp.ecc:</td><td>potentiostatic pitting technique</td></tr><tr><td>zra.ecc:</td><td>zero resistance ammeter technique</td></tr><tr><td>ircmp.ecc:</td><td>manual IR technique</td></tr><tr><td>pzir.ecc:</td><td>IR determination with potentiostatic impedance technique</td></tr><tr><td>gzir.ecc:</td><td>IR determination with galvanostatic impedance technique</td></tr><tr><td>loop.ecc:</td><td>loop technique</td></tr><tr><td>TO.ecc:</td><td>trigger out technique</td></tr><tr><td>TI.ecc:</td><td>trigger in technique</td></tr><tr><td>TOS.ecc:</td><td>trigger out set technique</td></tr><tr><td>vscanlimit.ecc:</td><td>voltage scan technique with limits</td></tr><tr><td>iscanlimit.ecc:</td><td>current scan technique with limits</td></tr><tr><td>mp.ecc:</td><td>modular pulse technique</td></tr><tr><td>casp.ecc:</td><td>constant amplitude sinusoidal micro potentio polarization technique</td></tr><tr><td>casg.ecc:</td><td>constant amplitude sinusoidal micro galvano polarization technique</td></tr><tr><td>calimit.ecc:</td><td>chrono-amperometry technique with limits</td></tr><tr><td>cplimit.ecc:</td><td>chrono-potentiometry technique with limits</td></tr></table>

These techniques are doubled with the name name4.ecc only for VMP-300 series and name5.ecc for VMP-300 series P boards. The techniques which have not these file must not to be used with VMP-300 series (e.g. do not load a file name.ecc on VMP-300 series and do not load a file name4.ecc on VMP-3 based devices). 

The biovscan, load and pow techniques are only supported for VMP-3 based devices. 

All the files of EC-Lab® Development Package must be stored in the same directory. 

# 3. General information

# 3.1. Calling conventions

The library uses the stdcall calling conventions for all exported functions. 

# 3.2. Data alignment

All the structures used by the library are aligned on double-word to simplify the communication with other programming environments. 

# 3.3. Multi-thread applications

All exported functions are protected by a synchronization object, they can be called in a multi-thread application. 

# 3.4. Data types

The library is written in object Pascal under Delphi. All data type used in this document are Pascal types. The type translation table C/C++ - Object Pascal is : 


Type translation table


<table><tr><td>C/C++ Type</td><td>ObjectPascal Type</td></tr><tr><td>unsigned short [int]</td><td>Word</td></tr><tr><td>[signed] short [int]</td><td>SmallInt</td></tr><tr><td>unsigned [int]</td><td>Cardinal { 3.25 fix }</td></tr><tr><td>[signed] int</td><td>Integer</td></tr><tr><td>UINT</td><td>LongInt { or Cardinal }</td></tr><tr><td>WORD</td><td>Word</td></tr><tr><td>DWORD</td><td>LongInt { or Cardinal }</td></tr><tr><td>unsigned long</td><td>LongInt { or Cardinal }</td></tr><tr><td>unsigned long int</td><td>LongInt { or Cardinal }</td></tr><tr><td>[signed] long</td><td>LongInt</td></tr><tr><td>[signed] long int</td><td>LongInt</td></tr><tr><td>char</td><td>Char</td></tr><tr><td>signed char</td><td>ShortInt</td></tr></table>


Type translation table


<table><tr><td>C/C++ Type</td><td>ObjectPascal Type</td></tr><tr><td>unsigned char</td><td>Byte</td></tr><tr><td>char*</td><td>PChar</td></tr><tr><td>LPSTR or PSTR</td><td>PChar</td></tr><tr><td>LPWSTR or PWSTR</td><td>PWideChar { 3.12 fix }</td></tr><tr><td>void*</td><td>Pointer</td></tr><tr><td>BOOL</td><td>Bool</td></tr><tr><td>float</td><td>Single</td></tr><tr><td>double</td><td>Double</td></tr><tr><td>long double</td><td>Extended</td></tr><tr><td>UserType*</td><td>^使用者Type</td></tr><tr><td>NULL</td><td>NIL</td></tr></table>

The following data types are used by the library: 

<table><tr><td colspan="3">Data types</td></tr><tr><td>Data types</td><td>Format</td><td>Range</td></tr><tr><td>int8</td><td>signed 8-bit</td><td>-128..127</td></tr><tr><td>int16</td><td>signed 16-bit</td><td>-32768..32767</td></tr><tr><td>int32</td><td>signed 32-bit</td><td>-</td></tr><tr><td></td><td></td><td>2147483648..2147483647</td></tr><tr><td>uint8</td><td>unsigned 8-bit</td><td>0..255</td></tr><tr><td>uint16</td><td>unsigned 16-bit</td><td>0..65535</td></tr><tr><td>uint32</td><td>unsigned 32-bit</td><td>0..4294967295</td></tr><tr><td>boolean</td><td>unsigned 8-bit</td><td>FALSE=0, TRUE=1</td></tr><tr><td>single</td><td>Single precision floating point (32 bits, 7-8 significant digits)</td><td>[1.5 x 10-45, 3.4 x 1038]</td></tr><tr><td>double</td><td>Double precision floating point (64 bits, 15-16 significant digits)</td><td>[5.0 x 10-324, 1.7 x 10308]</td></tr></table>

# 3.5. Variables description

The following variables are used by the library: 

<table><tr><td colspan="3">Variable description</td></tr><tr><td>Variable</td><td>Description</td><td>Unit</td></tr><tr><td>t</td><td>time</td><td>second (s)</td></tr><tr><td>I</td><td>Current</td><td>Ampere (A)</td></tr><tr><td>Ic</td><td>Current control</td><td>Ampere (A)</td></tr><tr><td>&lt;I&gt;</td><td>Average current</td><td>Ampere (A)</td></tr><tr><td>Ewe</td><td>WE potential versus REF</td><td>Volt (V)</td></tr><tr><td>&lt;Ewe&gt;</td><td>Average of WE potential versus REF</td><td>Volt (V)</td></tr><tr><td>Ece</td><td>CE potential versus REF</td><td>Volt (V)</td></tr><tr><td>Ewe-Ece</td><td>WE versus CE potential</td><td>Volt (V)</td></tr><tr><td>Ec</td><td>Potential control</td><td>Volt (V)</td></tr><tr><td>R</td><td>Resistor</td><td>Ohm (Ω)</td></tr><tr><td>power</td><td>Power</td><td>Watt (W)</td></tr><tr><td>cycle</td><td>Cycle number</td><td>-</td></tr><tr><td>Q</td><td>Electric charge from the beginning of the technique</td><td>Ampere(second A.s)</td></tr><tr><td>f</td><td>Frequency</td><td>Hertz (Hz)</td></tr><tr><td>phase</td><td>Angle</td><td>radian (rad)</td></tr><tr><td>|Ewe|</td><td>Module of Ewe (V)</td><td>Volt (V)</td></tr><tr><td>|Ece|</td><td>Module of Ece (V)</td><td>Volt (V)</td></tr><tr><td>|Ice|</td><td>Module of Ice (A)</td><td>Ampere (A)</td></tr><tr><td>|I|</td><td>Module of I</td><td>Ampere (A)</td></tr><tr><td>I Range</td><td>Current range</td><td>-</td></tr><tr><td>E Range</td><td>WE potential range</td><td>-</td></tr><tr><td>tb</td><td>Timebase</td><td>Microsecond (μs)</td></tr></table>

# 4. Using the library

First of all, the function BL_Connect must be called to establish the connection with the selected instrument through Ethernet or USB. The function will return a device identifier (ID) which will have to be used with all the functions of the library to communicate with this instrument. Note that one can communicate with several instruments thanks to the device identifier. 

After establishing the connection, the firmware must be loaded (if not already done) on channels plugged with the function BL_LoadFirmware to make them operational. Use the function BL_IsChannelPlugged to list channels plugged on the instrument, and the function BL_GetChannelBoardType can be used to identify the channel before loading the firmware. Once the firmware is loaded, the function BL_GetChannelInfos can return channel information such as firmware version, board version, memory size, ... 

Now the instrument is ready to receive techniques with user's parameters on selected channels thanks to the function BL_LoadTechnique. Note that the techniques are defined by the *.ecc files delivered with the library (for instance the file ocv.ecc defines the Open Circuit Voltage technique). See the section 7. Techniques for a complete description of parameters available for each technique. 

Electrochemical techniques parameters must be carefully programmed according to the instrument hardware specifications. Be aware that wrong parameters can generate faulty operations of the technique. 

Once the techniques are loaded, channels can be started (or stopped) with the function BL_StartChannel (or BL_StopChannel). One can also synchronize channels together with the functions BL_StartChannels / BL_StopChannels. 

The data can be recovered from selected channels with the function BL_GetData. The format of the returned data depends on the technique used to generate these data. One can find the identifier of this technique in the structure TDataInfos returned by the function BL_GetData. See the section 7. Techniques for a complete description of the data format for each technique. 

Once the techniques are finished and all data recovered, one must close the connection with the instrument with the function BL_Disconnect. 

# 5. Structures and Constants

# 5.1. Structures

The structure TDeviceInfos defines device information and is used by the function BL_Connect : 

<table><tr><td colspan="3">Structure TDeviceInfos</td></tr><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>DeviceCode</td><td>int32</td><td>Device code (see section 5.3. Constants)</td></tr><tr><td>RAMsize</td><td>int32</td><td>RAM size, in MBytes</td></tr><tr><td>CPU</td><td>int32</td><td>Computer board cpu</td></tr><tr><td>NumberOfChannels</td><td>int32</td><td>Number of channels connected</td></tr><tr><td>NumberOfSlots</td><td>int32</td><td>Number of slots available</td></tr><tr><td>FirmwareVersion</td><td>int32</td><td>Communication firmware version</td></tr><tr><td>FirmwareDate_yyyy</td><td>int32</td><td>Communication firmware date YYYYY</td></tr><tr><td>FirmwareDate_mm</td><td>int32</td><td>Communication firmware date MM</td></tr><tr><td>FirmwareDate_dd</td><td>int32</td><td>Communication firmware date DD</td></tr><tr><td>HTdisplayOn</td><td>int32</td><td>Allow hyper-terminal prints (true/false)</td></tr><tr><td>NbOfConnectedPC</td><td>int32</td><td>Number of connected PC</td></tr></table>

The structure TChannelInfos defines channel information and is used by the function BL_GetChannelInfos : 

<table><tr><td colspan="3">Structure TChannelInfos</td></tr><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>Channel</td><td>int32</td><td>Channel (0..15)</td></tr><tr><td>BoardVersion</td><td>int32</td><td>Board version</td></tr><tr><td>BoardSerialNumber</td><td>int32</td><td>Board serial number</td></tr><tr><td>FirmwareCode</td><td>int32</td><td>Identifier of the firmware loaded on the channel (see section 5.3. Constants)</td></tr><tr><td>FirmwareVersion</td><td>int32</td><td>Firmware version</td></tr><tr><td>XilinxVersion</td><td>int32</td><td>Xilinx version</td></tr><tr><td>AmpCode</td><td>int32</td><td>Amplifier code (see section 5.3. Constants)</td></tr><tr><td>NbAmps</td><td>int32</td><td>Number of amplifiers (0..16)</td></tr><tr><td>Lcboard</td><td>int32</td><td>Low current board present (= 1)</td></tr><tr><td>Zboard</td><td>int32</td><td>TRUE if channel with impedance capabilities</td></tr><tr><td>RESERVED</td><td>int32</td><td>not used</td></tr><tr><td>RESERVED</td><td>int32</td><td>not used</td></tr><tr><td>MemSize</td><td>int32</td><td>Memory size (in bytes)</td></tr><tr><td>MemFilled</td><td>int32</td><td>Memory filled (in bytes)</td></tr><tr><td>State</td><td>int32</td><td>Channel state : run/stop/pause (see section 5.3. Constants)</td></tr><tr><td>MaxIRange</td><td>int32</td><td>Maximum I range allowed (see section 5.3. Constants)</td></tr><tr><td>MinIRange</td><td>int32</td><td>Minimum I range allowed (see section 5.3. Constants)</td></tr><tr><td>MaxBandwidth</td><td>int32</td><td>Maximum bandwidth allowed (see section 5.3. Constants)</td></tr><tr><td>NbOfTechniques</td><td>int32</td><td>Number of techniques loaded</td></tr></table>

The structure TCurrentValues defines channel current values (Ewe, Ece, I, ...) and is used by functions BL_GetCurrentValues and BL_GetData : 

<table><tr><td colspan="3">Structure TCCurrentValues</td></tr><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>State</td><td>int32</td><td>Channel state : run/stop/pause (see section 5.3. Constants)</td></tr><tr><td>MemFilled</td><td>int32</td><td>Memory filled (in Bytes)</td></tr><tr><td>TimeBase</td><td>single</td><td>Time base (s)</td></tr><tr><td>Ewe</td><td>single</td><td>Working electrode potential (V)</td></tr><tr><td>EweRangeMin</td><td>single</td><td>Ewe min range (V)</td></tr><tr><td>EweRangeMax</td><td>single</td><td>Ewe max range (V)</td></tr><tr><td>Ece</td><td>single</td><td>Counter electrode potential (V)</td></tr><tr><td>EceRangeMin</td><td>single</td><td>Ece min range (V)</td></tr><tr><td>EceRangeMax</td><td>single</td><td>Ece max range (V)</td></tr><tr><td>Eoverflow</td><td>int32</td><td>Potential overflow</td></tr><tr><td>I</td><td>single</td><td>Current value (A)</td></tr><tr><td>IRange</td><td>int32</td><td>Current range (see section 5.3. Constants)</td></tr><tr><td>Ioverflow</td><td>int32</td><td>Current overflow</td></tr></table>


Structure TCurrentValues


<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>ElapsedTime</td><td>single</td><td>Elapsed time (s)</td></tr><tr><td>Freq</td><td>single</td><td>Frequency (Hz)</td></tr><tr><td>Rcomp</td><td>single</td><td>R compensation (Ohm)</td></tr><tr><td>Saturation</td><td>int32</td><td>E or/and I saturation</td></tr><tr><td>OptErr</td><td>int32</td><td>Hardware Option Error Code (see section 5.4. Error codes)</td></tr><tr><td>OptPos</td><td>int32</td><td>Index of the option generating the OptErr (VMP-300 series only, otherwise 0)</td></tr></table>

The structure TDataInfos defines data information (i.e. information on the data saved in the data buffer) and is used by the function BL_GetData : 


Structure TDataInfos


<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>IRQskipped</td><td>int32</td><td>Number of IRQ skipped</td></tr><tr><td>NbRows</td><td>int32</td><td>Number of rows in the data buffer, i.e. number of points saved in the data buffer</td></tr><tr><td>NbCols</td><td>int32</td><td>Number of columns in the data buffer, i.e. number of variables defining a point in the data buffer</td></tr><tr><td>TechniqueIndex</td><td>int32</td><td>Index (0-based) of the technique which has generated the data. This field is only useful for linked techniques</td></tr><tr><td>TechniqueID</td><td>int32</td><td>Identifier of the technique which has generated the data. Must be used to identify the data format in the data buffer (see section 5.3. Constants)</td></tr><tr><td>ProcessIndex</td><td>Int32</td><td>Index (0-based) of the process of the technique which has generated the data. Must be used to identify the data format in the data buffer</td></tr><tr><td>loop</td><td>int32</td><td>Loop number</td></tr><tr><td>StartTime</td><td>double</td><td>Start time (s)</td></tr><tr><td>MuxPad</td><td>int32</td><td>(no longer used)</td></tr></table>

The array TDataBuffer is used to retrieve data from the device by the function BL_GetData: 

# Type TDataBuffer

TDataBuffer $=$ array[1..1000] of uint32; 

PDataBuffer $=$ ^TDataBuffer 

The structure TEccParam defines an elementary technique parameter and is used by the function BL_LoadTechnique: 

# Structure TEccParam

<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>ParamStr</td><td>array[1..64] of char</td><td>string defining the parameter label (see section 7. Techniques for a complete description of parameters available for each technique)</td></tr><tr><td>ParamType</td><td>int32</td><td>Parameter type (0=int32, 1=boolean, 2=single)</td></tr><tr><td>ParamVal</td><td>int32</td><td>Parameter value (WARNING: numerical value)</td></tr><tr><td>ParamIndex</td><td>int32</td><td>Parameter index (0-based). Useful for multi-step parameters only.</td></tr></table>

# Type PEccParam

PEccParam $=$ ^TEccParam 

The structure TEccParams defines an array of elementary technique parameters and is used by the function BL_LoadTechnique: 

# Structure TEccParams : array of elementary technique parameters

<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>len</td><td>int32</td><td>Length of the array pointed to by pParams</td></tr><tr><td>pParams</td><td>PEccParam</td><td>Pointer on the array of technique parameters (array of structure TEccParam)</td></tr></table>

# Structure THardwareConf : hardware configuration

<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>Conn</td><td>int32</td><td>Electrode connection for constant value of this parameter see 5. Structures and Constants</td></tr><tr><td>Ground</td><td>int32</td><td>Instrument ground</td></tr><tr><td colspan="3">Structure THardwareConf : hardware configuration</td></tr><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td></td><td></td><td>for constant value of this parameter see 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td></tr><tr><td colspan="3">Type PHardwareConf</td></tr><tr><td colspan="3">PHardwareConf = ^THardwareConf</td></tr></table>

# 5.2. Other structures

# 5.2.1. Labview Structures

The structures below are defined for LabVIEW compatibility for technique parameter loading and are used by the function BL_LoadTechnique_LV. 

<table><tr><td colspan="3">Structure TArrayOfChar_LV : array of char</td></tr><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>dimSize</td><td>int32</td><td>Length of the array</td></tr><tr><td>FirstChar</td><td>char</td><td>First element in the array of char</td></tr></table>

# Type PPArrayOfChar_LV

PArrayOfChar_LV $=$ ^TArrayOfChar_LV; 

PPArrayOfChar_LV $=$ ^PArrayOfChar_LV; 

# Structure TEccParam_LV : elementary technique parameter

<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>ParamStr</td><td>PPArrayOfChar_LV</td><td>string defining the parameter (see section 7. Techniques for a complete description of parameters available for each technique)</td></tr><tr><td>ParamType</td><td>int32</td><td>Parameter type (0=int32, 1=boolean, 2=single)</td></tr><tr><td>ParamVal</td><td>int32</td><td>Parameter value (WARNING : numerical value)</td></tr><tr><td>ParamIndex</td><td>int32</td><td>Parameter index (0-based). Useful for multi-step parameters only.</td></tr></table>

# Structure TEccParams_LV : array of elementary technique parameters

<table><tr><td>Field name</td><td>Data type</td><td>Description</td></tr><tr><td>dimSize</td><td>int32</td><td>Length of the array</td></tr><tr><td>FirstEccParam_LV</td><td>TEccParam_LV</td><td>First element in the array</td></tr></table>

# Type PPEccParams_LV

PEccParams_LV $=$ ^TEccParams_LV 

PPEccParams_LV $=$ ^PEccParams_LV 

# 5.3. Constants


Device constants (used by the function BL_Connect)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_DEV_VMP</td><td>0</td><td>VMP device</td></tr><tr><td>KBIO_DEV_VMP2</td><td>1</td><td>VMP2 device</td></tr><tr><td>KBIO_DEV_MPG</td><td>2</td><td>MPG device</td></tr><tr><td>KBIO_DEV_BISTAT</td><td>3</td><td>BISTAT device</td></tr><tr><td>KBIO_DEV_MCS_200</td><td>4</td><td>MCS-200 device</td></tr><tr><td>KBIO_DEV_VMP3</td><td>5</td><td>VMP3 device</td></tr><tr><td>KBIO_DEV_VSP</td><td>6</td><td>VSP device</td></tr><tr><td>KBIO_DEV_HCP803</td><td>7</td><td>HCP-803 device</td></tr><tr><td>KBIO_DEV_EPP400</td><td>8</td><td>EPP-400 device</td></tr><tr><td>KBIO_DEV_EPP4000</td><td>9</td><td>EPP-4000 device</td></tr><tr><td>KBIO_DEV_BISTAT2</td><td>10</td><td>BISTAT 2 device</td></tr><tr><td>KBIO_DEV_FCT150S</td><td>11</td><td>FCT-150S device</td></tr><tr><td>KBIO_DEV_VMP300</td><td>12</td><td>VMP-300 device</td></tr><tr><td>KBIO_DEV_SP50</td><td>13</td><td>SP-50 device</td></tr><tr><td>KBIO_DEV_SP150</td><td>14</td><td>SP-150 device</td></tr><tr><td>KBIO_DEV_FCT50S</td><td>15</td><td>FCT-50S device</td></tr><tr><td>KBIO_DEV_SP300</td><td>16</td><td>SP300 device</td></tr><tr><td>KBIO_DEV_CLB500</td><td>17</td><td>CLB-500 device</td></tr><tr><td>KBIO_DEV_HCP1005</td><td>18</td><td>HCP-1005 device</td></tr><tr><td>KBIO_DEV_CLB2000</td><td>19</td><td>CLB-2000 device</td></tr><tr><td>KBIO_DEV_VSP300</td><td>20</td><td>VSP-300 device</td></tr><tr><td>KBIO_DEV_SP200</td><td>21</td><td>SP-200 device</td></tr><tr><td>KBIO_DEV MPG2</td><td>22</td><td>MPG2 device</td></tr><tr><td>KBIO_DEV_ND1</td><td>23</td><td>RESERVED</td></tr><tr><td>KBIO_DEV_ND2</td><td>24</td><td>RESERVED</td></tr><tr><td>KBIO_DEV_ND3</td><td>25</td><td>RESERVED</td></tr><tr><td>KBIO_DEV_ND4</td><td>26</td><td>RESERVED</td></tr></table>


Device constants (used by the function BL_Connect)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_DEV_SP240</td><td>27</td><td>SP-240 device</td></tr><tr><td>KBIO_DEV_MPG205</td><td>28</td><td>MPG-205 (VMP3)</td></tr><tr><td>KBIO_DEV_MPG210</td><td>29</td><td>MPG-210 (VMP3)</td></tr><tr><td>KBIO_DEV_MPG220</td><td>30</td><td>MPG-220 (VMP3)</td></tr><tr><td>KBIO_DEV_MPG240</td><td>31</td><td>MPG-240 (VMP3)</td></tr><tr><td>KBIO_DEV_BP300</td><td>32</td><td>BP-300 (VMP300)</td></tr><tr><td>KBIO_DEV_VMP3e</td><td>33</td><td>VMP-3e (VMP3)</td></tr><tr><td>KBIO_DEV_VSP3e</td><td>34</td><td>VSP-3e (VMP3)</td></tr><tr><td>KBIO_DEV_SP50E</td><td>35</td><td>SP-50e (VMP3)</td></tr><tr><td>KBIO_DEV_SP150E</td><td>36</td><td>SP-150e (VMP3)</td></tr><tr><td>KBIO_DEV UNKNOWN</td><td>255</td><td>Unknown device</td></tr></table>


Firmware code constants (used by the structure TChannelInfos)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_FIRM_NON</td><td>0</td><td>No firmware loaded</td></tr><tr><td>KBIO_FIRM_INTERPR</td><td>1</td><td>Firmware for EC-Lab® software</td></tr><tr><td>KBIO_FIRM UNKNOWN</td><td>4</td><td>Unknown firmware loaded</td></tr><tr><td>KBIO_FIRM_KERNEL</td><td>5</td><td>Firmware for the library</td></tr><tr><td>KBIO_FIRM_INVALID</td><td>8</td><td>Invalid firmware loaded</td></tr><tr><td>KBIO_FIRM_ECAL</td><td>10</td><td>Firmware for calibration software</td></tr></table>


Amplifier constants (used by the structure TChannelInfos)


<table><tr><td>Constant</td><td>Value</td><td>Description</td><td>Device Family</td></tr><tr><td>KBIO_AMPL_NON</td><td>0</td><td>No amplifier</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_2A</td><td>1</td><td>Amplifier 2 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_1A</td><td>2</td><td>Amplifier 1 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_5A</td><td>3</td><td>Amplifier 5 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_10A</td><td>4</td><td>Amplifier 10 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_20A</td><td>5</td><td>Amplifier 20 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_HEUS</td><td>6</td><td>reserved</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_LC</td><td>7</td><td>Low current amplifier</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_80A</td><td>8</td><td>Amplifier 80 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_4AI</td><td>9</td><td>Amplifier 4 A</td><td>VMP3 series</td></tr><tr><td colspan="4">Amplifier constants (used by the structure TChannelInfos)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td><td>Device Family</td></tr><tr><td>KBIO_AMPL_PAC</td><td>10</td><td>Fuel Cell Tester</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_4AI_VSP</td><td>11</td><td>Amplifier 4 A (VSP instrument)</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_LC_VSP</td><td>12</td><td>Low current amplifier (VSP instrument)</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_UNDEF</td><td>13</td><td>Undefined amplifier</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_MUIC</td><td>14</td><td>reserved</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_ERROR</td><td>15</td><td>No amplifier in case of error</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_8AI</td><td>16</td><td>Amplifier 8 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_LB500</td><td>17</td><td>Amplifier LB500</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_100A5V</td><td>18</td><td>Amplifier 100 A</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_LB2000</td><td>19</td><td>Amplifier LB2000</td><td>VMP3 series</td></tr><tr><td>KBIO_AMPL_1A48V</td><td>20</td><td>Amplifier 1A 48V</td><td>VMP-300 series</td></tr><tr><td>KBIO_AMPL_4A14V</td><td>21</td><td>Amplifier 4A 14V</td><td>VMP-300 series</td></tr><tr><td>KBIO_AMPL_5A_MPG2B</td><td>22</td><td>Amplifier 5A</td><td>MPG-205</td></tr><tr><td>KBIO_AMPL_10A_MPG2B</td><td>23</td><td>Amplifier 10A</td><td>MPG-210</td></tr><tr><td>KBIO_AMPL_20A_MPG2B</td><td>24</td><td>Amplifier 20A</td><td>MPG-220</td></tr><tr><td>KBIO_AMPL_40A_MPG2B</td><td>25</td><td>Amplifier 40A</td><td>MPG-240</td></tr><tr><td>KBIO_AMPL_COIN_CELL_HOLDER</td><td>26</td><td>Coin cell holder amplifier</td><td></td></tr><tr><td>KBIO_AMPL4_10A5V</td><td>27</td><td>Amplifier 10A 5V</td><td>VMP-300 series</td></tr><tr><td>KBIO_AMPL4_2A30V</td><td>28</td><td>Amplifier 2A 30V</td><td>VMP-300 series</td></tr><tr><td>KBIO_AMPL4_1A48VP</td><td>129</td><td>Amplifier 1A 48VP</td><td>VMP-300 series</td></tr></table>


I range constants (used by the structures TDataInfos and TEccParam)


<table><tr><td>Constant</td><td>Value</td><td>Description</td><td>Device Family</td></tr><tr><td>KBIO_IRANGE_KEEP</td><td>-1</td><td>Keep previous I range</td><td>VMP-300 series</td></tr><tr><td>KBIO_IRANGE_100pA</td><td>0</td><td>I range 100 pA</td><td>VMP-300 series</td></tr><tr><td>KBIO_IRANGE_1nA</td><td>1</td><td>I range 1 nA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_10nA</td><td>2</td><td>I range 10 nA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_100nA</td><td>3</td><td>I range 100 nA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_1uA</td><td>4</td><td>I range 1 μA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_10uA</td><td>5</td><td>I range 10 μA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_100uA</td><td>6</td><td>I range 100 μA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_1mA</td><td>7</td><td>I range 1 mA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_10mA</td><td>8</td><td>I range 10 mA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_100mA</td><td>9</td><td>I range 100 mA</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_1A</td><td>10</td><td>I range 1 A</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_BOOSTER</td><td>11</td><td>Booster</td><td>VMP3 /VMP-300 series</td></tr></table>


I range constants (used by the structures TDataInfos and TEccParam)


<table><tr><td>Constant</td><td>Value</td><td>Description</td><td>Device Family</td></tr><tr><td>KBIO_IRANGE_AUTO</td><td>12</td><td>Auto range</td><td>VMP3 /VMP-300 series</td></tr><tr><td>KBIO_IRANGE_10pA</td><td></td><td>IRANGE_100pA + Igain x10</td><td>VMP-300 series</td></tr><tr><td>KBIO_IRANGE_1pA</td><td></td><td>IRANGE_100pA + Igain x100</td><td>VMP-300 series</td></tr></table>


E range constants (used by the structures TDataInfos and TEccParam)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_ERANGE_2_5</td><td>0</td><td>±2.5 V</td></tr><tr><td>KBIO_ERANGE_5</td><td>1</td><td>±5 V</td></tr><tr><td>KBIO_ERANGE_10</td><td>2</td><td>±10 V</td></tr><tr><td>KBIO_ERANGE_AUTO</td><td>3</td><td>Auto range</td></tr></table>


Bandwidth constants (used by the structures TDataInfos and TEccParam)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_BW_KEEP</td><td>-1</td><td>Keep previous bandwidth</td></tr><tr><td>KBIO_BW_1</td><td>1</td><td>Bandwidth #1</td></tr><tr><td>KBIO_BW_2</td><td>2</td><td>Bandwidth #2</td></tr><tr><td>KBIO_BW_3</td><td>3</td><td>Bandwidth #3</td></tr><tr><td>KBIO_BW_4</td><td>4</td><td>Bandwidth #4</td></tr><tr><td>KBIO_BW_5</td><td>5</td><td>Bandwidth #5</td></tr><tr><td>KBIO_BW_6</td><td>6</td><td>Bandwidth #6</td></tr><tr><td>KBIO_BW_7</td><td>7</td><td>Bandwidth #7</td></tr><tr><td>KBIO_BW_8</td><td>8</td><td>Bandwidth #8 (only with VMP-300 series)</td></tr><tr><td>KBIO_BW_9</td><td>9</td><td>Bandwidth #9 (only with VMP-300 series)</td></tr></table>


Filter Constants (used by the structures TDataInfos and TEccParam)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_FILTER_RSRVD</td><td>-1</td><td>Reserved value, should not be used</td></tr><tr><td>KBIO_FILTER_NONE</td><td>0</td><td>Full band</td></tr><tr><td>KBIO_FILTER_50KHZ</td><td>1</td><td>50 kHz filter</td></tr><tr><td>KBIO_FILTER_1KHZ</td><td>2</td><td>1 kHz filter</td></tr><tr><td>KBIO_FILTER_5HZ</td><td>3</td><td>5 Hz filter</td></tr><tr><td colspan="3">Electrode connection constants (used by the structure THardwareConf)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_CONN_STD</td><td>0</td><td>Standard connection</td></tr><tr><td>KBIO_CONN CETOGRND</td><td>1</td><td>CE to ground connection</td></tr><tr><td>KBIO_CONN_WETOGRND</td><td>2</td><td>WE to ground connection</td></tr><tr><td>KBIO_CONN_HV</td><td>3</td><td>High Voltage connection (0-50V measure)</td></tr></table>


Channel mode constants (used only with VMP-300 series, by the structure THardwareConf)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_MODE_GROUNDED</td><td>0</td><td>Grounded mode</td></tr><tr><td>KBIO_MODE_FLOATING</td><td>1</td><td>Floating mode</td></tr></table>


Technique identifier constants (used by the structure TDataInfos)


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_TECHID_NONE</td><td>0</td><td>None</td></tr><tr><td>KBIO_TECHID_OCV</td><td>100</td><td>Open Circuit Voltage (Rest) identifier</td></tr><tr><td>KBIO_TECHID_CA</td><td>101</td><td>Chrono-amperometry identifier</td></tr><tr><td>KBIO_TECHID_CP</td><td>102</td><td>Chrono-potentiometry identifier</td></tr><tr><td>KBIO_TECHID_CV</td><td>103</td><td>Cyclic Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_PEIS</td><td>104</td><td>Potentio Electrochemical Impedance Spectroscopy identifier</td></tr><tr><td>KBIO_TECHID_POTPULSE</td><td>105</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_GALPULSE</td><td>106</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_GEIS</td><td>107</td><td>Galvano Electrochemical Impedance Spectroscopy identifier</td></tr><tr><td>KBIO_TECHIDSTACKPEIS_SLAVE</td><td>108</td><td>Potentio Electrochemical Impedance Spectroscopy on stack identifier</td></tr><tr><td>KBIO_TECHIDSTACKPEIS</td><td>109</td><td>Potentio Electrochemical Impedance Spectroscopy on stack identifier</td></tr><tr><td>KBIO_TECHID_CPOWER</td><td>110</td><td>Constant Power identifier</td></tr><tr><td>KBIO_TECHID_CLOAD</td><td>111</td><td>Constant Load identifier</td></tr><tr><td>KBIO_TECHID_FCT</td><td>112</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_SPEIS</td><td>113</td><td>Staircase Potentio Electrochemical Impedance Spectroscopy identifier</td></tr><tr><td>KBIO_TECHID_SGEIS</td><td>114</td><td>Staircase Galvano Electrochemical Impedance Spectroscopy identifier</td></tr><tr><td>KBIO_TECHID_STACKPDYN</td><td>115</td><td>Potentio dynamic on stack identifier</td></tr><tr><td>KBIO_TECHID_STACKPDYN_SLAVE</td><td>116</td><td>Potentio dynamic on stack identifier</td></tr><tr><td colspan="3">Technique identifier constants (used by the structure TDataInfos)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_TECHIDSTACKGDYN</td><td>117</td><td>Galvano dynamic on stack identifier</td></tr><tr><td>KBIO_TECHIDSTACKGEIS_SLAVE</td><td>118</td><td>Galvano Electrochemical Impedance Spectroscopy on stack identifier</td></tr><tr><td>KBIO_TECHIDSTACKGEIS</td><td>119</td><td>Galvano Electrochemical Impedance Spectroscopy on stack identifier</td></tr><tr><td>KBIO_TECHIDSTACKGDYN_SLAVE</td><td>120</td><td>Galvano dynamic on stack identifier</td></tr><tr><td>KBIO_TECHID_CPO</td><td>121</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_CGA</td><td>122</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_COKINE</td><td>123</td><td>(unused)</td></tr><tr><td>KBIO_TECHID_PDYN</td><td>124</td><td>Potentio dynamic identifier</td></tr><tr><td>KBIO_TECHID_GDYN</td><td>125</td><td>Galvano dynamic identifier</td></tr><tr><td>KBIO_TECHID_CVA</td><td>126</td><td>Cyclic Voltammetry Advanced identifier</td></tr><tr><td>KBIO_TECHID_DPV</td><td>127</td><td>Differential Pulse Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_SWV</td><td>128</td><td>Square Wave Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_NPV</td><td>129</td><td>Normal Pulse Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_RNPV</td><td>130</td><td>Reverse Normal Pulse Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_DNPV</td><td>131</td><td>Differential Normal Pulse Voltammetry identifier</td></tr><tr><td>KBIO_TECHID_DPA</td><td>132</td><td>Differential Pulse Amperometry identifier</td></tr><tr><td>KBIO_TECHID_EVT</td><td>133</td><td>Ecorr vs. time identifier</td></tr><tr><td>KBIO_TECHID_LP</td><td>134</td><td>Linear Polarization identifier</td></tr><tr><td>KBIO_TECHID_GC</td><td>135</td><td>Generalized corrosion identifier</td></tr><tr><td>KBIO_TECHIDCPP</td><td>136</td><td>Cyclic Potentiodynamic Polarization identifier</td></tr><tr><td>KBIO_TECHID_PDP</td><td>137</td><td>Potentiodynamic Pitting identifier</td></tr><tr><td>KBIO_TECHID_PSP</td><td>138</td><td>Potentiostatic Pitting identifier</td></tr><tr><td>KBIO_TECHID_ZRA</td><td>139</td><td>Zero Resistance Ammeter identifier</td></tr><tr><td>KBIO_TECHID_MIR</td><td>140</td><td>Manual IR identifier</td></tr><tr><td>KBIO_TECHID_PZIR</td><td>141</td><td>IR Determination with Potentiostatic Impedance identifier</td></tr><tr><td>KBIO_TECHID_GZIR</td><td>142</td><td>IR Determination with Galvanostatic Impedance identifier</td></tr><tr><td>KBIO_TECHID_LOOP</td><td>150</td><td>Loop (used for linked techniques) identifier</td></tr><tr><td>KBIO_TECHID_TO</td><td>151</td><td>Trigger Out identifier</td></tr><tr><td>KBIO_TECHID_TI</td><td>152</td><td>Trigger In identifier</td></tr><tr><td>KBIO_TECHID_TOS</td><td>153</td><td>Trigger Set identifier</td></tr><tr><td>KBIO_TECHID CPLIMIT</td><td>155</td><td>Chrono-potentiometry with limits identifier</td></tr><tr><td>KBIO_TECHID_GDYNLIMIT</td><td>156</td><td>Galvano dynamic with limits identifier</td></tr><tr><td>KBIO_TECHID_CALIMIT</td><td>157</td><td>Chrono-amperometry with limits identifier</td></tr><tr><td>KBIO_TECHID_PDYNLIMIT</td><td>158</td><td>Potentio dynamic with limits identifier</td></tr><tr><td>KBIO_TECHID_LASV</td><td>159</td><td>Large amplitude sinusoidal voltammetry</td></tr><tr><td>KBIO_TECHID_MP</td><td>167</td><td>Modular Pulse</td></tr><tr><td>KBIO_TECHID_CASG</td><td>169</td><td>Constant amplitude sinusoidal micro galvano polarization</td></tr><tr><td>KBIO_TECHID_CASP</td><td>170</td><td>Constant amplitude sinusoidal micro potentio polarization</td></tr></table>

<table><tr><td colspan="3">Channel state constants (used by the structure TChannelInfos)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>KBIO_STATE_STOP</td><td>0</td><td>Channel is stopped</td></tr><tr><td>KBIO_STATE Runs</td><td>1</td><td>Channel is running</td></tr><tr><td>KBIO_STATE_PAUSE</td><td>2</td><td>Channel is paused</td></tr></table>

<table><tr><td colspan="3">Parameter type constants (used by the structure TEccParam)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>PARAM_INT32</td><td>0</td><td>Parameter type = int32</td></tr><tr><td>PARAMBOOLEAN</td><td>1</td><td>Parameter type = boolean</td></tr><tr><td>PARAM_SINGLE</td><td>2</td><td>Parameter type = single</td></tr></table>

<table><tr><td colspan="3">Channel type constant (returned by BL_GetChannelBoardType)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>BOARD_TYPE_CHANNEL UNKNOWN</td><td>0</td><td>The channel board could not be identified or is not supported</td></tr><tr><td>BOARD_TYPE_CHANNEL_ESSENTIAL</td><td>1</td><td>The channel is an VMP-3 series board</td></tr><tr><td>BOARD_TYPE_CHANNEL_PREMIUM</td><td>2</td><td>The channel is a VMP-300 series board</td></tr><tr><td>BOARD_TYPE_CHANNEL_PREMIUM_P</td><td>3</td><td>The channel is a VMP-300 series P board</td></tr></table>

# 5.4. Error codes

All functions (with a few exceptions) exported by the library return a signed 32-bit as a result. If the function succeeded the returned value is 0, otherwise the returned value is negative, as described below: 


General error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_GEN_NOTCONNECTED</td><td>-1</td><td>no instrument connected</td></tr><tr><td>ERR_GEN CONNECTIONINPROGRESS</td><td>-2</td><td>connection in progress</td></tr><tr><td>ERR_GEN_CHANNELNOTPLUGGED</td><td>-3</td><td>selected channel(s) unplugged</td></tr><tr><td>ERR_GEN_INVALIDPARAMETERS</td><td>-4</td><td>invalid function parameters</td></tr><tr><td>ERR_GEN_FILENOTEXISTS</td><td>-5</td><td>selected file does not exist</td></tr><tr><td>ERR_GEN_FUNCTIONFAILED</td><td>-6</td><td>function failed</td></tr><tr><td>ERR_GEN_NOCHANNELECTED</td><td>-7</td><td>no channel selected</td></tr><tr><td>ERR_GEN_INVALIDCONF</td><td>-8</td><td>invalid instrument configuration</td></tr><tr><td>ERR_GEN_ECLAB_LOADATED</td><td>-9</td><td>EC-Lab® firmware loaded in the instrument</td></tr><tr><td>ERR_GEN_libNOTCORRECTLYLOADATED</td><td>-10</td><td>library not correctly loaded in memory</td></tr><tr><td>ERR_GEN_USBLIBRARYERROR</td><td>-11</td><td>USB library not correctly loaded in memory</td></tr><tr><td>ERR_GEN_FUNCTIONINPROGRESS</td><td>-12</td><td>function of the library already in progress</td></tr><tr><td>ERR_GEN_CHANNEL_RUNNING</td><td>-13</td><td>selected channel(s) already used</td></tr><tr><td>ERR_GEN_DEVICE_NOTALLOWED</td><td>-14</td><td>device not allowed</td></tr><tr><td>ERR_GEN_UPDATEPARAMETERS</td><td>-15</td><td>Invalid update function parameters</td></tr></table>


Instrument error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_INSTR_VMEERROR</td><td>-101</td><td>internal instrument communication failed</td></tr><tr><td>ERR_INSTR_TOOMANYDATA</td><td>-102</td><td>too many data to transfer from the instrument (device error)</td></tr><tr><td>ERR_INSTR_RESPNOTPOSSIBLE</td><td>-103</td><td>selected channel(s) unplugged (device error)</td></tr><tr><td>ERR_INSTR_RESPERROR</td><td>-104</td><td>Instrument response error</td></tr><tr><td>ERR_INSTR_MSGSIZEERROR</td><td>-105</td><td>Invalid message size</td></tr></table>


Communication error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERRCOMMCOMMFAILED</td><td>-200</td><td>communication failed with the instrument</td></tr><tr><td colspan="3">Communication error codes</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERRCOMM CONNECTIONFAILED</td><td>-201</td><td>cannot establish connection with the instrument</td></tr><tr><td>ERRCOMM_WAITINGACK</td><td>-202</td><td>waiting for the instrument response</td></tr><tr><td>ERRCOMM_INVALIDIPADDRESS</td><td>-203</td><td>invalid IP address</td></tr><tr><td>ERRCOMMALLOCMEMFAILED</td><td>-204</td><td>cannot allocate memory in the instrument</td></tr><tr><td>ERRCOMM_LOADFIRMWAREFAILED</td><td>-205</td><td>cannot load firmware on the selected channel(s)</td></tr><tr><td>ERRCOMM_INCOMPATIBLESERVER</td><td>-206</td><td>communication firmware not compatible with the library</td></tr><tr><td>ERRCOMM_MAXCONNREACHED</td><td>-207</td><td>maximum number of allowed connections reached</td></tr></table>

<table><tr><td colspan="3">Firmware error codes</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_FIRM_FIRMFILENOTEXISTS</td><td>-300</td><td>cannot find kernel.bin file</td></tr><tr><td>ERR_FIRM_FIRMFILEACCESSFAILED</td><td>-301</td><td>cannot read kernel.bin file</td></tr><tr><td>ERR_FIRM_FIRMINVALIDFILE</td><td>-302</td><td>invalid kernel.bin file</td></tr><tr><td>ERR_FIRM_FIRMLOADINGFAILED</td><td>-303</td><td>cannot load kernel.bin on the selected channel(s)</td></tr><tr><td>ERR_FIRM_XILFILENOTEXISTS</td><td>-304</td><td>cannot find FPGA firmware file</td></tr><tr><td>ERR_FIRM_XILFILEACCESSFAILED</td><td>-305</td><td>cannot read FPGA firmware file</td></tr><tr><td>ERR_FIRM_XILINVALIDFILE</td><td>-306</td><td>invalid FPGA firmware file</td></tr><tr><td>ERR_FIRM_XILLOADINGFAILED</td><td>-307</td><td>cannot load FPGA firmware file on the selected channel(s)</td></tr><tr><td>ERR_FIRM_FIRMWARENOTLOADED</td><td>-308</td><td>no firmware loaded on the selected channel(s)</td></tr><tr><td>ERR_FIRM_FIRMWAREINCOMPATIBLE</td><td>-309</td><td>loaded firmware not compatible with the library</td></tr></table>

<table><tr><td colspan="3">Technique error codes</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_TECH_ECCFILENOTEXISTS</td><td>-400</td><td>cannot find the selected ECC file</td></tr><tr><td>ERR_TECH_INCOMPATIBLEECC</td><td>-401</td><td>ECC file not compatible with the channel firmware</td></tr><tr><td>ERR_TECH_ECCFILECORRUPTED</td><td>-402</td><td>ECC file is corrupted</td></tr><tr><td>ERR_TECH_LOADTECHNIQUEFAILED</td><td>-403</td><td>cannot load the ECC file</td></tr><tr><td>ERR_TECH_DATAACORRUPTED</td><td>-404</td><td>data returned by the instrument are corrupted</td></tr><tr><td>ERR_TECH_MEMFULL</td><td>-405</td><td>cannot load techniques: full memory</td></tr><tr><td colspan="3">VMP-300 series hardware options error codes (TCurrentValuesOPTErr)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_NO_ERROR</td><td>0</td><td>No error found</td></tr><tr><td>ERR_OPT_CHANGES</td><td>1</td><td>Number of options changed</td></tr><tr><td>ERR_OPEN</td><td>2</td><td>Open-in signal was asserted</td></tr><tr><td>ERR_ICRCMP_OVR</td><td>3</td><td>R Compensation overflow</td></tr><tr><td>ERR_OPT_4A</td><td>100</td><td>4A amplifier unknown error</td></tr><tr><td>ERR_OPT_4A_OVRTEMP</td><td>101</td><td>4A amplifier temperature overflow</td></tr><tr><td>ERR_OPT_4A_BADPOW</td><td>102</td><td>4A amplifier bad power</td></tr><tr><td>ERR_OPT_4A_POWFAIL</td><td>103</td><td>4A amplifier power fail</td></tr><tr><td>ERR_OPT_48V</td><td>200</td><td>48V amplifier unknown error</td></tr><tr><td>ERR_OPT_48V_OVRTEMP</td><td>201</td><td>48V amplifier temperature overflow</td></tr><tr><td>ERR_OPT_48V_BADPOW</td><td>202</td><td>48V amplifier bad power</td></tr><tr><td>ERR_OPT_48V_POWFAIL</td><td>203</td><td>48V amplifier power fail</td></tr><tr><td>KBIO_OPT_10A5V_ERR</td><td>300</td><td>10A 5V amplifier error</td></tr><tr><td>KBIO_OPT_10A5V_OVRTEMP</td><td>301</td><td>10A 5V amplifier overheat</td></tr><tr><td>KBIO_OPT_10A5V_BADPOW</td><td>302</td><td>10A 5V amplifier bad power</td></tr><tr><td>KBIO_OPT_10A5V_POWFAIL</td><td>303</td><td>10A 5V amplifier power fail</td></tr><tr><td>KBIO_OPT_1A48VP_ERR</td><td>600</td><td>1A 48VP amplifier error</td></tr><tr><td>KBIO_OPT_1A48VP_OVRTEMP</td><td>601</td><td>1A 48VP amplifier overheat</td></tr><tr><td>KBIO_OPT_1A48VP_BADPOW</td><td>602</td><td>1A 48VP amplifier bad power</td></tr><tr><td>KBIO_OPT_1A48VP_POWFAIL</td><td>603</td><td>1A 48VP amplifier power fail</td></tr></table>

<table><tr><td colspan="3">VMP3 series hardware options error codes (TCurrentValuesOPTErr)</td></tr><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>ERR_NO_ERROR</td><td>0</td><td>No error found</td></tr><tr><td>ERR_OPEN</td><td>2</td><td>Open-in signal was asserted</td></tr></table>

# 6. Functions reference

# 6.1. Functions overview

Function overview of the library: 


General functions


<table><tr><td>BL_GetLibVersion</td><td>Return the version of the library</td></tr><tr><td>BL_GetVolumeSerialNumber</td><td>Return the volume serial number</td></tr><tr><td>BL_GetErrorMsg</td><td>Return the message corresponding to the selected error code</td></tr></table>


Communication functions


<table><tr><td>BL_Connect</td><td>Open a connection with the selected instrument</td></tr><tr><td>BL_Disconnect</td><td>Close the connection</td></tr><tr><td>BL_TestConnection</td><td>Test the communication with the instrument</td></tr><tr><td>BL_TestCommSpeed</td><td>Test the communication speed</td></tr><tr><td>BL_GetUSBdeviceinfos</td><td>Get information on a USB device</td></tr></table>


Firmware functions


<table><tr><td>BL_LoadFirmware</td><td>Load the firmware on selected channels</td></tr></table>


Channel information functions


<table><tr><td>BL_IsChannelPlugged</td><td>Test if selected channel is plugged</td></tr><tr><td>BL_GetChannelsPlugged</td><td>Return the channels which are plugged</td></tr><tr><td>BL_GetChannelBoardType</td><td>Return the type of the selected channel (see section 5.3. Constants)</td></tr><tr><td>BL_GetChannelInfos</td><td>Return information on selected channel</td></tr><tr><td>BL_GetMessage</td><td>Return the messages generated by the firmware of the selected channel</td></tr><tr><td>BL_GetHardConf</td><td>Return ThardwareConf object with electrode connection mode and instrument ground.</td></tr><tr><td>BL_SetHardConf</td><td>Set the electrode connection mode and instrument ground with a ThardwareConf object.</td></tr><tr><td>BL_GetOptErr</td><td>Retrieve the current error status</td></tr></table>

<table><tr><td colspan="2">Technique functions</td></tr><tr><td>BL_LoadTechnique</td><td>Load a technique and its parameters on selected channel</td></tr><tr><td>BL_LoadTechnique_LV</td><td>loads a technique and its parameters on selected channel
Used with LABVIEW</td></tr><tr><td>BL_UpdateParameters</td><td>Update a technique with new parameters on selected channel</td></tr><tr><td>BL_UpdateParameters_LV</td><td>Update a technique with new parameters on selected channel
Used with LabView</td></tr><tr><td>BL_SetDefineBoolParameter</td><td>Populate TEccParam structure with a boolean</td></tr><tr><td>BL_SetDefineSglParameter</td><td>Populate TEccParam structure with a single</td></tr><tr><td>BL_SetIntParameter</td><td>Populate TEccParam structure with an integer</td></tr></table>

<table><tr><td colspan="2">Start/stop functions</td></tr><tr><td>BL_StartChannel</td><td>Start technique(s) loaded on selected channel</td></tr><tr><td>BL_StartChannels</td><td>Start technique(s) loaded on selected channels</td></tr><tr><td>BL_StopChannel</td><td>Stops selected channel</td></tr><tr><td>BL_StopChannels</td><td>Stops selected channels</td></tr></table>

<table><tr><td colspan="2">Data functions</td></tr><tr><td>BL_GetValues</td><td>Return current values (Ewe, Ece, I, t, ...) from selected channel</td></tr><tr><td>BL_Data</td><td>Return data from selected channel</td></tr><tr><td>BLGetData_LV</td><td>Return data from selected channel Used with LABVIEW</td></tr><tr><td>BL_ConvertNumericIntoSingle</td><td>Deprecated, use BL_ConvertNumericIntoSingle instead.</td></tr><tr><td>BL_ConvertChannelNumericIntoSingle</td><td>Convert a numerical value coming from the data buffer of a channel from the specified type into a single</td></tr><tr><td>BL_ConvertTimeChannelNumericIn toSeconds</td><td>Convert the time numerical value coming from the data buffer of a channel from</td></tr></table>

the specified type into a time value in seconds. 

# Miscellaneous functions

BL_SetExperimentInfos 

Save experiment information on selected channel 

BL_GetExperimentInfos 

Read experiment information from selected channel 

BL_SendMsg 

Send a message to the selected channel 

BL_LoadFlash 

Update the communication firmware of the instrument 

# 6.2. General functions

<table><tr><td>Function</td><td>BL_GetLibVersion</td></tr><tr><td>Syntax</td><td>function BL_GetLibVersion(pVersion: PChar; 
  psize: uint32): int32;</td></tr><tr><td>Parameters</td><td>pVersion
  pointer to the buffer that will receive the text (C-string 
  format) 
  psize
  pointer to a uint32 specifying the maximum number of 
  characters of the buffer. It also returns the number of 
  characters of the copied string.</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded 
&lt; 0 : see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies the version of the library into the buffer.</td></tr><tr><td>Delphi 
example</td><td>procedure DisplayVersion; 
var
  pVersion: PChar; 
  len: int32; 
begin
  len:= 255
  pVersion:= StrAlloc(len); 
  zeromemory(pVersion, len); 
  BL_GetLibVersion(pVersion, @len); 
ShowMessage(pVersion); 
StrDestroy(pVersion); 
end;</td></tr></table>

<table><tr><td>Function</td><td>BL_GetVolumeSerialNumber</td></tr><tr><td>Syntax</td><td>procedure BL_GetVolumeSerialNumber: uint32;</td></tr><tr><td rowspan="2">Description</td><td>This function returns the volume serial number.</td></tr><tr><td>NOTE: the serial number of a (logical) drive is generated every time a drive is formatted. When Windows formats a drive, a drive&#x27;s serial number gets calculated using the current date and time and is stored in the drive&#x27;s boot sector. The odds of two disks getting the same number are virtually nil on the same machine.</td></tr><tr><td>Delphi</td><td>procedure DisplayVolumeSerialNumber;</td></tr></table>

# example

var VolumeSerialNumber: uint32; begin VolumeSerialNumber : $=$ BL_GetVolumeSerialNumber; ShowMessage(inttostr(VolumeSerialNumber)); end; 

<table><tr><td>Function</td><td>BL_GetErrorMsg</td></tr><tr><td>Syntax</td><td>function BL_GetErrorMsg(errorcode: int32; pmsg: PChar; psize: point32): int32;</td></tr><tr><td>Parameters</td><td>errorcode error code selected pmsg pointer to the buffer that will receive the text (C-string format) psize pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string.</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded &lt; 0 : see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies into the buffer the message corresponding to a given error code.</td></tr><tr><td>Delphi example</td><td>procedure DisplayErrorMessage; var msg: PChar; len: int32; begin len := 255; msg := StrAlloc(len); zeromemory(msg, len); BL_GetErrorMsg(-10, msg, @len); ShowMessage(msg); StrDestroy(msg); end;</td></tr></table>

# 6.3. Communication functions

<table><tr><td>Function</td><td>BL_Connect</td></tr><tr><td>Syntax</td><td>function BL_Connect(pstr : PChar; 
TimeOut: uint8; 
pID: pint32; 
pInfos: PDeviceInfos): int32;</td></tr><tr><td>Parameters</td><td>pstr 
pointer to the buffer specifying the instrument to connect to (C- 
string format). Ex : 192.109.209.200, USB0, USB1, ... 
(see section APPENDIX A. Find instruments to detect available 
instruments) 
TimeOut 
communication time-out in second (5 s recommended). 
This timeout will be used for the multithreading mutex that is 
used to control access to the network. When 2 functions are 
using the network, the second will be denied access until the first 
has finished. If the timeout is reached, the second function will 
return an error ERR_GEN_FUNCTIONINPROGRESS. 
pID 
pointer to a int32 that will receive the device identifier of the 
instrument 
pInfos 
pointer to a device information structure (see section 5. 
Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function establishes the connection with the selected instrument 
and copies general information (device code, RAM size, ...) into the 
TDeviceInfos structure. The returned identifier (ID) must be used in 
all other routines to communicate with the instrument.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure Connect; 
var 
IPaddress: array[0..15] of char; {IP address} 
Infos: TDeviceInfos; {device information} 
begin 
IPaddress: = '192.109.209.226' + #0; 
if BL_Connect(IPAddress, 10, @ID, @Infos) = 0 then 
ShowMessage('Connection OK!'); 
end;</td></tr><tr><td>Function</td><td>BL_Disconnect</td></tr><tr><td>Syntax</td><td>function BL_Disconnect(ID: int32): int32;</td></tr><tr><td>Parameters</td><td>ID: device identifier</td></tr><tr><td>Return value</td><td>= 0: the function succeeded
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function closes the connection with the instrument.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier}
procedure Disconnect;
begin
BL_Disconnect(ID);
end;</td></tr></table>

<table><tr><td>Function</td><td>BL_TestConnection</td></tr><tr><td>Syntax</td><td>function BL_TestConnection(ID: int32): int32;</td></tr><tr><td>Parameters</td><td>ID: device identifier</td></tr><tr><td>Return value</td><td>= 0: the function succeeded
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function tests the communication with the selected instrument.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier}
procedure TestConnection;
begin
if BL_TestConnection(ID) = 0 then
ShowMessage('Connection OK')
else
ShowMessage('Connection failed!");
end;</td></tr><tr><td>Function</td><td>BL_TestCommSpeed</td></tr><tr><td>Syntax</td><td>function BL_TestCommSpeed(ID: int32; 
channel: uint8; 
spd_rcvt: pint32; 
spd_kernel: pint32 
): int32;</td></tr><tr><td rowspan="4">Parameters</td><td>ID 
device identifier</td></tr><tr><td>channel 
selected channel (0..15)</td></tr><tr><td>spd_rcvt 
pointer to a int32 that will receive the communication speed (in ms) between the library and the device</td></tr><tr><td>spd_kernel 
pointer to a int32 that will receive the communication speed (in ms) between the library and the selected channel</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function tests the communication speed between the library and the selected channel. This function is for advanced users only.</td></tr><tr><td rowspan="2">Delphi 
example</td><td>var ID: int32; {device identifier}</td></tr><tr><td>procedure TestCommSpeed; 
varspd_rcvt,spd_kernel: int32; 
begin 
if BL_TestCommSpeed(FID, 
0, 
@spd_rcvt, 
@spd_kernel) = ERR_NOERROR then 
ShowMessage('Comm speed: ' + ntostr(spd_rcvt) + 'ms/' + ntostr(spd_kernel) + 'ms'); 
end;</td></tr><tr><td>Function</td><td>BL_GetUSBdeviceinfos</td></tr><tr><td>Syntax</td><td>function BL_GetUSBdeviceinfos(USBindex: uint32; pcompany: PChar; pcompanysize: uint32; pdevice: PChar; pdevicesize: uint32; pSN: PChar; pSNSize: uint32): boolean;</td></tr><tr><td>Parameters</td><td>USBbindex index of USB device selected (0-based) pcompany pointer to the buffer that will receive the company name (C-string format) pcompanysize pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string. pdevice pointer to the buffer that will receive the device name (C-string format) pdevicesize pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string. pSN pointer to the buffer that will receive the device serial number (C-string format) pSNSize pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string.</td></tr><tr><td>Return value</td><td>= TRUE: the function succeeded = FALSE: the function failed</td></tr><tr><td>Description</td><td>This function returns information stored in the selected USB device. This function is for advanced users only.</td></tr></table>

# 6.4. Firmware functions

<table><tr><td>Function</td><td>BL_LoadFirmware</td></tr><tr><td>Syntax</td><td>function BL_LoadFirmware(ID: int32; pChannels: uint8; pResults: pint32; Length: uint8; ShowGauge: boolean; ForceReload: boolean; BinFile: Pchar; XlxFile: PChar): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier</td></tr><tr><td></td><td>pChannels pointer to the array specifying a set of channels of the device. For each element of the array: = 0: channel not selected = 1: channel selected</td></tr><tr><td></td><td>pResults pointer to the array that will receive the result of the function for each channel : = 0: the function succeeded &lt; 0: see section 4.3 for error codes</td></tr><tr><td></td><td>Length length of the arrays pointed to by pChannels and pResults.</td></tr><tr><td></td><td>ShowGauge if TRUE a gauge is shown during the firmware loading.</td></tr><tr><td></td><td>ForceReload if TRUE the firmware is loaded unconditionally, if FALSE the firmware is loaded only if not already done.</td></tr><tr><td></td><td>BinFile Name of bin file (nil for default file).</td></tr><tr><td></td><td>XlxFile Name of xilinx file (nil for default file). VMP300-series P boards don&#x27;t need a Xlx file</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function loads the firmware on selected channels. Be aware that channels are unusable until the firmware is loaded.</td></tr></table>

# Delphi example

```txt
var ID: int32; {device identifier}   
procedure LoadFirmware;   
var Channels: array[1..16] of uint8; Results: array[1..16] of int32;   
begin {Initialize array} BL_GetChannelsPlugged(ID, @Channels, 16); zeromemory(@Results, sizeof(Result)); {Load firmware} BL_LoadFirmware(ID, @Channels, @Results, 16, TRUE, FALSE, nil, nil);   
end; 
```

# 6.5. Channel information functions

<table><tr><td>Function</td><td>BL_IsChannelPlugged</td></tr><tr><td>Syntax</td><td>function BL_IsChannelPlugged(ID: int32; ch: uint8): boolean;</td></tr><tr><td>Parameters</td><td>ID device identifier ch selected channel (0..15)</td></tr><tr><td>Return value</td><td>TRUE: selected channel is plugged FALSE: selected channel is not plugged</td></tr><tr><td>Description</td><td>This function tests if the selected channel is plugged.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier} procedure TestChannelsPlugged; var i: int32; begin for i:= 0 to 15 do begin if BL_IsChannelPlugged(ID, i) then ShowMessage(&#x27;Channel #&#x27; + inttostr(i) + &#x27; plugged&#x27;); end; end;</td></tr></table>

<table><tr><td>Function</td><td>BL_GetChannelsPlugged</td></tr><tr><td>Syntax</td><td>function BL_GetChannelsPlugged (ID: int32; 
pChPlugged: uint8; 
Size: uint8): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier</td></tr><tr><td></td><td>pChPlugged 
pointer to the array representing channels of the device. 
For each element of the array : 
0 = channel not plugged 
1 = channel plugged</td></tr><tr><td></td><td>Size 
size of the array pointed by pChPlugged</td></tr><tr><td>Return value</td><td>= 0: the function succeeded
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function returns the plugged channels.</td></tr><tr><td>Delphi example</td><td>See example of BL_LoadFirmware function</td></tr></table>

<table><tr><td>Function</td><td>BL_GetChannelInfos</td></tr><tr><td>Syntax</td><td>function BL_GetChannelInfos (ID: int32; ch: uint8; pInfos: PChannelInfos): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier ch channel selected (0..15) pInfos pointer on a channel information structure (see section 5. Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies information of the selected channel into the TChannelInfos structure.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier} procedure DisplayChannelInfos; varInfos: TChannelInfos; begin if BL_GetChannelInfos(ID, 0, @Infos) = 0 then begin ShowMessage('SerialNumber=' + inttostr(InfosBOARDSerialNumber) + ' MemSize=' + inttostr(Infos.MemSize)); end; end;</td></tr><tr><td>Function</td><td>BL_GetMessage</td></tr><tr><td>Syntax</td><td>function BL_GetMessage(ID: int32; ch: uint8; msg: PChar; size: uint32): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier channel selected channel (0..15) msg pointer to the buffer that will receive the text (C-string format) size pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies into the buffer the messages generated by the firmware of selected channel. Be aware that messages are retrieved one-by-one, this implies this function must be called several times in order to get all messages available in the message queue.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier} procedure DisplayMessages; var msg: PChar; len: int32; begin msg:= StrAlloc(255); repeat len:= 255; ZeroMemory(msg, len); if BL_GetMessage(ID, 0, msg, @len) &lt;&gt; 0 then Exit; if len &gt; 0 then ShowMessage(msg); until (len = 0); StrDestroy(msg); end;</td></tr><tr><td>Function</td><td>BL_GetHardConf</td></tr><tr><td>Syntax</td><td>function BL_GetHardConf(ID: int32; ch: uint8; pHardConf: PHardwareConf): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier ch selected channel (0..15) pHardConf pointer to a ThardwareConf object</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function return in pHardConf the hardware configuration of the channel ch. The hardware configuration is the electrode connection and the instrument ground. This function must be used only with VMP-300 series.</td></tr><tr><td>Delphi example</td><td>procedure GetHardConf(aId: int32; aCh: int32; var apHardConf: THardwareConf); var errcode: int32; begin errcode := BL_GetHardConf(aId, aCh, @apHardConf); end;</td></tr><tr><td>Function</td><td>BL_SetHardConf</td></tr><tr><td>Syntax</td><td>function BL_SetHardConf(ID: int32; 
ch: uint8; 
HardConf: THardwareConf): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
ch 
selected channel (0..15) 
HardConf 
ThardwareConf object. The attribute “Conn” is the electrode 
connection mode and the attribute “Ground” is the instrument 
ground. See 5. Structures and Constants to have the value of these 
attributes.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function set the hardware configuration of a channel with 
HardConf object. 
This function must be used only with VMP-300 series.</td></tr><tr><td>Delphi 
example</td><td>procedure SetHardConf(aId: int32; 
aCh: int32; 
var aHardConf: THardwareConf); 
var 
errcode: int32; 
begin 
errcode := BL_SetHardConf(aId, aCh, aHardConf); 
end;</td></tr><tr><td>Function</td><td>BL_GetOptErr</td></tr><tr><td>Syntax</td><td>function BL_GetOptErr(ID: int32; ch: uint8; pOptError: pint32; pOptPos: pint32): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier ch selected channel (0..15) pOptError pointer to the current error status, an integer. (see section 5.4. Error codes) pOptPos pointer to the current option address in error, an integer. It is 0 when the current error is not related to an option.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function returns the current error status. If the error is related to an option, OptPos contains the address of that option. Note: the error status is reset to 0 (No Error) after this function has been called.</td></tr><tr><td>Delphi example</td><td>procedure GetOptErr(aId: int32; aCh: int32; var OptError: pint32; var OptPos: pint32); var errcode: int32; begin errcode := BL_GetOptErr(aId, aCh, @OptError, @OptPos); end;</td></tr><tr><td>Function</td><td>BL_GetChannelBoardType</td></tr><tr><td>Syntax</td><td>function BL_GetChannelBoardType( 
ID: int32; 
Channel: uint8; 
pChannelType: point32; 
): int32;</td></tr><tr><td rowspan="3">Parameters</td><td>ID 
device identifier</td></tr><tr><td>Channel 
selected channel (0..15)</td></tr><tr><td>pChannelType 
Pointer to the returned channel type.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function returns the board Type (see 5. Structures and Constants) of the target channel into pChannelType. It can be called before the firmare is loaded and can be used to select the firmware file that should be sent.</td></tr><tr><td>Delphi 
example</td><td>See BLGetData.</td></tr></table>

# 6.6. Technique functions

<table><tr><td>Function</td><td>BL_LoadTechnique</td></tr><tr><td>Syntax</td><td>function BL_LoadTechnique (ID: int32; 
  channel: uint8; 
  pFName: PChar; 
  Params: TEffParameters; 
  FirstTechnique: boolean; 
  LastTechnique: boolean; 
  DisplayParams: boolean): int32;</td></tr><tr><td rowspan="7">Parameters</td><td>ID 
device identifier</td></tr><tr><td>channel 
selected channel (0..15)</td></tr><tr><td>pFName 
pointer to the buffer containing the name of the *.ecc file which 
defines the technique (C-string format)</td></tr><tr><td>Params 
structure of parameters of selected technique (see section 5. 
Structures and Constants). See section 7. Techniques for a 
complete description of parameters available for each technique.</td></tr><tr><td>FirstTechnique 
TRUE if the technique loaded is the first one</td></tr><tr><td>LastTechnique 
TRUE if the technique loaded is the last one</td></tr><tr><td>DisplayParams 
Display parameters sent (for debugging purpose)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function loads a technique and its parameters on the selected 
channel. 
Note: to run linked techniques, this function must be called for each 
selected technique.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
function LoadTechniques: boolean; 
var</td></tr></table>

```txt
{OCV}   
EccParamArray_OCV: array of TEccParam;   
EccParams_OCV: TEccParams;   
fname_OCV: PChar;   
{VSCAN}   
EccParamArray_VSCAN: array of TEccParam;   
EccParams_VSCAN: TEccParams;   
fname_VSCAN: Pchar;   
gin   
Result:= FALSE;   
fname_OCV:= nil;   
fname_VSCAN:= nil;   
SetLength(EccParamArray_OCV, 4);   
SetLength(EccParamArray_VSCAN, 20);   
try {Define OCV parameters}   
fname_OCV:= StrNew('ocv.ecc' + #0);   
BL_UndefineSglParameter('Rest_time_T', 0.1, 0, @EccParamArray_OCV[0]);   
BL_UndefineSglParameter('Record EVERY_dE', 0.1, 0, @EccParamArray_OCV[1]);   
BL_UndefineSglParameter('Record EVERY_dT', 0.01, 0, @EccParamArray_OCV[2]);   
BL_UndefineIntParameter('E_Range', ERANGE_AUTO, 0, @EccParamArray_OCV[3]);   
{Load OCV on selected channel}   
EccParams_OCV.len:= length(EccParamArray_OCV);   
EccParams_OCV.pParams:= @EccParamArray_OCV[0];   
if BL_LoadTechnique(ID, {device identifier} 0, {selected channel} fname_OCV, {ECC filename(c-string)} EccParams_OCV, {parameters} TRUE, {first technique} FALSE, {last technique} FALSE) <> 0 then Exit; {display params}   
{Define VSCAN parameters}   
fname_VSCAN:= StrNew('vscan.ecc' + #0);   
{Vertex #0}   
BL_UndefineSglParameter ('Voltage_step', 0.0, 0, @EccParamArray_VSCAN[0]);   
BL_UndefineBoolParameter('vs_initial', FALSE, 0, @EccParamArray_VSCAN[1]);   
BL_UndefineSglParameter ('Scan_Rate', 0.0, 0, @EccParamArray_VSCAN[2]);   
{Vertex #1}   
BL_UndefineSglParameter ('Voltage_step', 1.0, 1, @EccParamArray_VSCAN[3]);   
BL_UndefineBoolParameter('vs_initial', FALSE, 1, @EccParamArray_VSCAN[4]);   
BL_UndefineSglParameter ('Scan_Rate', 10.0, 1, @EccParamArray_VSCAN[5]);   
{Vertex #2}   
BL_UndefineSglParameter ('Voltage_step', -2.0, 2, @EccParamArray_VSCAN[6]);   
BL_UndefineBoolParameter('vs_initial', FALSE, 2, @EccParamArray_VSCAN[7]);   
BL_UndefineSglParameter ('Scan_Rate', 15.0, 2, @EccParamArray_VSCAN[8]);   
{Vertex #3}   
BL_UndefineSglParameter ('Voltage_step', 0.0, 3, 
```

@EccParamArray_VSCAN[9]);   
BL_UndefBoolParameter('vs_initl',FALSE,3, @EccParamArray_VSCAN[10]);   
BL_UndefSglParameter ('Scan_Rate'，20.0，3, @EccParamArray_VSCAN[11]);   
{Misc.}   
BL_UndefIntParameter ('Scan_number'，2，0, @EccParamArray_VSCAN[12]);   
BL_UndefIntParameter ('N_Cycles'，0，0, @EccParamArray_VSCAN[13]);   
BL_UndefSglParameter ('Record_every_dE'，0.01，0, @EccParamArray_VSCAN[14]);   
BL_UndefSglParameter ('Begin.measuring_I'，0.4，0, @EccParamArray_VSCAN[15]);   
BL_UndefSglParameter ('End.measuring_I'，0.8，0, @EccParamArray_VSCAN[16]);   
BL_UndefIntParameter ('I_Range'，IRANGE_10MA，0, @EccParamArray_VSCAN[17]);   
BL_UndefIntParameter ('E_Range'，ERANGE_AUTO，0, @EccParamArray_VSCAN[18]);   
BL_UndefIntParameter ('Bandwidth'，BANDWIDTH_5，0, @EccParamArray_VSCAN[19]);   
{Load VSCAN on selected channel}   
EccParams_VSCAN.len:= length(EccParamArray_VSCAN);   
EccParams_VSCAN.pParams:= @EccParamArray_VSCAN[0]; if BL_LoadTechnique(ID, {device identifier} 0,{selected channel} fname_VSCAN，\*.ecc filename(c-string)} EccParams_VSCAN,{parameters} FALSE,{first technique} TRUE,{last technique} FALSE) $<  > 0$ then Exit; {display params}   
Result: $\equiv$ TRUE; finally if fname_OCV<>nil then StrDestroy(fname_OCV); if fname_VSCAN<>nil then StrDestroy(fname_VSCAN); SetLength(EccParamArray_OCV，0); SetLength(EccParamArray_VSCAN，0); end; end; 

<table><tr><td>Function</td><td>BL_LoadTechnique_LV</td></tr><tr><td>Syntax</td><td>function BL_LoadTechnique_LV (ID: int32; 
  channel: uint8; 
  pFName: PChar; 
  HdIParams: PPEccParams_LV; 
  FirstTechnique: boolean; 
  LastTechnique: boolean; 
  DisplayParams: boolean): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
FName 
pointer to the buffer containing the name of the *.ecc file which 
defines the technique (C-string format) 
HdIParams 
structure of parameters of selected technique (see section 5. 
Structures and Constants). See section 7. Techniques for a 
complete description of parameters available for each technique. 
FirstTechnique 
TRUE if the technique loaded is the first one. 
LastTechnique 
TRUE if the technique loaded is the last one. 
DisplayParams 
Display parameters sent (for debugging purpose).</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function loads a technique and its parameters on the selected 
channel. It has been developed for LabVIEW compatibility. 
Note: to run linked techniques, this function must be called for each 
selected technique.</td></tr><tr><td>Delphi 
example</td><td>See example of BL_LoadTechnique function</td></tr><tr><td>Function</td><td>BL_JudgesParameter</td></tr><tr><td>Syntax</td><td>function BL_JudgesParameter(Ibl: PChar; 
value: boolean; 
index: int32; 
pParam: PEccParam): int32;</td></tr><tr><td>Parameters</td><td>Ibl 
parameter label (C-string format) 
value 
parameter value (boolean) 
index 
parameter index (useful only for multi-step parameters) 
pParam 
pointer on a elementary technique parameter structure (see section 5. Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function must be used to populate TEccParam structure with a boolean.</td></tr><tr><td>Delphi 
example</td><td>See example of BL_LoadTechnique function</td></tr></table>

<table><tr><td>Function</td><td>BL_SetSglParameter</td></tr><tr><td>Syntax</td><td>function BL_SetSglParameter(Ibl: PChar; 
value: single; 
index: int32; 
pParam: PEccParam): int32;</td></tr><tr><td>Parameters</td><td>Ibl 
parameter label (C-string format) 
value 
parameter value (single) 
index 
parameter index (useful only for multi-step parameters) 
pParam 
pointer on a elementary technique parameter structure (see section 5. Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function must be used to populate TEccParam structure with a</td></tr><tr><td colspan="2">single.</td></tr><tr><td>Delphi example</td><td>See example of BL_LoadTechnique function</td></tr></table>

<table><tr><td>Function</td><td>BL_ConfineIntParameter</td></tr><tr><td>Syntax</td><td>function BL_ConfineIntParameter(Ibl: PChar; 
value: int32; 
index: int32; 
pParam: PEccParam): int32;</td></tr><tr><td>Parameters</td><td>Ibl 
parameter label (C-string format) 
value 
parameter value (int32) 
index 
parameter index (useful only for multi-step parameters) 
pParam 
pointer on a elementary technique parameter structure (see section 5. Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function must be used to populate TEccParam structure with an integer</td></tr><tr><td>Delphi 
example</td><td>See example of BL_LoadTechnique function</td></tr></table>

<table><tr><td>Function</td><td>BL_UpdateParameters</td></tr><tr><td>Syntax</td><td>function BL_UpdateParameters(ID: int32; 
channel: uint8; 
TechIdx: int32; 
Params: TeccParams; 
EccFileName: PAnsiChar): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
TechIdx 
index of the technique if several techniques have been started. 
The first have index 0, the second have index 1, ... 
Params</td></tr><tr><td></td><td>array of parameters of selected technique (see section 5. Structures and Constants). See section 7. Techniques for a complete description of parameters available for each technique.</td></tr><tr><td></td><td>EccFileName pointer to the buffer containing the name of the *.ecc file which defines the technique (C-string format)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function updates a technique with new parameters on the selected channel. You should call this function only while an experiment is running.</td></tr><tr><td>Note</td><td>• For timing reasons, you cannot update more than 10 parameters at a time. Any call to this function with more than 10 parameters will fail.
• There are parameters that cannot be changed while running. These are set before the technique begins and are unlocked only at the end, see Error: Reference source not found.</td></tr><tr><td>Delphi example</td><td>See example of BL_LoadTechnique function</td></tr></table>

<table><tr><td>Function</td><td>BL_UpdateParameters_LV</td></tr><tr><td>Syntax</td><td>function BL_UpdateParameters_LV(ID: int32; 
channel: uint8; 
TechIdx: int32; 
HdlParams: PPEccParams_LV; 
EccFileName: PAnsiChar): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
TechIdx 
index of the technique if several techniques have been started. 
The first have index 0, the second have index 1, ... 
HdlParams 
array of parameters of selected technique (see section 5. 
Structures and Constants). See section 7. Techniques for a 
complete description of parameters available for each technique. 
EccFileName 
pointer to the buffer containing the name of the *.ecc file which</td></tr><tr><td>Return value</td><td>defines the technique (C-string format)</td></tr><tr><td rowspan="2">Description</td><td>= 0: the function succeeded</td></tr><tr><td>&lt; 0: see section 5. Structures and Constants</td></tr><tr><td rowspan="3">Note</td><td>This function updates a technique with new parameters on the selected channel. It has been developed for LabView compatibility.</td></tr><tr><td>• For timing reasons, you cannot update more than 10 parameters at a time. Any call to this function with more than 10 parameters will fail.</td></tr><tr><td>• There are parameters that cannot be changed while running. These are set before the technique begins and are unlocked only at the end, see Error: Reference source not found.</td></tr><tr><td>Delphi example</td><td>See OCV LabView examples (ocv.vi)</td></tr></table>

# .7. Start/stop functions

<table><tr><td>Function</td><td>BL_StartChannel</td></tr><tr><td>Syntax</td><td>function BL_StartChannel(ID: int32; 
channel: uint8): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function starts technique(s) loaded on selected channel.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure StartChannel; 
begin 
if BL_StartChannel(ID, 0) = 0 then 
ShowMessage('Channel started!'); 
end;</td></tr><tr><td>Function</td><td>BL_StartChannels</td></tr><tr><td>Syntax</td><td>function BL_StartChannels(ID: int32; pChannels: uint8; pResults: pint32; Length: uint8): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier</td></tr><tr><td></td><td>pChannels pointer to the array representing channels of the device. For each element of the array: = 0: channel not selected = 1: channel selected</td></tr><tr><td></td><td>pResults pointer to the array that will receive the result of the function for each channels : = 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td></td><td>Length length of the arrays pointed by pChannels and pResults.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function starts technique(s) loaded on selected channels.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier}</td></tr><tr><td></td><td>procedure StartChannels; var Channels: array[1..16] of uint8; Results: array[1..16] of int32; begin {Initialize array} BL_GetChannelsPlugged(ID, @Channels, 16); zeromemory(@Results, sizeof(Results));</td></tr><tr><td></td><td>{Start channels} if BL_StartChannels(ID, @Channels, @Results, 16) = 0 then ShowMessage('Channels started!'); end;</td></tr><tr><td>Function</td><td>BL_StopChannel</td></tr><tr><td>Syntax</td><td>function BL_StopChannel(ID: int32; 
channel: uint8): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function stops the selected channel.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure StopChannel; 
begin 
if BL_StopChannel(ID, 0) = 0 then 
ShowMessage('Channel stopped!'); 
end;</td></tr><tr><td>Function</td><td>BL_StopChannels</td></tr><tr><td>Syntax</td><td>function BL_StopChannels(ID: int32; pChannels: uint8; pResults: pint32; Length: uint8): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier</td></tr><tr><td></td><td>pChannels pointer to the array representing channels of the device. For each element of the array: = 0: channel not selected = 1: channel selected</td></tr><tr><td></td><td>pResults pointer to the array that will receive the result of the function for each channels: = 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td></td><td>Length length of the arrays pointed by pChannels and pResults.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function stops the selected channels.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier}</td></tr><tr><td></td><td>procedure StopChannels; var Channels: array[1..16] of uint8; Results: array[1..16] of int32; begin {Initialize array} BL_GetChannelsPlugged(ID, @Channels, 16); zeromemory(@Results, sizeof(Result));</td></tr><tr><td></td><td>{Stop channels} if BL_StopChannels(ID, @Channels, @Results, 16) = 0 then ShowMessage('Channels stopped!'); end;</td></tr></table>

# 6.8. Data functions

<table><tr><td>Function</td><td>BL_GetValues</td></tr><tr><td>Syntax</td><td>function BL_GetValues(ID: int32; 
channel: uint8; 
pValues: PCURRENT): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
pValues 
pointer to a current values structure (see section 5. 
Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies current values (Ewe, Ece, I, t, ...) from the 
selected channel into the structure TCurrentValues.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure DisplayValues; 
var 
curvalues: TCurrentValues; 
begin 
if BL_GetValues(ID, 0, @curvalues) = 0 then 
ShowMessage(&#x27;Ewe(V)&#x27; + FloatToStr(curvalues.Ewe) + 
&#x27; Ece(V)&#x27; + FloatToStr(curvalues.Ece) + 
&#x27; I(A)&#x27; + FloatToStr(curvalues.I)); 
end;</td></tr></table>

# Function BL_GetData

```txt
Syntax function BL_GetData (ID : int32; channel : uint8; pBuf : PDataBuffer; pInfos : PdataInfos; pValues : PCurrentValues): int32;  
Parameters ID device identifier channel selected channel (0..15) 
```

<table><tr><td></td><td>pBuf pointer to the buffer that will receive data.</td></tr><tr><td></td><td>pInfos pointer to a data information structure (see section 5. Structures and Constants)</td></tr><tr><td></td><td>pValues pointer to TCurrentValues structure (see section 5. Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies data from the selected channel into the buffer and copies information into the TDataInfos structure.</td></tr><tr><td></td><td>The field TECHNIQUEID of the structure TDataInfos contains the ID of the technique used to record data. Thanks to this technique ID one can identify the format of the data in the buffer (see section 7. Techniques for a complete description of data formats for each technique). Be aware that techniques can also be composed of several process (PEIS and GEIS for instance). In this case one can identify the process used to record data with the field PROCESSINDEX of the structure TDataInfos.</td></tr><tr><td>Delphi example</td><td>var ID: int32; {device identifier}</td></tr><tr><td></td><td>procedureGetData;</td></tr><tr><td></td><td>buf: TDataBuffer;</td></tr><tr><td></td><td>infos: TDataInfos;</td></tr><tr><td></td><td>values: TCurrentValues;</td></tr><tr><td></td><td>idx: int32;</td></tr><tr><td></td><td>row: int32;</td></tr><tr><td></td><td>thigh: int32;</td></tr><tr><td></td><td>tlow: int32;</td></tr><tr><td></td><td>t: double;</td></tr><tr><td></td><td>Ec: single;</td></tr><tr><td></td><td>Ewe: single;</td></tr><tr><td></td><td>Ece: single;</td></tr><tr><td></td><td>I: single;</td></tr><tr><td></td><td>Imoy: single;</td></tr><tr><td></td><td>freq: single;</td></tr><tr><td></td><td>cycle: int32;</td></tr><tr><td></td><td>boardType: uint32;</td></tr><tr><td></td><td>begin</td></tr><tr><td></td><td>BL_GetChannelBoardType(ID, Channel, @boardType);</td></tr><tr><td></td><td>repeat</td></tr><tr><td></td><td>{Get data}</td></tr><tr><td></td><td>if BL_GetData(ID, 0, @buf, @infos, @values) &lt;&gt; 0 then Exit;</td></tr><tr><td></td><td>ShowMessage(&#x27;Technique ID=&#x27;+ + inttostr_infos.TechnequeID) + &#x27; Technique index=&#x27;+ + inttostr相关信息.TechnequeIndex) + &#x27; Number of points=&#x27;+ + inttostr相关信息.NbRows));</td></tr><tr><td></td><td>{Display data}</td></tr><tr><td></td><td>for row := 0 to相关信息.NbRows - 1 do</td></tr><tr><td></td><td>begin</td></tr><tr><td></td><td>idx := row*infos.NbCols;</td></tr></table>

```matlab
{OCV technique} if (infos.TechniqueID = TECHNIQUEID_OCV) then begin BL_ConvertTimeChannelNumericIntoSeconds(@buf[idx+1], &t, values.TimeBase, boardType); t := t +infos.StartTime*0.001; BL_ConvertChannelNumericIntoSingle(buf[ idx+3], @Ewe, boardType); BL_ConvertChannelNumericIntoSingle(buf[ idx+4], @Ece, boardType); ShowMessage('t=' + format('%.3e', [t]) + 's' + 'Ewe=' + format('%.3e', [Ewe]) + 'V' + 'Ece=' + format('%.3e', [Ece]) + 'V'); end {CV technique} else if (infos.TechniqueID = TECHNIQUEID_CV) then begin BL_ConvertTimeChannelNumericIntoSeconds(@buf[ idx+1], &t, values.TimeBase, boardType); t := t +infos.StartTime*0.001; BL_ConvertChannelNumericIntoSingle(buf[ idx+3], @Ec, boardType); BL_ConvertChannelNumericIntoSingle(buf[ idx+4], @Imoy, boardType); BL_ConvertChannelNumericIntoSingle(buf[ idx+5], @Ewe, boardType); cycle := buf[ idx+6]; ShowMessage('t=' + format('%.3e', [t]) + 's' + 'Ec=' + format('%.3e', [Ec]) + 'V' + 'Imoy=' + format('%.3e', [Imoy]) + 'A' + 'Ewe=' + format('%.3e', [Ewe]) + 'V' + 'cycle=' + inttostr(cycle)); end {PEIS technique} else if (infos.TechniqueID = TECHNIQUEID_PEIS) then begin ifinfos.ProcessIndex = 0 then {PEIS, 1th process} begin BL_ConvertTimeChannelNumericIntoSeconds(@buf[ idx+1], &t, values.TimeBase, boardType); t := t +infos.StartTime*0.001; BL_ConvertChannelNumericIntoSingle(buf[ idx+3], @Ewe, boardType); BL_ConvertChannelNumericIntoSingle(buf[ idx+4], @I, boardType); ShowMessage('t=' + format('%.3e', [t]) + 's' + 'Ewe=' + format('%.3e', [Ewe]) + 'V' + 'I' + format('%.3e', [I]) + 'A'); end else if infosProcessIndex = 1 then {PEIS, 2th process} begin BL_ConvertChannelNumericIntoSingle(buf[ idx+1], @freq, boardType); {...} BL_ConvertChannelNumericIntoSingle(buf[ idx+5], @Ewe, boardType); BL_ConvertChannelNumericIntoSingle(buf[ idx+6], @I, boardType); {...} BL_ConvertChannelNumericIntoSingle(buf[ idx+14], @t, boardType); {...} ShowMessage('freq=' + format('%.3e', [freq]) + 'Hz' + 't=' + format('%.3e', [t]) + 's' + 'Ewe=' + format('%.3e', [Ewe]) + 'V' + 'I' + format('%.3e', [I]) + 'A'); end; 
```

end;   
end; {for-loop} until (values.MemFilled $= 0$ ) and (values.State $=$ STATE_STOP);   
end; 

<table><tr><td>Function</td><td>BL_GetData_LV</td></tr><tr><td>Syntax</td><td>function BL_GetData_LV (ID : int32; 
  channel : uint8; 
  pBuf : PDataBuffer; 
  pInfos : PdataInfos; 
  pValues : PCurrentValues): int32;</td></tr><tr><td>Parameters</td><td>ID 
  device identifier 
  channel 
  selected channel (0..15) 
  pBuf 
  pointer to the buffer that will receive data. 
  pInfos 
  pointer to a data information structure (see section 5. 
  Structures and Constants) 
  pValues 
  pointer to TCurrentValues structure (see section 5. 
  Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies data from the selected channel into the buffer 
and copies information into the TDataInfos structure. It has been 
developed for LabView compatibility. 
It is based on the function BL_GetData, with an adjustment of the 
memory to avoid the problem of alignment. 
The field TECHNIQUEID of the structure TDataInfos contains the ID of 
the technique used to record data. Thanks to this technique ID one 
can identify the format of the data in the buffer (see section 7. 
Techniques for a complete description of data format for each 
technique). 
Be aware that techniques can also be composed of several process 
(PEIS and GEIS for instance). In this case one can identify the 
process used to record data with the field PROCESSINDEX of the 
structure TDataInfos.</td></tr><tr><td>Function</td><td>BL_ConvertNumericIntoSingle</td></tr><tr><td>Syntax</td><td>function BL_ConvertNumericIntoSingle(num: uint32; psgl: psingle): int32;</td></tr><tr><td>Parameters</td><td>num
numerical value
psgl
pointer to the single that will receive the result of conversion</td></tr><tr><td>Return value</td><td>= 0: the function succeeded
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>Deprecated. Use BL_ConvertChannelNumericIntoSingle instead. The function still remains in ECLib.dll and applications using it will still work as long as no Premium P boards are included in the instrument.</td></tr><tr><td>Delphi
example</td><td></td></tr></table>

<table><tr><td>Function</td><td>BL_ConvertChannelNumericIntoSingle</td></tr><tr><td>Syntax</td><td>function BL_ConvertChannelNumericIntoSingle( num: uint32; pRetFloat: psingle; ChannelType: uint32 ): int32;</td></tr><tr><td>Parameters</td><td>num
numerical value
pRetFloat
pointer to the single that will receive the result of conversion
ChannelType
Channel type constant. Return from BL_GetChannelBoardType.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded</td></tr><tr><td rowspan="2">Description</td><td>&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>This function converts a numerical value coming from the data buffer of BL_GetData function received from the specified channel type into single float format.</td></tr><tr><td>Delphi example</td><td>See BLGetData.</td></tr></table>

<table><tr><td>Function</td><td>BL_ConvertTimeChannelNumericIntoSeconds</td></tr><tr><td>Syntax</td><td>function BL_ConvertTimeChannelNumericIntoSeconds( 
pnum: uint32; 
pRetTime: pdouble; 
Timebase: single; 
ChannelType: uint32 ): int32;</td></tr><tr><td>Parameters</td><td>pnum 
Pointer to the numerical value in the data buffer 
pRetTime 
pointer to the double that will receive the result of 
conversion 
Timebase 
Timebase value for the technique that recoreDED the data. 
Can be read from the current 
ChannelType 
Channel type constant. Return from 
BL_GetChannelBoardType.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function converts a numerical value coming from the data buffer 
of BLGetData function received from the specified channel type into 
a time value in second from the technique start in a double float 
format.</td></tr><tr><td>Delphi 
example</td><td>See BLGetData.</td></tr></table>

# 6.9. Miscellaneous functions

<table><tr><td>Function</td><td>BL_SetExperimentInfos</td></tr><tr><td>Syntax</td><td>function BL_SetExperimentInfos(ID: int32; 
channel: uint8; 
ExpInfos: TExperimentInfos): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
ExpInfos 
Experiment information (see section 5. Structures and 
Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function saves experiment information on selected channel.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure SaveExperimentInfos; 
var 
ExpInfos: TExperimentInfos; 
begin 
zeromemory(@ExpInfos, sizeof(TExperimentInfos)); 
ExpInfos.Group := 0; 
ExpInfos.PCidentifier := 1; 
ExpInfos.TimeHMS := 2; 
ExpInfos.TimeYMD := 3; 
StrCopy(@ExpInfos.FileName, 'test'); 
if BL_SetExperimentInfos(ID, 0, ExpInfos) = 0 then 
ShowMessage('Experiment information saved!'); 
end;</td></tr><tr><td>Function</td><td>BL_GetExperimentInfos</td></tr><tr><td>Syntax</td><td>function BL_GetExperimentInfos(ID: int32; 
channel: uint8; 
pExpInfos: PexperimentInfos): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
channel 
selected channel (0..15) 
pExpInfos 
pointer to an experiment information structure (see section 5. 
Structures and Constants)</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function copies experiment information from selected channel 
into the TEXPERIMENTINFOS structure.</td></tr><tr><td>Delphi 
example</td><td>var ID: int32; {device identifier} 
procedure ReadExperimentInfos; 
var 
ExpInfos: TExperimentInfos; 
begin 
zeromemory(@ExpInfos, sizeof(TExperimentInfos)); 
if BL_GetExperimentInfos(ID, 1, @ExpInfos) = 0 then 
ShowMessage('Experiment information read!'); 
end;</td></tr><tr><td>Function</td><td>BL_SendMsg</td></tr><tr><td>Syntax</td><td>function BL_SendMsg(ID: int32; ch: uint8; pbuf: pointer; plen: uint32): int32;</td></tr><tr><td>Parameters</td><td>ID device identifier ch selected channel (0..15) pbuf pointer to the data buffer plen pointer to a uint32 specifying the length of data to transfer. It also returns the length of data copied into the buffer.</td></tr><tr><td>Return value</td><td>= 0: the function succeeded &lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function sends a message to the selected channel. Function for advanced users only.</td></tr></table>

<table><tr><td>Function</td><td>BL_LoadFlash</td></tr><tr><td>Syntax</td><td>function BL_LoadFlash(ID: int32; 
pfname: PChar; 
ShowGauge: boolean): int32;</td></tr><tr><td>Parameters</td><td>ID 
device identifier 
pfname 
file name (.flash extension) of the new communication 
firmware 
(C-string format) 
ShowGauge 
show a gauge during transfer</td></tr><tr><td>Return value</td><td>= 0: the function succeeded 
&lt; 0: see section 5. Structures and Constants</td></tr><tr><td>Description</td><td>This function updates the communication firmware of the instrument. 
Function for advanced users only.</td></tr></table>

# 7. Techniques

# 7.1. Notes

See the description of the function BL_LoadTechnique for a complete example of how to load a technique and its parameters on a channel. 

See the description of the function BL_GetData for a complete example of data recovery and data conversion. 

PEIS, GEIS, SPEIS, SGEIS, PZIR and GZIR techniques can only be used with boards with impedance capabilities. 

Electrochemical techniques parameters must be carefully programmed according to the instrument hardware specifications. Be aware that wrong parameters can generate faulty operations of the technique. 

Please note that the name of the ECC file is name.ecc except to VMP-300 series. For VMP-300 series the name is name4.ecc. For VMP-300 series P boards, the name is name5.ecc 

The counter-electrode value of the EIS techniques must not be taken into account with the VMP-300 series. 

Some technique parameters cannot be modified when an experiment is running. They are set at the start of the technique and are unlocked only when it is finished. The immutable parameters are presented in section 7.1.3. 

The parameter names are case-sensitive. The BL_LoadTechnique and BL_UpdateParameters will issue an error when a parameter is not recognized. 

# 7.1.1 Recording additional values and XCTR changes from v5.34

From version 5.34 and onwards, the xctr parameter was enhanced to allow recording of some additional variables during an experiment. 

As a consequence, some experiment variables are not recorded by default anymore in several techniques, and must be specified by the xctr parameter. 

On the other hand, it allowed several technique timebase to be lowered since the record step does not take as much time as before. For instance, the CV technique got its timebase lowered from 50 to 45 µs. 

If you want to record more than the default fields, you will have to specify the additional values to record by the XCTR parameter. Any value added to the record step will appear in the data array that is returned by the BL_GetData function in the order of the XCTR bit-field flag being set. 

You will also need to manually add a small delay (depending on the value recorded, see 7.1.2) to the timebase, otherwise the technique might miss some measurements. These steps are explained in the XCTR definition and in the technique documentation. 

# 7.1.2 Timebase calculation

The timebase that is presented in the next pages is the minimum amount of time that you should allow between two records for a technique. It provides sufficient time for the technique to record the basic set of data it has to measure. 

When using XCTR to record additional experiment values, you will have to specify a timebase that is large enough for the machine to handle the acquisition of these extra values. The two following points will tell you how to calculate the optimal delay to add to the technique: 

Adding one extra measurement has a base cost of 5µs. So anytime you start adding records to your technique, you should add 5µs to the original timebase. 

Each additional measurement after the first one has a cost of 0.5µs, but you should round the final delay at the next integer value 


Examples of calculation


<table><tr><td>Additional value</td><td>Delay to add to timebase</td></tr><tr><td>Ece</td><td>5μs</td></tr><tr><td>Ece, Analog IN1</td><td>5 + 0.5 = 5.5μs rounded to 6μs</td></tr><tr><td>Ece, Analog IN1, IRange</td><td>5+0.5+0.5 = 6μs</td></tr></table>

# 7.1.3 Hardware parameters

Some parameters configure the base acquisition and cannot be modified throughout the experiment. They are set at the beginning of the technique and unlocked at the end. You can set them regardless of the technique. 

<table><tr><td colspan="4">Hardware parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants allowed on section 5. Structures and Constants</td></tr><tr><td>E_Range</td><td>Ewe range</td><td>integer</td><td>see ERange constants allowed on section 5. Structures and Constants</td></tr><tr><td>Bandwidth</td><td>Bandwidth</td><td>integer</td><td>see bandwidth constants allowed on section 5. Structures and Constants</td></tr><tr><td>tb</td><td>Time base (s)</td><td>single</td><td>&gt;0, often μs range</td></tr></table>

# 7.2. Open Circuit Voltage technique


Technique ID: 100


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>ocv.ecc</td><td>ocv4.ecc 
ovc5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/d7034be7584fe3aa89458c5bbd82dc2a5d4794fa0652717bc74458f28450cae8.jpg)


# 7.2.1. Description

The Open Circuit Voltage (OCV) technique consists of a period during which no potential or current is applied to the working electrode. The cell is disconnected from the power amplifier. Only, the potential measurement is available. So the evolution of the rest potential can be recorded. 

# 7.2.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">OCV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record每一天_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record每一天_dT</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr></table>

# 7.2.3. Data format

Data format returned by the function BL_GetData: 

VMP3 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/6312f0e8ddc8343cfccea92af52eeb0eb6bf1ed4fbe368e107a409c816cb2f1b.jpg)


VMP-300 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/be56587890080c44cac5ff7f824f21a2cc23c5b91ef0be4cf481aed389ddc38f.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.2.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe and Ece must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.3. Cyclic Voltammetry technique


Technique ID: 103


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>cv.ecc</td><td>cv4.ecc 
cv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>45μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f295f5eb5bb81eb84ef54b666afc2a01d3f732813f47e84e382c033edab8cd87.jpg)


# 7.3.1. Description

Cyclic voltammetry (CV) is the most widely used technique for acquiring qualitative information about electrochemical reactions. CV provides information on redox processes, heterogeneous electron-transfer reactions and adsorption processes. It offers a rapid location of redox potential of the electroactive species. 

CV consists of scanning linearly the potential of a stationary working electrode using a triangular potential waveform. During the potential sweep, the potentiostat measures the current resulting from electrochemical reactions (consecutive to the applied potential). The cyclic voltammogram is a current response as a function of the applied potential. 

# 7.3.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>vs_initializer</td><td>Current step vs initial one</td><td>Array of 5 boolean</td><td>True/False</td></tr><tr><td>Voltage_step</td><td>Voltage step (V)</td><td>Array of 5 single</td><td>[Ei, E1, E2, Ei, Ef] see CV picture.</td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 5 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>= 2</td></tr><tr><td>Record EVERY_dE</td><td>recording on dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>N_Cycles</td><td>Number of cycle</td><td>integer</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Begin step accumulation. "1" means 100% of step</td><td>single</td><td>[0..1]</td></tr><tr><td>End(measuring_I</td><td>End step accumulation. "1" means 100% of step</td><td>single</td><td>[0..1]</td></tr></table>

# 7.3.3. Data format

Data format returned by the function BL_GetData on VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/6a0fe9515877c5e9f2b0c73138837d36a900179ffc5ae7a208e322a5c6b9ef58.jpg)


VMP-300 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/636c2c3f624df8d5f90cc81be953a840cd4aa7f31a3fd29975f89c3e578ca5db.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.3.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe and Ece must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.4. Cyclic Voltammetry Advanced technique


Technique ID: 126


<table><tr><td>Instrument
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>biovscan.ecc</td><td>Not supported</td></tr><tr><td>Timebase</td><td>40μs</td><td></td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/45b4097c4b198b1287f21d0c625f9b0c22e4bdb5e1dd36409d68257e4b141eef.jpg)


# 7.4.1. Description

Cyclic voltammetry (CV) is the most widely used technique for acquiring qualitative information about electrochemical reactions. CV provides information on redox processes, heterogeneous electron-transfer reactions and adsorption processes. It offers a rapid location of redox potential of the electroactive species. 

CV consists of scanning linearly the potential of a stationary working electrode using a triangular potential waveform. During the potential sweep, the potentiostat measures the current resulting from electrochemical reactions (consecutive to the applied potential). The cyclic voltammogram is a current response as a function of the applied potential. 

# 7.4.1. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 


CVA parameters


<table><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>vs_initializer_scan</td><td>Current scan vs initial one</td><td>Array of 4 boolean</td><td>True/False</td></tr><tr><td>Voltage_scan</td><td>Voltage scan (V)</td><td>Array of 4 single</td><td>[Ei, E1, E2, Ef] see CVA picture.</td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 4 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>=2</td></tr><tr><td>Record EVERY_dE</td><td>recording on dE</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>N_Cycles</td><td>Number of cycle</td><td>integer</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Begin step accumulation.</td><td>single</td><td>[0..1]</td></tr></table>


CVA parameters


<table><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td rowspan="2">End/measuring_I</td><td>&quot;1&quot; means 100% of step</td><td></td><td></td></tr><tr><td>End step accumulation. &quot;1&quot; means 100% of step</td><td>single</td><td>[0..1]</td></tr><tr><td>vs_initializer_step</td><td>Current step vs initial one</td><td>Array of 2 boolean</td><td>True/False</td></tr><tr><td>Voltage_step</td><td>Voltage step (V)</td><td>Array of 2 single</td><td>≥ 0</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 2 single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>= 0</td></tr><tr><td>Record_every_dT</td><td>Recording on dT</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_every_dI</td><td>Recording on dI</td><td>single</td><td>≥ 0</td></tr><tr><td>Trig_on_off</td><td>trigger</td><td>boolean</td><td>True/False</td></tr></table>

# 7.4.2. Data format

Data format returned by the function BL_GetData: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/6bfef9cd575883a342055b551de3667589eb1b92b98f586ebc89b7ace73ca5eb.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.4.3. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe and Ece must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.5. Chrono-Potentiometry technique


Technique ID: 102


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>cp.ecc</td><td>cp4.ecc
cp5.ecc</td></tr><tr><td>Timebase</td><td>21μs</td><td>21μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/87f15311898ac56763a821efa2df3a5cdb5a14a91c8fb5b817c5f682686fce65.jpg)


# 7.5.1. Description

The Chronopotentiometry (CP) is a controlled current technique. The current is controlled and the potential is the variable determined as a function of time. The chronopotentiometry technique is similar to the Chronoamperometry technique, potential steps being replaced by current steps. The current is applied between the working and the counter electrode. 

This technique can be used for different kind of analysis or to investigate electrode kinetics. It is considered less sensitive than voltammetric techniques for analytical uses. Generally, the curves Ewe $=$ f(t) contains plateaus that correspond to the redox potential of electroactive species. 

# 7.5.1. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CP parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Current_step</td><td>Current step (A)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Current step vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 100 single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants allowed on section 5. Structures and ConstantsWarning: I Auto-range is not allowed</td></tr></table>

# 7.5.2. Data format

Data format returned by the function BL_GetData : 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f5cd1426a7feaa9b3adbd70bc1a7fdde3b0df03040c6f44fa4a84cc856621bb5.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.5.3. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

<table><tr><td>time: 
The time should be converted using the function 
BL_ConvertTimeChannelNumericIntoSeconds</td></tr></table>

<table><tr><td>•</td><td>Float conversion: 
Ewe and I must be converted with the function 
BL_ConvertChannelNumericIntoSingle</td></tr></table>

<table><tr><td>cycle: 
no conversion needed</td></tr></table>

# 7.6. Chrono-Amperometry technique


Technique ID: 101


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td rowspan="2">File</td><td rowspan="2">ca.ecc</td><td>ca4.ecc</td></tr><tr><td>ca5.ecc</td></tr><tr><td>Timebase</td><td>24μs</td><td>21μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/bc669f6610ff82490e910e3cf5774eb1dfed6423399c3da61d88ad5903749108.jpg)


# 7.6.1. Description

The basis of the controlled-potential techniques is the measurement of the current response to an applied potential step. 

The Chronoamperometry (CA) technique involves stepping the potential of the working electrode from an initial potential, at which (generally) no faradic reaction occurs, to a potential Ei at which the faradic reaction occurs. The current-time response reflects the change in the concentration gradient in the vicinity of the surface. 

Chronoamperometry is often used for measuring the diffusion coefficient of electroactive species or the surface area of the working electrode. This technique can also be applied to the study of electrode processes mechanisms. 

An alternative and very useful mode for recording the electrochemical response is to integrate the current, so that one obtains the charge passed as a function of time. This is the chronocoulometric mode that is particularly used for measuring the quantity of adsorbed reactants. 

# 7.6.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 


CA parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Voltage_step</td><td>Voltage step (V)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Voltage step vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 100 single</td><td>≥ 0</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>single</td><td>≥ 0</td></tr><tr><td colspan="4">CA parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

# 7.6.3. Data format

See CP technique data format. 

# 7.6.4. Data conversion

See CP technique data conversion. 

# 7.7. Voltage Scan technique


Technique ID: 124


<table><tr><td>Instrument Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>vscan.ecc</td><td>vscan4.ecc vscan5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>50μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/d81aa348780f977a8f91b7b5adb047dc824198e4e24c8ae3b35b07f3a30f3638.jpg)


# 7.7.1. Description

The Potentiodynamic (PDYN) technique allows the user to perform potentiodynamic periods with different scan rates. 

# 7.7.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">VSCAN parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Voltage_step</td><td>Vertex potential (V)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Vertex potential vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Scan_Rate</td><td>Scan rate (V/s) from previous vertex potential</td><td>Array of 100 single</td><td>&gt;0, Value of the first scan-rate is ignored</td></tr><tr><td>Scan_number</td><td>Number of scans minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥0</td></tr><tr><td>Begin_MEasuring_I</td><td>Begin step accumulation. &quot;1&quot; means 100% of step</td><td>single</td><td>[0..1]</td></tr><tr><td>End_MEasuring_I</td><td>End step accumulation. &quot;1&quot; means 100% of step</td><td>single</td><td>[0..1]</td></tr></table>

# 7.7.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/1a6fd12567dcc9756850412651668584dbddd8f38a4d82eb497ffd83ec08370a.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/28dc7afdb7edcccb9cbffd488ad53acb38dba68a58c4d06597c7e418b043966c.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.7.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before : 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ec, <I> and Ewe must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.8. Current Scan technique


Technique ID: 125


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>iscan.ecc</td><td>iscan 4.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>50μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/505a11b2eb817fa9fa7dd0ac09fc32e8eadc71ef9fae7ebca85997baa5f21af5.jpg)


# 7.8.1. Description

The Galvanodynamic (GDYN) technique allows the user to perform galvanodynamic periods with different scan rates. 

# 7.8.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">ISCAN parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Current_step</td><td>Vertex current (A)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Vertex current vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Scan_Rate</td><td>Scan rate (A/s) from previous vertex current</td><td>Array of 100 single</td><td>&gt;0</td></tr><tr><td>Scan_number</td><td>Number of scans minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>single</td><td>≥0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥0</td></tr><tr><td>Begin_MEasuring_E</td><td rowspan="2">Select the part of the current step (1 = 100%) used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_E</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and ConstantsWarning : I Auto-range is not allowed</td></tr></table>

# 7.8.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/305df54c543d6d894b876f0ce8d3430b2534058b96ba4ecc157d45ee36f6a248.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/11d834f5e9e61f93ced2d54f6bbced11282e1f8400dee6510134ca20c0d26c7b.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.8.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ic, <I> and Ewe must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.9. Constant Power technique


Technique ID: 110


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>pow.ecc</td><td>/</td></tr><tr><td>Timebase</td><td>100μs</td><td></td></tr></table>

# 7.9.1. Description

The Constant Power technique is designed to study the discharge (or the charge) of a cell at constant power. 

# 7.9.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">Constant Power parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Power_step</td><td>Power step (W)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Power step vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 100 single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr><tr><td>I_RANGE</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning : I Auto-range not authorized</td></tr></table>

# 7.9.3. Data format

Data format returned by the function BL_GetData: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/3a618d054826eaf191610e6b0457585641a34c5fba4aafa6811c86e8c309b339.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.9.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe, I and P must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.10. Constant Load technique


Technique ID: 111


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>load.ecc</td><td>/</td></tr><tr><td>Timebase</td><td>100μs</td><td></td></tr></table>

# 7.10.1. Description

The Constant Load technique is designed to discharge a battery at a constant resistance. 

# 7.10.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">Constant Load parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Load_step</td><td>Resistor step value (Ohm)</td><td>Array of 100 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Resistor step value vs initial one</td><td>Array of 100 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 100 single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr><tr><td>I_RANGE</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning : I Auto-range is not allowed</td></tr></table>

# 7.10.3. Data format

Data format returned by the function BL_GetData : 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/87b12ca337096bd44df7576ea42aad0305fe82acf281651b0b7bdc8c04d641c1.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.10.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time : The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe, I and R must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.11. Potentio Electrochemical Impedance Spectroscopy technique


Technique ID: 104


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>peis.ecc</td><td>peis4.ecc 
peis5.ecc</td></tr><tr><td>Timebase</td><td>24μs*</td><td>24μs*</td></tr></table>


* Timebase is for first process only. 


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/2cdabd1f13b8bf1e61bec90eeff2e75b84159b914dbaf108975a7b575d0ebce1.jpg)


# 7.11.1. Description

The Potentio Electrochemical Impedance Spectroscopy (PEIS) technique performs impedance measurements into potentiostatic mode in applying a sine wave around a DC potential E that can be set to a fixed value or relatively to the cell equilibrium potential. 

For very capacitive or low impedance electrochemical systems, the potential amplitude can lead to a current overflow that can stop the experiment in order to protect the unit from overheating. Using GEIS instead of PEIS can avoid this inconvenient situation. 

Moreover, during corrosion experiment, a potential shift of the electrochemical system can occur. PEIS technique can lead to impedance measurements far from the corrosion potential while GEIS can be performed at a zero current. 

# 7.11.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">PEIS parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>vs_initializer</td><td>Voltage step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Initial_Voltage_step</td><td>Initial voltage step (V)</td><td>single</td><td>-</td></tr><tr><td>Duration_step</td><td>Step duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>single</td><td>≥ 0</td></tr><tr><td>Final_freqency</td><td>Final frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr></table>


PEIS parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Initial_freqency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic (TRUE for linear points spacing)</td><td>boolean</td><td>True/False</td></tr><tr><td>Amplitude_Voltage</td><td>Sine amplitude (V)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Frequency_number</td><td>Number of frequencies</td><td>integer</td><td>≥ 1</td></tr><tr><td>Average_N(times</td><td>Number of repeat times (used for frequencies averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Correction</td><td>Non-stationary correction</td><td>boolean</td><td>True/False</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait before each frequency</td><td>single</td><td>≥ 0</td></tr></table>

# 7.11.3. Data format

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/4f2ab65f04b47ae7245f53bc4ab54823d6971e1c9a127d10cc03678dd662ea9b.jpg)


Data format of process 1: 

VMP3 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/d7bb5083b7b6a0c3076413b5b08f17c4c6b3d60f7785a453cede777131eb64f3.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f9c35716f4a611896101a21fef51cef7030bd669b592e0d4ff2ef90e32464a89.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/c087396a7359870b3e5b063b316225a0d26cd4f0ea33f33f82a439c980b60c89.jpg)


VMP-300 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f7106cb206cc36bd1dbeef75252f3221716d2b4b0fc99d6e10d01c29f63dd8e6.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a09ad5d5bd9ff811c7a7d18c0c9aacd236df18d6a4847728042a5fb999817bfd.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/b1ada243bc0ba758687b172b6dd32016699b3a270d1f7fc3097a622c0ce3b151.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.11.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: no conversion needed 

# 7.12. Staircase Potentio Electrochemical Impedance Spectroscopy technique


Technique ID: 113


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>seisp.ecc</td><td>seisp4.ecc 
seisp5.ecc</td></tr><tr><td>Timebase</td><td>24μs</td><td>24μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e5d324d2bd6a715670da6af3147596c5987f5daa90b5adaa080e89a13392c8c9.jpg)


# 7.12.1. Description

The Staircase Potentio Electrochemical Impedance Spectroscopy (SPEIS) technique is designed to perform successive impedance measurements (on a whole frequency range) during a potential sweep. The main application of this technique is to study electrochemical reaction kinetics along voltamperometric (I(E)) curves in analytical electrochemistry. Thus this technique finds all its interest to study the complexity of non-stationary interfaces with faradic processes where the total AC response (whole frequency range) is required. 

Another common application of such a technique is semi-conductor materials study. For these stationary systems only two or three frequencies for each potential step are required to determine the donor density and the flat band potential. 

The SPEIS technique consists in a staircase potential sweep (potential limits and number of steps defined by the user). An impedance measurement (with an adjustable number of frequencies) is performed on each potential step. For all these applications a Mott-Schottky plot ( $\ 1 / { \mathsf { C } } ^ { 2 }$ vs. Ewe) can be displayed and a special linear fit is applied to extract the semi-conductor parameters. 

# 7.12.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">SPEIS parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>vs_initializer</td><td>Voltage step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>vs_final</td><td>Voltage step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Initial_Voltage_step</td><td>Initial voltage step (V)</td><td>single</td><td>-</td></tr><tr><td>Final_Voltage_step</td><td>Final voltage step (V)</td><td>single</td><td>-</td></tr></table>


SPEIS parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Duration_step</td><td>Step duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of voltage steps</td><td>integer</td><td>[0..98]</td></tr><tr><td>Record_Every_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_Every_dI</td><td>Record every dI (A)</td><td>single</td><td>≥ 0</td></tr><tr><td>Final_freqency</td><td>Final frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Initial_freqency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic (TRUE for linear points spacing)</td><td>boolean</td><td>True/False</td></tr><tr><td>Amplitude_Voltage</td><td>Sine amplitude (V)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Frequency_number</td><td>Number of frequencies</td><td>integer</td><td>≥ 1</td></tr><tr><td>Average_N(times</td><td>Number of repeat times (used for frequencies averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Correction</td><td>Non-stationary correction</td><td>boolean</td><td>True/False</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait before each frequency</td><td>single</td><td>≥ 0</td></tr></table>

# 7.12.3. Data format

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/084f27240d219c786f0cab370dfd0978f015f2af0312ca711966a9bf0395b57d.jpg)


Data format of process 1: 

VMP3 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/ecc2dcd74eed4aa34db0905afc47d01c40399c00f2f5f8e8fa31774818d46f23.jpg)


VMP-300 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/3558bca895a4afacde273edd97c734a0afda33c4ec1ba940546f02a12259409f.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/11b5ff89ef715c64e97ef232b97c65a18e1e1cdd4bc9fa43d0c23498e974dcb8.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.12.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce must be converted with the function BL_ConvertChannelNumericIntoSingle 

cycle: 

no conversion needed 

# 7.13. Galvano Electrochemical Impedance Spectroscopy technique


Technique ID: 107


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>geis.ecc</td><td>geis4.ecc 
geis5.ecc</td></tr><tr><td>Timebase</td><td>24μs*</td><td>24μs*</td></tr></table>


* Timebase is for first process only. 


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e990ee0595dcd5c3d4c1db07965c5565dc287d2899e4deb58494485551f3e507.jpg)


# 7.13.1. Description

The Galvano Electrochemical Impedance Spectroscopy (GEIS) technique performs impedance measurements into galvanostatic mode in applying a sine wave around a DC current I that can be set to a fixed value or relatively to a previous controlled current. In the case of particular non-linear systems it can be necessary to use PEIS instead of GEIS. 

The Electrochemical Impedance spectroscopy (EIS) finds many of applications in corrosion, battery, fuel cell development, sensors and physical electrochemistry. It can provide information on reaction parameters, corrosion rates, electrode surfaces porosity, coating, mass transport, interfacial capacitance measurements. 

# 7.13.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">GEIS parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>vs_initializer</td><td>Current step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Initial_Current_step</td><td>Initial current step (A)</td><td>single</td><td>-</td></tr><tr><td>Duration_step</td><td>Step duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Final_freqency</td><td>Final frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Initial_freqency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr></table>


GEIS parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic (TRUE for linear points spacing)</td><td>boolean</td><td>True/False</td></tr><tr><td>Amplitude_Current</td><td>Sine amplitude (A)</td><td>single</td><td>[0..50% of the Irange]</td></tr><tr><td>Frequency_number</td><td>Number of frequencies</td><td>integer</td><td>≥ 1</td></tr><tr><td>Average_N(times</td><td>Number of repeat times (used for frequencies averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Correction</td><td>Non-stationary correction</td><td>boolean</td><td>True/False</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait before each frequency</td><td>single</td><td>≥ 0</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and ConstantsWarning : I Auto-range is not allowed</td></tr></table>

# 7.13.3. Data format

See PEIS technique data format. 

# 7.13.4. Data conversion

See PEIS technique data conversion. 

# 7.14. Staircase Galvano Electrochemical Impedance Spectroscopy technique


Technique ID: 114


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>seisg.ecc</td><td>seisg4.ecc 
seisg5.ecc</td></tr><tr><td>Timebase</td><td>24μs*</td><td>24μs*</td></tr></table>


* Timebase for first process only 


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a0a8c3bc1e6157737a5b0158526d017c4d965d26de874d1bcf4cf2cc37f8c399.jpg)


# 7.14.1. Description

The Staircase Galvano Electrochemical Impedance Spectroscopy (SGEIS) technique is close to the SPEIS technique. The difference is that the potentiostat works as a galvanostat and applies a current sweep (staircase shape). In the same way an impedance measurement (whole frequency range) can be performed on each current step. The user can also select several frequencies. 

# 7.14.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">SGEIS parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>vs.initial</td><td>Current step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>vs_final</td><td>Current step vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Initial_Current_step</td><td>Initial current step (A)</td><td>single</td><td>-</td></tr><tr><td>Final_Current_step</td><td>Final current step (A)</td><td>single</td><td>-</td></tr><tr><td>Duration_step</td><td>Step duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Step_number</td><td>Number of voltage steps</td><td>integer</td><td>≥ 0</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Final_freqency</td><td>Final frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Initial_freqency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr></table>


SGEIS parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic (TRUE for linear points spacing)</td><td>boolean</td><td>True/False</td></tr><tr><td>Amplitude_Current</td><td>Sine amplitude (A)</td><td>single</td><td>[0..50% of range]</td></tr><tr><td>Frequency_number</td><td>Number of frequencies</td><td>integer</td><td>≥ 1</td></tr><tr><td>Average_N(times</td><td>Number of repeat times (used for frequencies averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Correction</td><td>Non-stationary correction</td><td>boolean</td><td>True/False</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait before each frequency</td><td>single</td><td>≥ 0</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and ConstantsWarning : I Auto-range is not allowed</td></tr></table>

# 7.14.3. Data format

See SPEIS technique data format. 

# 7.14.4. Data conversion

See SPEIS technique data conversion. 

# 7.15. Differential Pulse Voltammetry technique


Technique ID: 127


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>dpv.ecc</td><td>dpv4.ecc 
dpv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/9538307c9590f3651ba4a4e0531be8fd1abe9bde6f373196455032cd026224be.jpg)


# 7.15.1. Description

DPV is very useful technique for analytical determination (for example metal ion quantification in a sample). The differential measurements discriminate faradic current from capacitive one. 

In this technique, the applied waveform is the sum of a pulse train and a staircase from the initial potential (Ei) to a final potential (Ef). The current is sampled just before the pulse and near the end of the pulse. The resulting current is the difference between these two currents. It has a relatively flat baseline. The current peak height is directly related to the concentration of the electroactive species in the electrochemical cell. 

# 7.15.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">DPV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Ef</td><td>Final potential (v)</td><td>single</td><td>-</td></tr><tr><td>OCf</td><td>Final potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>SH</td><td>Step height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>ST</td><td>Step width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td rowspan="2">Select the part of the current step. 1 = 100 % used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning: I Auto-range is not allowed</td></tr></table>

# 7.15.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/28143912b3023b5dfc25ef2cb93a0359ad21a536928dee4455adc656e196bb01.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e3fcc10e606ab8b4bbfc8bcf4a32f23e63491765bc7cda370c38509dd03bd700.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.15.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.16. Square Wave Voltammetry technique


Technique ID: 128


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>swv.ecc</td><td>swv4.ecc 
swv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/5cd9685c6070ab615ad8d36dc824f8dc990d78dca74cdaeae8b3c296ab7a1a7a.jpg)


# 7.16.1. Description

Among the electroanalytical techniques, the Square Wave Voltammetry (SWV) combines the background suppression, the sensitivity of DPV and the diagnostic value of NPV. 

The SWV is a large amplitude differential technique characterized by a pulse height (PH) and a pulse width (PW).The pulse width can be expressed in term of square wave frequency $\mathsf { f } { = } 1 / ( 2 \mathsf { P } \mathsf { W } )$ .The scan rate is $\mathsf { v } = \mathsf { P H } / ( 2 \mathsf { P W } )$ . The current is sampled twice, once at the end of the forward pulse and once at the end of the reverse pulse. The difference between the two measurements is plotted versus the base staircase potential. The resulting peak-shaped voltammogram is symmetrical about the halfwave potential and the peak current is proportional to the concentration. 

Excellent sensitivity accrues from the fact that the net current is larger than either the forward or reverse components (since it is the difference between them). 

# 7.16.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">SWV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Ef</td><td>Final potential (v)</td><td>single</td><td>-</td></tr><tr><td>OCf</td><td>Final potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>SH</td><td>Step height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and ConstantsWarning : I Auto-range is not allowed</td></tr></table>

# 7.16.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/b19200680ac1a74236b52ebf2eb3b8d5811b8acc45229a90aaa008943fb04ec1.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/8cb8245c94b6f7ad7f4b7734eac6b01dd7b72520768e140b9e4cf1821959eb72.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.16.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, 

# 7.17. Normal Pulse Voltammetry technique


Technique ID: 129


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>npv.ecc</td><td>npv4.ecc
npv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/1c9ec7cedee531f6273837705f02dce10b44123577f185ade175ba004e37ef79.jpg)


# 7.17.1. Description

Pulsed techniques have been introduced to increase the ratio between the faradic and nonfaradic currents in order to permit a quantification of species to very low concentration levels. 

The Normal Pulse Voltammetry is one of the first pulsed techniques elaborated for polarography needs. An essential idea behind the NPV is the cyclic renewal of the diffusion layer. With a DME, this is achieved by the stirring accompanying the fall of the mercury drop. But at other electrodes renewal may not be so easily accomplished. 

NPV consists of a series of pulses of linear increasing amplitude (from Ei to Ef). The potential pulse is ended by a return to the base value Ei. The usual practice is to select Ei in a region where the electroactive species of interest does not react at the electrode. The current is sampled at a time t near the end of the pulse and at a time t’ before the pulse. The plotted current is the difference of both currents measured at the end of the pulse (forward) and at the end of the period previous to the pulse (reverse). 

# 7.17.2. Technique parameters


Technique parameters available for the function BL_LoadTechnique:


<table><tr><td colspan="4">NPV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Ef</td><td>Final potential (v)</td><td>single</td><td>-</td></tr><tr><td>OCf</td><td>Final potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr></table>


NPV parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>ST</td><td>Step width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants Warning: I Auto-range is not allowed</td></tr></table>

# 7.17.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/01bb7d6fc698fcfd7ec6752e1c493e7935328f2d46a5ff83df8e9fed49d42330.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/bb88ced58827c7da90d0379674b80ab94d5337aa688607c31818b97221d66226.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.17.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function 

BL_ConvertChannelNumericIntoSingle 

# 7.18. Reverse Normal Pulse Voltammetry technique


Technique ID: 130


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>rnpv.ecc</td><td>rnpv4.ecc 
rnpv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f318420378435b696ef37e1600b187a456513bd59a13f9512bccd24af98e6fa8.jpg)


# 7.18.1. Description

The Reverse Normal Pulse Voltammetry is a derivative technique from the NPV. The main difference is that the initial (base) potential Ei is placed in the diffusion-limited region for electrolysis of the species present in the bulk solution. The pulses are made through the region where the species in solution is not electroactive. 

The RNPV experiment involves a significant faradic current. This method is a reversal experiment because of the detection of the product from a prior electrolysis. 

# 7.18.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">RNPV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Ef</td><td>Final potential (v)</td><td>single</td><td>-</td></tr><tr><td>OCf</td><td>Final potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>ST</td><td>Step width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning: I Auto-range is not allowed</td></tr></table>

# 7.18.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e65198ea126a6a5813e0bde1deee5ae11cd50e563a4c949170b46e49c10661b6.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/6476be872141836bbcb3b9636ea51b19e47bc2a221414884c3c6d8b96ce21bba.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.18.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.19. Differential Normal Pulse Voltammetry technique


Technique ID: 131


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>dnpv.ecc</td><td>dnpv4.ecc 
dnpv5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/c70ef52a2788361b7e687718680d39ac1a40b8a2a75c3709ef069a9e02797402.jpg)


# 7.19.1. Description

Originally introduced as a polarographic technique (performed at a DME), the Differential Normal Pulse Voltammetry is a sensitive electroanalytical technique very close to the DPV technique with a pulsed potential sweep. The potential pulse is swept from an initial potential Ei to a final potential Ef. There are two main differences with the DPV technique: first the pulse waveform is made with a prepulse (SH amplitude with PPW duration) before the pulse (PH amplitude with PW duration) and second the potential always comes back to the initial potential (Ei) after the pulsed sequence. Ei is assumed to be the potential where no faradic reaction occurs. The plotted current is the difference of both currents measured at the end of the pulse (I forward) and at the end of the prepulse (I reverse). 

This technique is often used in polarography and by biologists to define the most appropriate potential for the electrochemical detection to a fixed potential with the DPA technique. 

# 7.19.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">DNPV parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Ef</td><td>Final potential (v)</td><td>single</td><td>-</td></tr><tr><td>OCf</td><td>Final potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>PPW</td><td>PrePulse Width</td><td>single</td><td>≥ 0</td></tr></table>


DNPV parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>SH</td><td>Step height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>ST</td><td>Step width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and ConstantsWarning: I Auto-range is not allowed</td></tr></table>

# 7.19.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/30ba80e7b3b04bc900d92f1b880405fddb66c57cb7f41ef9f7b9e33709c45901.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/51e0a175b7fc512647b2bddc1395dd98434c9027bc6d03db4473162d4e7c3e81.jpg)


The charge can be recorded additionnally using XCTR. 

The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.19.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.20. Differential Pulse Amperometry technique


Technique ID: 132


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>dpa.ecc</td><td>dpa4.ecc 
dpa5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/4db37191875d9459de93447fc8eae203df87744cd4d05dc63e88fa146aa92394.jpg)


# 7.20.1. Description

The Differential Pulse Amperometry results from the DNPV technique without increasing pulse steps. The potential waveform and the current sampling are the same as for DNPV. A DPA experiment is often used as a sensitive method for the quantification of electrochemical species at a defined potential (Es). This potential value is often determined with a DNPV experiment (using a potential sweep with the same waveform) previously performed. This technique is dedicated to the quantification of biological electroactive species. 

# 7.20.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">DPA parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>PPH</td><td>PrePulse height</td><td>single</td><td>≥ 0</td></tr><tr><td>PPW</td><td>PrePulse Width</td><td>single</td><td>≥ 0</td></tr><tr><td>PH</td><td>Pulse height (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>PW</td><td>Pulse width (ms)</td><td>single</td><td>≥ 0</td></tr><tr><td>P</td><td>Period (mV)</td><td>single</td><td>≥ 0</td></tr><tr><td>Tp</td><td>Duration (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning: I Auto-range is not allowed</td></tr></table>

# 7.20.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e31a13dd899b336fd207de11c40e16c02d8e232379e9778603f31de397676c63.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a558a6d0362c53135e4adbee1c4505ee686c6a8cb522e8fdff3338057626a39a.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.20.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.21. Ecorr. Vs Time technique


Technique ID: 133


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>evt.ecc</td><td>evt4.ecc 
evt5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/013121c4f999a72734eb1cada422e9da6a0836c42f91a76a118525da4c80e22e.jpg)


# 7.21.1. Description

This technique corresponds to the follow up of the corrosion potential (when the circuit is open) versus time. During the measurement no potential or current is applied to the cell. 

# 7.21.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">EVT parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Record每一天_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record每一天_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr></table>

# 7.21.3. Data format

Data format returned by the function BL_GetData: 

VMP3 series: 

<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>6</td><td>7</td><td>...</td></tr><tr><td>t_high</td><td>t_low</td><td>Ewe</td><td>Ece</td><td>t_high</td><td>t_low</td><td>Ewe</td><td>Ece</td><td>...</td></tr><tr><td colspan="4">Point #0</td><td colspan="5">Point #1</td></tr></table>

VMP-300 series: 

<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>...</td></tr><tr><td>t_high</td><td>t_low</td><td>Ewe</td><td>t_high</td><td>t_low</td><td>Ewe</td><td>...</td></tr><tr><td colspan="3">Point #0</td><td colspan="3">Point #1</td><td></td></tr></table>

The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.21.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: Ewe and Ece must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.22. Linear Polarization technique


Technique ID: 134


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>lp.ecc</td><td>lp4.ecc
lp5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/c24c2c2a4e43deaa347ae0e3f745ed1b79589233e01ac220a70fce881beae5cb.jpg)


# 7.22.1. Description

The linear polarization technique is used in corrosion monitoring. This technique is especially designed for the determination of a polarization resistance Rp of a material and Icorr through potential steps around the corrosion potential. 

Rp is defined as the slope of the potential-current density curve at the free corrosion potential: Rp = (dE/dI) dE- ${ \tt > } 0$ 

Rp is determined using the ''Rp fit'' graphic tool. Contrary to the Potentiodynamic Pitting (PDP) technique, no current limitation is available with the linear polarization technique. 

# 7.22.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 


LP parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record EVERY_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>OC1</td><td>Step voltage vs initial one (not used)</td><td>boolean</td><td>True/False</td></tr><tr><td>E1</td><td>Step voltage (V) (not used)</td><td>single</td><td>≥ 0</td></tr><tr><td>T1</td><td>Step duration (s) (not used)</td><td>single</td><td>≥ 0</td></tr><tr><td>vs.initial_scan</td><td>Current scan vs initial one</td><td>Array of 2 boolean</td><td>True/False</td></tr><tr><td>Voltage_scan</td><td>Voltage scan (V)</td><td>Array of 2</td><td>-</td></tr></table>


LP parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td></td><td></td><td>single</td><td></td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 2 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>= 0</td></tr><tr><td>Record EVERY_dE</td><td>recording on dE</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr></table>

# 7.22.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/ea0f24dd66c119ecd4fad1e0c7e7035dcb26ab951b6581ce784aa0608a17d2c5.jpg)


Data format of process 1: 

VMP3 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/54a60c212aa5a76f6153056b8a146b64e313c6612960a3f162af023393480082.jpg)


VMP-300 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/ff3d8652f3a13ea96cb6875ba6be2c5b6c08e19bd7be74291ee76a08473b6286.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.22.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.23. Generalized Corrosion technique


Technique ID: 135


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>gc.ecc</td><td>gc4.ecc 
gc5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/1d3d6a1d63157ae964ac4f0c683790b3c5aff811d9f43a9b1753ed2cd77361a6.jpg)


# 7.23.1. Description

The generalized corrosion technique is applied for general corrosion (sometimes called uniform corrosion) study. For this corrosion, anodic dissolution is uniformly distributed over the entire metallic surface. The corrosion rate is nearly constant at all locations. 

Microscopic anodes and cathodes are continuously changing their electrochemical behavior from anode to cathode cells for a uniform attack. 

# 7.23.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">GC parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record EVERY_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>OC1</td><td>Step voltage vs initial one (not used)</td><td>boolean</td><td>True/False</td></tr><tr><td>E1</td><td>Step voltage (V) (not used)</td><td>single</td><td>≥ 0</td></tr><tr><td>T1</td><td>Step duration (s) (not used)</td><td>single</td><td>≥ 0</td></tr><tr><td>vs.initial_scan</td><td>Current scan vs initial one</td><td>Array of 3 boolean</td><td>True/False</td></tr><tr><td>Voltage_scan</td><td>Voltage scan (V)</td><td>Array of 3 single</td><td>-</td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 3 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>= 1</td></tr><tr><td>Record_every_dE</td><td>recording on dE</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr></table>

# 7.23.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/751f592716f210bb274d9c9f626249d1861fbe06f1e7bf67fc32caaaf44930df.jpg)


Data format of process 1: 

For VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a147d4264e8de0beb4474a2be4005b7d9d25fddb33d287f9b42227ea3a2a264c.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/8c781cecaf63bcc8a9d98db9cceee1b22395ecab573287dab0d5fe22974172e5.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.23.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.24. Cyclic PotentioDynamic Polarization technique


Technique ID: 136


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>cpp.ecc</td><td>cpp4.ecc 
cpp5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>44μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/30c09566fdc9f69a4cac1481f3fa6cfc66f65efb07788416d8ce648bab008f78.jpg)


# 7.24.1. Description

The Cyclic Potentiodynamic Polarization is often used to evaluate pitting susceptibility. It is the most common electrochemical test for localized corrosion resistance. The potential is swept in a single cycle or slightly less than one cycle. The size of the hysteresis is examined along with the difference between the values of the starting Open circuit corrosion potential and the return passivation potential. The existence of hysteresis is usually indicative of pitting, while the size of the loop is often related to the amount of pitting. This technique can be used to determine the pitting potential and the repassivation potential. 

This technique is based both on the PDP and PSP techniques. It begins with a potentiodynamic phase where the potential increases. This phase is limited either with a limit potential (EL) or with a pitting current (Ip) defined by the user. If the pitting current is not reached during the potentiodynamic phase, then a potentiostatic phase is applied until pitting (Ip is reached). Ip can be used as a safety parameter in order to avoid damages on the working electrode. Then an additional potentiodynamic phase is done as a reverse scan. 

# 7.24.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 


CPP parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record EVERY_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>vs_initializer_scan</td><td>Current scan vs initial one</td><td>Array of 3 boolean</td><td>True/False</td></tr><tr><td>Voltage_scan</td><td>Voltage scan (V)</td><td>Array of 3 single</td><td>-</td></tr></table>


CPP parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 3 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>= 1</td></tr><tr><td>I_pitting</td><td>Pitting current</td><td>single</td><td>≥ 0</td></tr><tr><td>t_b</td><td>Check condition |I|&gt;Ip after time t_b</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_Every_dE</td><td>recording on dE</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>Record_Every_dT</td><td>recording on dt</td><td>single</td><td>≥ 0</td></tr></table>

# 7.24.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/97459055ce16085f1322ce49b1d240df8b07ef6a23fd482a89e5f502f14328a9.jpg)


Data format of process 1: 

VMP3 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/02143d63a2ec62c6332ce5c93c243ae23e80522ea34e1a6fd7146f55fdf641de.jpg)


VMP-300 Series: 

<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>6</td><td>7</td><td>...</td></tr><tr><td>t_high</td><td>t_low</td><td>&lt;I&gt;</td><td>&lt;Ewe&gt;</td><td>t_high</td><td>t_low</td><td>&lt;I&gt;</td><td>&lt;Ewe&gt;</td><td>...</td></tr><tr><td colspan="4">Point #0</td><td colspan="4">Point #1</td><td></td></tr></table>

The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.24.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.25. PotentioDynamic Pitting technique


Technique ID: 137


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>pdp.ecc</td><td>pdp4.ecc 
pdp5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>42μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/5d7ba0a5f26947eb1861a1b928ae9202f1f946cca1bf93f5053d4820901583a3.jpg)


# 7.25.1. Description

Pitting corrosion occurs when discrete areas of a material undergo rapid attack while the vast majority of the surface remains virtually unaffected. The basic requirement for pitting is the existence of a passive state for the material in the environment of interest. Pitting of a given material depends strongly upon the presence of an aggressive species in the environment and a sufficiently oxidizing potential. 

This technique corresponds to the pitting potential determination of a material, using a potential sweep. The experiment stops when a pitting current (Ip) defined by the user is reached. Ip can be used as a safety parameter in order to avoid damages on the working electrode. 

# 7.25.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">PDP parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record EVERY_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record EVERY_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>vs_initializer_scan</td><td>Current scan vs initial one</td><td>Array of 2 boolean</td><td>True/False</td></tr><tr><td>Voltage_scan</td><td>Voltage scan (V)</td><td>Array of 2 single</td><td>-</td></tr><tr><td>Scan_Rate</td><td>slew rate array (mV/s)</td><td>Array of 2 single</td><td>≥ 0</td></tr><tr><td>Scan_number</td><td>Scan number</td><td>integer</td><td>= 1</td></tr></table>


PDP parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>I_pitting</td><td>Pitting current</td><td>single</td><td>≥ 0</td></tr><tr><td>t_b</td><td>Check condition |I|&gt;Ip after time t_b</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_every_dE</td><td>recording on dE</td><td>single</td><td>≥ 0</td></tr><tr><td>Average_over_dE</td><td>average every dE</td><td>boolean</td><td>True/False</td></tr><tr><td>Begin_MEasuring_I</td><td>elect the part of the current step (1 = 100%)</td><td>single</td><td>[0..1]</td></tr><tr><td>End/measuring_I</td><td>used for data averaging</td><td>single</td><td>[0..1]</td></tr><tr><td>Record_every_dT</td><td>recording on dt</td><td>single</td><td>≥ 0</td></tr><tr><td>Hold</td><td>Hold potential</td><td>boolean</td><td>True/False</td></tr></table>

# 7.25.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/dc2a177a3f0f87485435402b89c06c96d1adedad9635acbdda4ac72889a121da.jpg)


Data format of process 1: 

VMP3 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a8a21fb02f6253069ae354b9eaac237e2db87d03a8a681ac7491581cc25fa557.jpg)


VMP-300 Series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e1cb85c5a87e976207c1437a897e186e07e9c4894fae117d605c51a2efc34008.jpg)


<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>6</td><td>7</td><td>...</td></tr><tr><td></td><td colspan="4">Point #0</td><td colspan="4">Point #1</td></tr></table>

The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.25.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.26. PotentioStatic Pitting technique


Technique ID: 138


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>psp.ecc</td><td>psp4.ecc 
psp5.ecc</td></tr><tr><td>Timebase</td><td>24μs</td><td>24μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/84da0640fec0d06abd73d67ac78446e0adc8e2c53b8a203b743503444287dc47.jpg)


# 7.26.1. Description

The PSP technique corresponds to studying pitting occurrence under applied constant potential. 

The experiment stops when a pitting current (Ip) defined by the user is reached. Ip can be used as a safety parameter in order to avoid damages on the working electrode. 

# 7.26.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">PSP parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record_every_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record_every_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Ei</td><td>Initial Potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial Potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_every_dT</td><td>recording on dt</td><td>single</td><td>≥ 0</td></tr><tr><td>Record_every_dI</td><td>recording on dI (A)</td><td>single</td><td>≥ 0</td></tr><tr><td>I_pitting</td><td>Pitting current</td><td>single</td><td>≥ 0</td></tr><tr><td>t_b</td><td>Check condition |I|&gt;Ip after time t_b</td><td>single</td><td>≥ 0</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange</td></tr><tr><td></td><td></td><td></td><td>constants authorized on section 5. Structures and Constants</td></tr><tr><td></td><td></td><td></td><td>Warning: I Auto-range is not allowed</td></tr></table>

# 7.26.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>...</td></tr><tr><td>t_high</td><td>t_low</td><td>Ewe</td><td>t_high</td><td>t_low</td><td>Ewe</td><td>...</td></tr><tr><td colspan="3">Point #0</td><td colspan="3">Point #1</td><td></td></tr></table>

Data format of process 1: 

<table><tr><td>0</td><td>1</td><td>2</td><td>3</td><td>4</td><td>5</td><td>6</td><td>7</td><td>...</td></tr><tr><td>t_high</td><td>t_low</td><td>Ewe</td><td>I</td><td>t_high</td><td>t_low</td><td>Ewe</td><td>I</td><td>...</td></tr><tr><td colspan="4">Point #0</td><td colspan="5">Point #1</td></tr></table>

The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.26.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.27. Zero Resistance Ammeter technique


Technique ID: 139


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>zra.ecc</td><td>zra4.ecc 
zra5.ecc</td></tr><tr><td>Timebase</td><td>40μs</td><td>40μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/0c5052c59ff48e65b2564ccd4143b732a8b1d654cb56909338600add99a6f128.jpg)


# 7.27.1. Description

The Zero Resistance Ammeter technique is used to examine the effects of coupling dissimilar metals and to perform some types of electrochemical noise measurement. 

This technique consists into applying zero volts between the working electrode (WE) and the counter electrode (CE) and then measures the current and the potentials (Ewe, Ece) versus the reference electrode (REF). 

# 7.27.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">ZRA parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record每一天_dEr</td><td>Record every dE (V)</td><td>single</td><td>≥ 0</td></tr><tr><td>Rest_time_T</td><td>Rest duration (s)</td><td>single</td><td>[0..tb*231]</td></tr><tr><td>Record每一天_dTr</td><td>Record every dT (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Ei</td><td>Initial Potential (V)</td><td>single</td><td>-</td></tr><tr><td>OCi</td><td>Initial Potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Rest_time_Ti</td><td>Ei duration (s)</td><td>single</td><td>≥ 0</td></tr><tr><td>Record每一天_dT</td><td>recording on dt</td><td>single</td><td>≥ 0</td></tr><tr><td>Record每一天_dI</td><td>recording on dI (A)</td><td>single</td><td>≥ 0</td></tr><tr><td>I_max</td><td>Pitting current</td><td>single</td><td>≥ 0</td></tr><tr><td>t_b</td><td>Check condition |I|&gt;Ip after time t_b</td><td>single</td><td>≥ 0</td></tr></table>

# 7.27.3. Data format

Data format returned by the function BL_GetData 

Data format depends of the technique process used to record data. The process index is returned in the field TDataInfos.ProcessIndex. 

Data format of process 0: 

VMP3 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/ae45b0c18418ce355a8a763bb01899c01fc0b914ee053d1e0ed28635e1687a79.jpg)


VMP-300 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/1c1cce5a4ba87729ba0819a79ddcb8928e54e6bbd6d660417e9c38118be68096.jpg)


Data format of process 1: 

VMP3 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e6689de6f6b4c721867f5a316196cee5e2082eede24ee4a355ac07fc1ae90767.jpg)


VMP-300 series: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/548f990c75dc2820809e9e2f000166c9e101b326bf07df0a0b8ab047b0fa52b3.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.27.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: 

The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: 

time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce, Q-Qo must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.28. Manual IR technique

Technique ID: 140 

<table><tr><td>Instrument Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>IRcmp.ecc</td><td>IRcmp4.ecc
IRcmp5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

# 7.28.1. Description

The ohmic drop iRu is the voltage drop developed across the solution resistance Ru between the reference electrode and the working electrode, when current is flowing through. When the product iRu gets significant it introduces an important error in the control of the working electrode potential and should be compensated. 

In controlled potential techniques, The Manual IR (MIR) can be used to compensate the ohmic drop when the uncompensated solution resistance value (Ru) is known or measured before the experiment start. This technique will not measure Ru. 

When used with linked techniques and loops, this technique allow the user to keep the same Ru value for each loop. The user can select the percentage of compensation. It is highly recommended to not exceed $85 \%$ of the Ru measured value in order to avoid oscillations of the instrument. 

# 7.28.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">MIR parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Rcmp_Value</td><td>R value to compensate</td><td>single</td><td>≥ 0</td></tr><tr><td>Rcmp_Mode</td><td>Ohmic compensation mode: 
0 = software 
1 = hardware (VMP-300 only)</td><td>integer</td><td>0 or 1</td></tr></table>

To deactivate the compensation, you can simply set the Rcmp_value to 0 and Rcmp_Mode to software. 

# 7.28.3. Data format

No data recorded by this technique, it has been designed for linked experiments. 

# 7.28.4. Data conversion

No data conversion. 

# 7.29. IR Determination with PotentioStatic Impedance technique


Technique ID: 141


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>pzir.ecc</td><td>pzir4.ecc 
pzir5.ecc</td></tr><tr><td>Timebase</td><td>24μs</td><td>24μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/5a40030782aca70d0a3dd24edbd7afe0fbb2c71ba3e1ac8f7388b5040b826358.jpg)


# 7.29.1. Description

The ohmic drop iRu is the voltage drop developed across the solution resistance Ru between the reference electrode and the working electrode, when current is flowing through. When the product iRu gets significant it introduces an important error in the control of the working electrode potential and should be compensated. 

The IR Determination with Potentiostatic Impedance (PZIR) technique utilizes Impedance measurements to determine the Ru Value. This technique applies a sinusoidal excitation around the DC potential measured at the beginning of the technique. PZIR technique determines the solution resistance Ru, for one high frequency value, as the real part of the measured impedance. A percentage of the Ru value will be used to compensate next potentio techniques. It is highly recommended to not exceed $85 \%$ of the Ru measured value in order to avoid oscillations of the instrument. The Rcmp_Mode parameter will allow to specify the compensation mode for the next potentio techniques (only for VMP-300 series). 

When used in linked techniques including loops, Ru value can change during the experiment. PZIR can be an ideal tool to do a dynamic ohmic drop compensation between repeated techniques. 

For low impedance electrochemical systems it is recommended to use GZIR instead of PZIR. 

# 7.29.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">PZIR parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Final_frequency</td><td>Final frequency (Hz)</td><td>single</td><td>= Initial_frequency</td></tr><tr><td>Initial_frequency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Amplitude_Voltage</td><td>Sinus amplitude (V)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Average_N(times</td><td>Number of repeat times 
(used for frequencies 
averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait 
before each frequency</td><td>single</td><td>≥ 0</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic 
(TRUE for linear points 
spacing)</td><td>boolean</td><td>= True</td></tr><tr><td>Rcomp_Level</td><td>% IR compensation</td><td>single</td><td>≥ 0</td></tr><tr><td>Rcmp_Mode</td><td>Ohmic drop compensation 
mode.</td><td>integer</td><td>0 or 1</td></tr><tr><td></td><td>0 = software</td><td></td><td></td></tr><tr><td></td><td>1 = hardware (VMP-300 
only)</td><td></td><td></td></tr></table>

To deactivate the compensation, you can simply set the Rcomp_Level to 0 and Rcmp_Mode to software. 

# 7.29.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/0eb8574e2bf16847977fa345b2af58834a8b3cc3fbee657a1299831fed24cfbb.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/664beb7ed8e3ac3824dcadf0006868794d56c030fb43084a08c95fbf1ab23e5b.jpg)


For VMP-300: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/d368b4fe87844c30a5bc255838613fa76e2f3d6f33d242a96f898902e45f3128.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.29.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.30. IR Determination with GalvanoStatic Impedance technique


Technique ID: 142


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>gzir.ecc</td><td>gzir4.ecc 
gzir5.ecc</td></tr><tr><td>Timebase</td><td>24μs</td><td>24μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/a1ef97e8a66cbde2cb5b3b3f5412e794d05422218cf14a4559075466db18aa24.jpg)


# 7.30.1. Description

The ohmic drop iRu is the voltage drop developed across the solution resistance Ru between the reference electrode and the working electrode, when current is flowing through. When the product iRu gets significant it introduces an important error in the control of the working electrode potential and should be compensated. 

The IR Determination with Galvanostatic Impedance (GZIR) technique utilizes Impedance measurements to determine the Ru Value. This technique applies a sinusoidal excitation around the DC current measured at the beginning of the technique. GZIR technique determines the solution resistance Ru, for one high frequency value, as the real part of the measured impedance. A percentage of the Ru value will be used to compensate next potentiostatic techniques. It is highly recommended to not exceed exceed $85 \%$ of the Ru measured value in order to avoid oscillations of the instrument. 

When used in linked techniques including loops, Ru value can change during the experiment. GZIR can be an ideal tool to do a dynamic ohmic drop compensation between repeated techniques. 

In the case of particular non-linear systems it can be necessary to use PZIR instead of GZIR. 

# 7.30.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">GZIR parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Final_freqency</td><td>Final frequency (Hz)</td><td>single</td><td>= Initial_freqency</td></tr><tr><td>Initial_freqency</td><td>Initial frequency (Hz)</td><td>single</td><td>Depend on instrument</td></tr><tr><td>Amplitude_Current</td><td>Sinus amplitude (A)</td><td>singleDepen d on instrument</td><td>Depend on instrument</td></tr><tr><td>Average_N(times</td><td>Number of repeat times (used for frequencies averaging)</td><td>integer</td><td>≥ 1</td></tr><tr><td>Rcomp_Level</td><td>% IR compensation</td><td>single</td><td>≥ 0</td></tr><tr><td>Wait_for_steady</td><td>Number of period to wait before each frequency</td><td>single</td><td>≥ 0</td></tr><tr><td>sweep</td><td>sweep linear/logarithmic (TRUE for linear points spacing)</td><td>boolean</td><td>= True</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants Warning: I Auto-range is not allowed</td></tr><tr><td>Rcmp_Mode</td><td>Ohmic drop compensation mode. 
0 = software 
1 = hardware</td><td>integer</td><td>0 or 1</td></tr></table>

To deactivate the compensation, you can simply set the Rcomp_Level to 0 and Rcmp_Mode to software. 

# 7.30.3. Data format

Data format returned by the function BL_GetData for VMP3: 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/aac753386a253e0746646396a727146cc8bee24517477cda5583eb6a5f90ba33.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/0379643b54fe471417041f11b9b99f70b8be7d04a4b8a33ffdb54f168c8546dc.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/4c179c58532b2d8e588224337a74de42e960532bb757564c9061981272495d57.jpg)



For VMP-300:


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/830251bc953ca4a87672a42c950b8123b615a110150abf6250e61e44b893111b.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/2cfad97bcd702c3e70fcfc0a4ae51830c753558f80868966a0c6fdf0930a5da8.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/81575ec752ea5b1c66ec24f1b96017812cdbaae5be72a43b4aadffc682bddcfa.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.30.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

time: The time for process 0 should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 

Float conversion: time (only time of process 1), IRange, freq, Ewe, |Ewe|, Ece, |Ece|, I, |I|, Phase Zwe, Phase Zce must be converted with the function BL_ConvertChannelNumericIntoSingle 

# 7.31. Loop technique

Technique ID: 150 

<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>loop.ecc</td><td>loop4.ecc 
loop5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

# 7.31.1. Description

The loop technique has been built to repeat all or a part of an experiment made with several linked techniques. The user has to define the technique number Ne where he wants to go back to $N e = 0$ for the first technique of the experiment). Then all the techniques linked after the selected one will be repeated. The user has to choose the number of time nt that the experiment will be looped. An experiment with $n t = 2$ will have three loops. The loop technique can be also used as a mandatory goto technique when the experiment will be looped for unlimited number of times. To activate this mode the user has to put $n t = - 1$ . 

# 7.31.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">Loop parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>loop_N(times</td><td>Loop N times</td><td>integer</td><td>≥ 1 for conditional goto -1 for mandatory goto</td></tr><tr><td>protocol_number</td><td>Index of the technique to be linked (index 0-based)</td><td>integer</td><td>≥ 0</td></tr></table>

# 7.31.3. Data format

No data recorded by this technique, it has been designed for linked experiments. 

# 7.31.4. Data conversion

No data conversion. 

# 7.32. Trigger Out technique


Technique ID: 151


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td rowspan="2">File</td><td rowspan="2">TO.ecc</td><td>TO4.ecc</td></tr><tr><td>TO5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

# 7.32.1. Description

The 'Trigger Out' technique can be used to synchronize a potentiostat channel with an external instrument. A trigger out pulse is generated by the potentiostat during the technique placed after the 'Trigger Out' technique. The pulse duration and level can be programmed by the user. The pulse cannot last more than the next technique duration. Before and after the pulse the potentiostat drives the trigger out signal to the default level set by the 'Trigger Out Set' technique. 

# 7.32.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">TO parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Trigger_Loic</td><td>Trigger out level</td><td>integer</td><td>0 or 1</td></tr><tr><td>Trigger_Duration</td><td>Trigger out duration (s)</td><td>single</td><td>≥ 0</td></tr></table>

# 7.32.3. Data format

No data recorded by this technique, it has been designed for linked experiments. 

# 7.32.4. Data conversion

No data conversion. 

# 7.33. Trigger In technique


Technique ID: 152


<table><tr><td>Instrument Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>TI.ecc</td><td>TI4.ecc
TI5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

# 7.33.1. Description

The 'Trigger In' technique can be used to synchronize a potentiostat channel with an external instrument. The potentiostat waits an external trigger to continue the experiment with the technique set after the 'Trigger In' technique. Before receiving the trigger the potentiostat goes to the next technique control mode. The trigger in signal is level sensitive and can be set to be either logic low or high. For the potentiostat to recognize the trigger a pulse must be set and held for a minimum of 100 µs. 

# 7.33.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">TI parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Trigger_Occurrence</td><td>Trigger in level</td><td>integer</td><td>0 or 1</td></tr></table>

# 7.33.3. Data format

No data recorded by this technique, it has been designed for linked experiments. 

# 7.33.4. Data conversion

No data conversion. 

# 7.34. Trigger Out Set technique

Technique ID: 153 

<table><tr><td>Instrument Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>TOS.ecc</td><td>TOS4.ecc
TOS5.ecc</td></tr><tr><td>Timebase</td><td>20μs</td><td>20μs</td></tr></table>

# 7.34.1. Description

The 'Trigger Out Set' technique can be used together with the 'Trigger Out' technique to synchronize the potentiostat with an external instrument. 'Trigger Out Set' technique sets the default level of the trigger out signal to be either at a logic low or high level. Before and after a pulse generated by the 'Trigger Out' technique the potentiostat drives the trigger out signal to the default level. The trigger out default level can be changed only by another execution of a 'Trigger Out Set' technique or by a power-up or reset of the potentiostat. 

# 7.34.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">TOS parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Trigger_Value</td><td>Trigger out value</td><td>integer</td><td>0 or 1</td></tr></table>

# 7.34.3. Data format

No data recorded by this technique, it has been designed for linked experiments. 

# 7.34.4. Data conversion

No data conversion. 

# 7.35. Large Amplitude Sinusoidal Voltammetry technique


Technique ID: 159


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>Iasv.ecc</td><td>Iasv4.ecc 
Iasv5.ecc</td></tr><tr><td>Timebase</td><td>50μs</td><td>50μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/09b62a152f1205e8fc65edf98ce81e8240b1cc785dd64e53eb744b210c5b1db6.jpg)


# 7.35.1. Description

Large Amplitude Sinusoidal Voltammetry (LASV) is an electrochemical technique where the potential excitation of the working electrode is a large amplitude sinusoidal waveform. Similar to the cyclic voltammetry (CV) technique, it gives qualitative and quantitative information on redox processes. In contrast to the CV, the double layer capacitive current is not subject to sharp transitions at reverse potentials. Since the electrochemical systems are non-linear the current response exhibits higher order harmonics at large sinusoidal amplitudes. Valuable information can be found from data analysis in the frequency domain. 

# 7.35.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 


LASV parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential</td><td>single</td><td>-</td></tr><tr><td>Ei_vs初始</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Fs</td><td>Frequency of applied sinusoidal</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>E1</td><td>High Potential of sinusoidal (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>E1_vs初始</td><td>Voltage E1 vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>E2</td><td>Low Potential of sinusoidal (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs初始</td><td>Voltage E2 vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Period_number</td><td>Number of periods</td><td>Array of 20 integer</td><td>≥ 0</td></tr></table>


LASV parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..19]</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

This parameters cannot be updated with BL_UpdateParameters function. 

# 7.35.3. Data format

See CP technique data format. 

# 7.35.4. Data conversion

See CP technique data conversion. 

# 7.36. Chrono-Potentiometry technique with limits


Technique ID: 155


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>cplimit.ecc</td><td>cplimit4.ecc 
cplimit5.ecc</td></tr><tr><td>Timebase</td><td>50μs</td><td>34μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/10f2414d606f3dc0ec9c9b25c58b6e03ec9f8eb59a331dfa1923df1d8c6c5c76.jpg)


# 7.36.1. Description

The Chronopotentiometry (CP) is a controlled current technique. The current is controlled and the potential is the variable determined as a function of time. The chronopotentiometry technique is similar to the Chronoamperometry technique, potential steps being replaced by current steps. The current is applied between the working and the counter electrode. 

This technique can be used for different kind of analysis or to investigate electrode kinetics. It is considered less sensitive than voltammetric techniques for analytical uses. Generally, the curves Ewe $\mathbf { \mu } = \mathbf { \mu } \mathsf { f } ( \mathsf { t } )$ contains plateaus that correspond to the redox potential of electroactive species. 

Three limits (tests) are available. These three limits can be combined with logical operator (AND, OR). A limit is defined with a variable, a compare operator, and a value. 

The variables are potential (E), current (I), potential on Auxiliary 1 (AUX1) or potential on auxiliary 2 (AUX2). 

The logical operators are $\ " < " 0 \mathsf { r } \ " > " .$ . 

The value is a single data type value. 

# 7.36.1. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CPLIMIT
parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Current_step</td><td>Current step (A)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Current step vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 20 single</td><td>[0 ... tb*2^31]</td></tr><tr><td>Step_number</td><td>Number of steps</td><td>integer</td><td>[0 ... 19]</td></tr><tr><td></td><td>minus 1</td><td></td><td></td></tr><tr><td>Record_every_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0.0</td></tr><tr><td>Record_every_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0.0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr><tr><td>Test1_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test1_Value</td><td>Value of Test1 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test2_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test2_Value</td><td>Value of Test2 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test3_Config</td><td>Configuration of Test3 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test3_Value</td><td>Value of Test3 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Exit_Cond</td><td>Exit condition</td><td>Array of 20 integer</td><td>0: Next Step
1: Next Technique
2: STOP Experiment</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants allowed on section 5.
Structures and Constants
Warning: I Auto-range is not allowed</td></tr></table>

Test configuration format: 

The test configuration is a 32-bits integer. 

<table><tr><td>31</td><td>...</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td></tr><tr><td colspan="3">Variable</td><td colspan="3">Sign</td><td>Logic</td><td>Active</td></tr><tr><td colspan="3" rowspan="2">E (Voltage) = 0
AUX1 (Auxiliary Input 1) = 1
AUX2 (Auxiliary Input 2) = 2
I (Current) = 3</td><td colspan="3">"&lt;= " = 0</td><td>OR = 0</td><td>active = 1</td></tr><tr><td>"&gt;=" = 1</td><td>AND = 1</td><td colspan="5">not active = 0</td></tr></table>

If Test3 is active, Test1 and Test2 must be active. The Test3 is ignored if Test2 or Test1 is inactive. The comparison formula is: (Test1 AND/OR Test2) AND/OR Test3. 

If Test2 is active, Test1 must be active. The Test2 is ignored if Test1 is inactive. The comparison formula is: Test1 AND/OR Test2. 

Test value format: 

The test value is a single-precision floating point value. It should be set regarding the test configuration: 

- the limit in V for the configuration with E; 

- the limit in V for the configuration with AUX1; 

- the limit in V for the configuration with AUX2; 

- the limit in A for the configuration with I. 

# 7.36.2. Data format

See CP technique data format. 

# 7.36.3. Data conversion

See CP technique data format. 

# 7.37. Chrono-Amperometry technique with limits


Technique ID: 157


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>calimit.ecc</td><td>calimit4.ecc 
calimit5.ecc</td></tr><tr><td>Timebase</td><td>50μs</td><td>34μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/376e4a5ce70c9b03cbf4c26f0f4865f978d323fea35d8ae23bbfd91c8444fa5c.jpg)


# 7.37.1. Description

The basis of the controlled-potential techniques is the measurement of the current response to an applied potential step. 

The Chronoamperometry (CA) technique involves stepping the potential of the working electrode from an initial potential, at which (generally) no faradic reaction occurs, to a potential Ei at which the faradic reaction occurs. The current-time response reflects the change in the concentration gradient in the vicinity of the surface. 

Chronoamperometry is often used for measuring the diffusion coefficient of electroactive species or the surface area of the working electrode. This technique can also be applied to the study of electrode processes mechanisms. 

An alternative and very useful mode for recording the electrochemical response is to integrate the current, so that one obtains the charge passed as a function of time. This is the chronocoulometric mode that is particularly used for measuring the quantity of adsorbed reactants. 

Three limits (tests) are available. These three limits can be combined with logical operator (AND, OR). A limit is defined with a variable, a compare operator, and a value. 

The variables are potential (E), current (I), potential on Auxiliary 1 (AUX1) or potential on auxiliary 2 (AUX2). 

The logical operators are “<” or $\ " > \prime \prime$ . 

The value is a single data type value. 

# 7.37.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CALIMIT
parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Voltage_step</td><td>Voltage step (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs_initial</td><td>Voltage step vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 20 single</td><td>[0 ... tb*231]</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0 ... 19]</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>single</td><td>≥ 0.0</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>single</td><td>≥ 0.0</td></tr><tr><td>Test1_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test1_Value</td><td>Value of Test1 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test2_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test2_Value</td><td>Value of Test2 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test3_Config</td><td>Configuration of Test3 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test3_Value</td><td>Value of Test3 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Exit_Cond</td><td>Exit condition</td><td>Array of 20 integer</td><td>0: Next Step
1: Next Technique
2: STOP
Experiment</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

Test configuration format: 

The test configuration is a 32-bits integer. 

<table><tr><td>31</td><td>...</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td></tr><tr><td colspan="3">Variable</td><td colspan="3">Sign</td><td>Logic</td><td>Active</td></tr><tr><td colspan="3" rowspan="2">E (Voltage) = 0
AUX1 (Auxiliary Input 1) = 1
AUX2 (Auxiliary Input 2) = 2
I (Current) = 3</td><td colspan="3">"&lt;= " = 0</td><td>OR = 0</td><td>active = 1</td></tr><tr><td>"&gt;=" = 1</td><td>AND = 1</td><td colspan="5">not active = 0</td></tr></table>

If Test3 is active, Test1 and Test2 must be active. The Test3 is ignored if Test2 or Test1 is inactive. The comparison formula is: (Test1 AND/OR Test2) AND/OR Test3. 

If Test2 is active, Test1 must be active. The Test2 is ignored if Test1 is inactive. The comparison formula is: Test1 AND/OR Test2. 

Test value format: 

The test value is a single-precision floating point value. It should be set regarding the test configuration: 

- the limit in V for the configuration with E; 

- the limit in V for the configuration with AUX1; 

- the limit in V for the configuration with AUX2; 

- the limit in A for the configuration with I. 

# 7.37.3. Data format

See CP technique data format. 

# 7.37.4. Data conversion

See CP technique data conversion. 

# 7.38. Voltage Scan technique with limits


Technique ID: 158


<table><tr><td>Instrument Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>vscanlimit.ecc</td><td>vscanlimit4.ecc
vscanlimit5.ecc</td></tr><tr><td>Timebase</td><td>60μs</td><td>60μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/0a8cdf2b1721cbec5dd1aa5c2a4f1969af72952a152551e354bfe74764f65cac.jpg)


# 7.38.1. Description

The Potentiodynamic (PDYN) technique allows the user to perform potentiodynamic periods with different scan rates. 

Three limits (tests) are available. These three limits can be combined with logical operator (AND, OR). A limit is defined with a variable, a compare operator, and a value. 

The variables are potential (E), current (I), potential on Auxiliary 1 (AUX1) or potential on auxiliary 2 (AUX2). 

The logical operators are “<” or $\ " > \prime \prime$ 

The value is a single data type value. 

# 7.38.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">PDYNLIMIT parameters</td></tr><tr><td>Parameters</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Voltage_step</td><td>Vertex potential (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Vertex potential vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Scan_Rate</td><td>Scan rate (V/s) from previous vertex potential</td><td>Array of 20 single</td><td>&gt;0, Value of the first scan-rate is ignored</td></tr><tr><td>Scan_number</td><td>Number of scans minus 1</td><td>integer</td><td>[0 ... 19]</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>single</td><td>≥ 0.0</td></tr><tr><td>N_Cycles</td><td>Number of times the</td><td>integer</td><td>≥ 0</td></tr></table>


PDYNLIMIT parameters


<table><tr><td>Parameters</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td></td><td>technique is repeated</td><td></td><td></td></tr><tr><td>Begin_MEasuring_I</td><td>Select the part of the potential step.</td><td>si [0 n .0 gl ... e 1. 0]</td><td></td></tr><tr><td>End/measuring_I</td><td>1 = 100% used for data averaging</td><td>si [0 n .0 gl ... e 1. 0]</td><td></td></tr><tr><td>Test1_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test1_Value</td><td>Value of Test1 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test2_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test2_Value</td><td>Value of Test2 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test3_Config</td><td>Configuration of Test3 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test3_Value</td><td>Value of Test3 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Exit_Cond</td><td>Exit condition</td><td>Array of 20 integer</td><td>0: Next Step
1: Next Technique
2: STOP
Experiment</td></tr></table>

Test configuration format: 

The test configuration is a 32-bits integer. 

<table><tr><td>31</td><td>...</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td></tr><tr><td colspan="3">Variable</td><td colspan="3">Sign</td><td>Logic</td><td>Active</td></tr><tr><td colspan="3">E (Voltage) = 0</td><td></td><td></td><td></td><td></td><td></td></tr><tr><td colspan="3">AUX1 (Auxiliary Input 1) = 1</td><td colspan="3">“&lt;”= 0</td><td>OR = 0</td><td>active = 1</td></tr><tr><td colspan="3">AUX2 (Auxiliary Input 2) = 2</td><td colspan="3">“&gt;”= 1</td><td>AND = 1</td><td>not active = 0</td></tr><tr><td colspan="3">I (Current) = 3</td><td></td><td></td><td></td><td></td><td></td></tr></table>

If Test3 is active, Test1 and Test2 must be active. The Test3 is ignored if Test2 or Test1 is inactive. The comparison formula is: (Test1 AND/OR Test2) AND/OR Test3. 

If Test2 is active, Test1 must be active. The Test2 is ignored if Test1 is inactive. The comparison formula is: Test1 AND/OR Test2. 

Test value format: 

The test value is a single-precision floating point value. It should be set regarding the test configuration: 

- the limit in V for the configuration with E; 

- the limit in V for the configuration with AUX1; 

- the limit in V for the configuration with AUX2; 

- the limit in A for the configuration with I. 

# 7.38.3. Data format

See PDYN technique data format. 

# 7.38.4. Data conversion

See PDYN technique data conversion. 

# 7.38.5. Delphi Example

```txt
var ID: int32; {device identifier}  
function LoadTechniques: boolean;  
var  
{VSCANLIMIT}  
EccParamArray_VSCAN: array of TEccParam;  
EccParams_VSCAN: TEccParams;  
fname_VSCAN: Pchar;  
begin  
Result:= FALSE;  
fname_VSCAN:= nil;  
SetLength(EccParamArray_VSCAN, 28);  
try  
{Define VSCANLIMIT parameters}  
fname_VSCAN:= StrNew('vscanlimit4.ecc');  
{Vertex #0}  
BL_SetSglParameter ('Voltage_step', 0.0, 0, @EccParamArray_VSCAN[0]);  
BL_SetBoolParameter ('vs_initializer', FALSE, 0, @EccParamArray_VSCAN[1]);  
BL_SetSglParameter ('Scan_Rate', 0.0, 0, @EccParamArray_VSCAN[2]);  
{Limits #0: ((E < 1.0V) OR (AUX2 > 3.0V)) AND (I < 0.05A)}  
BL_SetIntParameter ('Test1_Config', 1, 0, @EccParamArray_VSCAN[3]);  
BL_SetSglParameter ('Test1_Value', 1.0, 0, @EccParamArray_VSCAN[4]);  
BL_SetIntParameter ('Test2_Config', 71, 0, @EccParamArray_VSCAN[5]);  
BL_SetSglParameter ('Test2_Value', 3.0, 0, @EccParamArray_VSCAN[6]);  
BL_SetIntParameter ('Test3_Config', 97, 0, @EccParamArray_VSCAN[7]);  
BL_SetSglParameter ('Test3_Value', 0.05, 0, @EccParamArray_VSCAN[8]);  
BL_SetIntParameter ('Exit_Cond', 0, 0, @EccParamArray_VSCAN[9]);  
{Vertex #1}  
BL_SetSglParameter ('Voltage_step', 1.0, 1, @EccParamArray_VSCAN[10]);  
BL_SetBoolParameter ('vs_initializer', FALSE, 1, @EccParamArray_VSCAN[11]); 
```

```vhdl
BL_UnclearScanParam ( 'Scan_Rate' , 10.0, 1, @EccParamArray_VSCAN[12]); {Limits #1: (I > 0.1A)}   
BL_UnclearScanParam ( 'Test1_Config' , 101, 1, @EccParamArray_VSCAN[13]); BL_UnclearScanParam ( 'Test1_Value' , 0.1, 1, @EccParamArray_VSCAN[14]); BL_UnclearScanParam ( 'Test2_Config' , 0, 1, @EccParamArray_VSCAN[15]); BL_UnclearScanParam ( 'Test2_Value' , 0.0, 1, @EccParamArray_VSCAN[16]); BL_UnclearScanParam ( 'Test3_Config' , 0, 1, @EccParamArray_VSCAN[17]); BL_UnclearScanParam ( 'Test3_Value' , 0.0, 1, @EccParamArray_VSCAN[18]); BL_UnclearScanParam ( 'Exit_Cond' , 2, 1, @EccParamArray_VSCAN[19]); {Misc.}   
BL_UnclearScanParam ( 'Scan_number' , 2, 0, @EccParamArray_VSCAN[20]);   
BL_UnclearScanParam ( 'N_Cycles' , 0, 0, @EccParamArray_VSCAN[21]);   
BL_UnclearScanParam ( 'Record_every_dE' , 0.01, 0, @EccParamArray_VSCAN[22]);   
BL_UnclearScanParam ( 'Begin_MEasuring_I' , 0.4, 0, @EccParamArray_VSCAN[23]);   
BL_UnclearScanParam ( 'End_MEasuring_I' , 0.8, 0, @EccParamArray_VSCAN[24]);   
BL_UnclearScanParam ( 'I_Range' , IRANGE_10MA, 0, @EccParamArray_VSCAN[25]);   
BL_UnclearScanParam ( 'E_Range' , ERANGE_AUTO, 0, @EccParamArray_VSCAN[26]);   
BL_UnclearScanParam ( 'Bandwidth' , BANDWIDTH_5, 0, @EccParamArray_VSCAN[27]);   
{Load VSCANLIMIT on selected channel}   
EccParams_VSCAN.len:= length(EccParamArray_VSCAN);   
EccParams_VSCAN.pParams:= @EccParamArray_VSCAN[0];   
if BL_LoadTechnique(ID, {device identifier} {selected channel} {.*.ecc filename(c-string)} {parameters} {first technique} {last technique} {display params}   
Result:= TRUE;   
finally   
if fname_VSCAN <> nil then StrDestroy(fname_VSCAN); SetLength(EccParamArray_VSCAN, 0);   
end; 
```

# 7.39. Current Scan technique with limits


Technique ID: 156


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>iscanlimit.e 
cc</td><td>iscanlimit4.ecc 
iscanlimit5.ecc</td></tr><tr><td>Timebase</td><td>60μs</td><td>60μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/c478c0360492f459618568cb19d4e22c1a26e6a9f9a66d1c9f5f9ce9b5dfc3e3.jpg)


# 7.39.1. Description

The Galvanodynamic (GDYN) technique allows the user to perform galvanodynamic periods with different scan rates. 

Three limits (tests) are available. These three limits can be combined with logical operator (AND, OR). A limit is defined with a variable, a compare operator, and a value. 

The variables are potential (E), current (I), potential on Auxiliary 1 (AUX1) or potential on auxiliary 2 (AUX2). 

The logical operators are “<” or $\ " > \prime \prime$ 

The value is a single data type value. 

# 7.39.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">GDYNLIMIT parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Current_step</td><td>Vertex current (A)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Vertex current vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Scan_Rate</td><td>Scan rate (A/s) from previous vertex current</td><td>Array of 20 single</td><td>&gt;0.0</td></tr><tr><td>Scan_number</td><td>Number of scans minus 1</td><td>integer</td><td>[0 ... 19]</td></tr><tr><td>Record EVERY_dI</td><td>Record every dI (A)</td><td>single</td><td>≥0.0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥0</td></tr></table>


GDYNLIMIT parameters


<table><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Begin_MEasuring_</td><td rowspan="2">Select the part of the current step (1 = 100%) used for data averaging</td><td>single</td><td>[0.0 ... 1.0]</td></tr><tr><td>End/measuring_E</td><td>single</td><td>[0.0 ... 1.0]</td></tr><tr><td>Test1_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test1_Value</td><td>Value of Test1 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test2_Config</td><td>Configuration of Test1 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test2_Value</td><td>Value of Test2 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Test3_Config</td><td>Configuration of Test3 by step</td><td>Array of 20 integer</td><td>See format below</td></tr><tr><td>Test3_Value</td><td>Value of Test3 by step</td><td>Array of 20 single</td><td>-</td></tr><tr><td>Exit_Cond</td><td>Exit condition</td><td>Array of 20 integer</td><td>0: Next Step
1: Next Technique
2: STOP Experiment</td></tr><tr><td>I_Range</td><td>I range</td><td>integer</td><td>see IRange constants authorized on section 5. Structures and Constants
Warning : I Auto-range is not allowed</td></tr></table>

Test configuration format: 

The test configuration is a 32-bits integer. 

<table><tr><td>31</td><td>...</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td></tr><tr><td colspan="3">Variable</td><td colspan="3">Sign</td><td>Logic</td><td>Active</td></tr><tr><td colspan="3">E (Voltage) = 0
AUX1 (Auxiliary Input 1) = 1
AUX2 (Auxiliary Input 2) = 2
I (Current) = 3</td><td colspan="3">“&lt;”= 0
“&gt;”= 1</td><td>OR = 0
AND = 1</td><td>active = 1
not active = 0</td></tr></table>

If Test3 is active, Test1 and Test2 must be active. The Test3 is ignored if Test2 or Test1 is inactive. The comparison formula is: (Test1 AND/OR Test2) AND/OR Test3. 

If Test2 is active, Test1 must be active. The Test2 is ignored if Test1 is inactive. The comparison formula is: Test1 AND/OR Test2. 

Test value format: 

The test value is a single-precision floating point value. It should be set regarding the test configuration: 

- the limit in V for the configuration with E; 

- the limit in V for the configuration with AUX1; 

- the limit in V for the configuration with AUX2; 

- the limit in A for the configuration with I. 

# 7.39.3. Data format

See GDYN technique data format. 

# 7.39.4. Data conversion

See GDYN technique data format. 

# 7.40. Modular Pulse technique


Technique ID: 167


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>mp.ecc</td><td>mp4.ecc 
mp5.ecc</td></tr><tr><td>Timebase</td><td>100μs</td><td>100μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/f4a0d5209fc3c16dff9c7a2f265a621f35ae1ff7fc1a66977da6ea6433002fbd.jpg)


# 7.40.1. Description

The Modular pulse technique (MOD) allows the user to control successively in different sequences the current and/or the voltage of the cell. With this technique including galvanostatic and potentiostatic sequences, the switch from one mode to the other is very fast. The recording conditions included in the sequence $( r _ { C } )$ offer the possibility to record only few sequences in a long time experiment. This technique is particularly useful for electrochemical coating. 

# 7.40.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">MOD parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Value_step</td><td>Voltage step (V) in Potentiostatic mode or Current step (A) in Galvanostatic mode</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs_initializer</td><td>Voltage / Current step vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Duration_step</td><td>Duration step (s)</td><td>Array of 20 single</td><td>≥ 2*tb</td></tr><tr><td>Record_every_dT</td><td>Record every dt (s)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td rowspan="2">Record_every_dM</td><td>Record every dI (A) in Potentiostatic</td><td rowspan="2">Array of 20 single</td><td rowspan="2">≥ 0</td></tr><tr><td>Record every dE (V) in Galvanostatic</td></tr><tr><td>Mode_step</td><td>Potentiostatic or Galvanostatic</td><td>Array of 20 integer</td><td>0 : Potentiostatic1 : Galvanostatic</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..19]</td></tr><tr><td>Record_Every_rc</td><td>Record every cycle</td><td>integer</td><td>≥ 0</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

# 7.40.3. Data format

Data format returned by the function BL_GetData : 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/77dfc3a8c8c3ac00fd1cd0c7d680fc440ed12e417cb3f27f506f78997716e58b.jpg)


The number of points saved in the buffer is returned in the field TDataInfos.NbRows. The number of variables defining a point is returned in the field TDataInfos.NbCols. 

# 7.40.4. Data conversion

Data returned into the buffer are not usable as-is, one must convert the data before: 

```javascript
time: The time should be converted using the function BL_ConvertTimeChannelNumericIntoSeconds 
```

```txt
- Float conversion: Ewe and I must be converted with the function BL_ConvertChannelNumericIntoSingle 
```

```txt
cycle: no conversion needed 
```

```txt
mode: no conversion needed. '0' is the potentio mode and '1' is the galvano mode. 
```

```yaml
step: no conversion needed 
```

# 7.41. Constant Amplitude Sinusoidal micro Galvano polarization technique


Technique ID: 169


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>casg.ecc</td><td>casg4.ecc 
casg5.ecc</td></tr><tr><td>Timebase</td><td>50μs</td><td>50μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/b6d5a93c21860a34dc7b1dc794760527161bf6d0f84c2af4c92613d116ef7c61.jpg)


# 7.41.1. Description

Constant Amplitude Sinusoidal micro Galvano polarization (CASG) is a technique similar than CASP. But in that case, the perturbation is performed around an initial current (Ii) with a small amplitude (Ia) and a constant low frequency (fs).Thanks to a Direct Fourier Transform the amplitudes of the fundamental frequency (fs), 1st (2 fs) and 2nd (3 fs) harmonics are determined. This technique can also be used for other applications such as battery, fuel cell, … 

# 7.41.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CASG parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ii</td><td>Initial current</td><td>single</td><td>-</td></tr><tr><td>Ii_vs初始</td><td>Initial current vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Fs</td><td>Frequency of applied sinusoidal</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>I1</td><td>High current of sinusoidal (A)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>I1_vs初始</td><td>Current I1 vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>I2</td><td>Low Current of sinusoidal (A)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>I2_vs初始</td><td>Current I2 vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>Period_number</td><td>Number of periods</td><td>Array of 20 integer</td><td>≥ 0</td></tr><tr><td>Record EVERY_dT</td><td>Record every dt (s)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Record EVERY_dE</td><td>Record every dE (V)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..19]</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

# 7.41.3. Data format

See CP technique data format. 

# 7.41.4. Data conversion

See CP technique data conversion. 

# 7.42. Constant Amplitude Sinusoidal micro Potentio polarization technique


Technique ID: 170


<table><tr><td>Instrument 
Series</td><td>VMP3</td><td>VMP-300</td></tr><tr><td>File</td><td>casp.ecc</td><td>casp4.ecc 
casp5.ecc</td></tr><tr><td>Timebase</td><td>50μs</td><td>50μs</td></tr></table>

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/beb28d6f6feea3148321e922ae9e7dec9c4556d99599e1c41398f2acbc2929bd.jpg)


# 7.42.1. Description

Constant Amplitude Sinusoidal micro Potentio polarization (CASP) is a technique used to determine the corrosion current and theTafel coefficients. In this technique, a sinusoidal voltage is applied around a potential (Ei) with a small amplitude (Va) and a constant low frequency (fs).Thanks to a Direct Fourier Transform, the amplitudes of the fundamental frequency (fs), 1st (2 fs) and 2nd (3 fs) harmonics are determined and used to calculate the corrosion current and the Tafel coefficients. This technique was designed to be faster than the usual linear polarization around the corrosion potential and, compared to the Tafel fit, does not require an adjustment of the Tafel parameters to have access to Icorr . 

# 7.42.2. Technique parameters

Technique parameters available for the function BL_LoadTechnique: 

<table><tr><td colspan="4">CASP parameters</td></tr><tr><td>Label</td><td>Description</td><td>Data types</td><td>Data range</td></tr><tr><td>Ei</td><td>Initial potential</td><td>single</td><td>-</td></tr><tr><td>Ei_vs初始</td><td>Initial potential vs initial one</td><td>boolean</td><td>True/False</td></tr><tr><td>Fs</td><td>Frequency of applied sinusoidal</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>E1</td><td>High Potential of sinusoidal (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>E1_vs初始</td><td>Voltage E1 vs initial one</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td>E2</td><td>Low Potential of sinusoidal (V)</td><td>Array of 20 single</td><td>-</td></tr><tr><td>vs初始</td><td>Voltage E2 vs initial</td><td>Array of 20 boolean</td><td>True/False</td></tr><tr><td></td><td>one</td><td></td><td></td></tr><tr><td>Period_number</td><td>Number of periods</td><td>Array of 20 integer</td><td>≥ 0</td></tr><tr><td>Record_every_dT</td><td>Record every dt (s)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Record_every_dI</td><td>Record every dI (A)</td><td>Array of 20 single</td><td>≥ 0</td></tr><tr><td>Step_number</td><td>Number of steps minus 1</td><td>integer</td><td>[0..19]</td></tr><tr><td>N_Cycles</td><td>Number of times the technique is repeated</td><td>integer</td><td>≥ 0</td></tr></table>

This parameters cannot be updated with BL_UpdateParameters function. 

# 7.42.3. Data format

See CP technique data format. 

# 7.42.4. Data conversion

See CP technique data conversion. 

# 8. Global parameters for hardware configuration

# 8.1 Electrode connection

The configuration parameters can be used with all techniques. The configuration parameters modification follows the same method as the technique parameters one. 

CE to ground Configuration : 

<table><tr><td colspan="4">Electrode connection parameters (only for VMP3 series)</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>ce</td><td>CE to ground</td><td>integer</td><td>CE to ground mode: ce = 1 and em = 3</td></tr><tr><td>em</td><td>Controlled potential mode</td><td>integer</td><td>Standard mode: ce = 0 and em = 0</td></tr></table>

# Note:

The functions BL_SetHardConf and BL_GetHardConf must be only used with VMP-300 series in order to change electrode connection mode. See 5. Structures and Constants and 6. Functions reference for more details. 

# 8.2 Instrument ground

This parameters is only used with VMP-300 series. The functions BL_SetHardConf and BL_GetHardConf must be used to modify or get the value of the parameter. See 6. Functions reference. 

# 8.3 Record and external control options

These options can be used only with VMP-300 series. 

<table><tr><td colspan="4">Record and external control options parameters (only for VMP-300 series)</td></tr><tr><td>Parameters</td><td>Description</td><td>Data type</td><td>Data range</td></tr><tr><td>Rcmp(Mod</td><td colspan="2">Solution ohmic drop mode of integer</td><td>0 = software based (via</td></tr></table>

# Record and external control options parameters (only for VMP-300 series)

<table><tr><td>e</td><td colspan="2">compensation. The VMP-300 has a hardware control of this compensation that can be enabled with this flag.</td><td colspan="2">the Rcmp_Value or Rcomp_Level parameter) 
1 = Hardware based (faster)</td></tr><tr><td>xctr</td><td>Bitfield controlling some extra options:</td><td>Integer / bitfield</td><td>Bit</td><td>Option</td></tr><tr><td></td><td rowspan="3">Activation of External control, or record of additional values: Ece, Analog IN1 &amp; 2, Ramp, Charge, Control and IRange values</td><td></td><td>1</td><td>Record Ece</td></tr><tr><td></td><td></td><td>2</td><td>Record Analog IN1</td></tr><tr><td></td><td></td><td>3</td><td>Record Analog IN2</td></tr><tr><td></td><td>Conversion to binary value to activate each options (see data range).</td><td></td><td>4</td><td>Enable External ctrl</td></tr><tr><td></td><td>1 : activated</td><td></td><td>5</td><td>Reserved</td></tr><tr><td></td><td>0 : not activated</td><td></td><td>6</td><td>Record Control</td></tr><tr><td></td><td></td><td></td><td>7</td><td>Record Charge</td></tr><tr><td></td><td></td><td></td><td>8</td><td>Record IRange</td></tr><tr><td></td><td></td><td></td><td colspan="2">All remaining bits are reserved.</td></tr><tr><td></td><td></td><td></td><td colspan="2">For example, activate External and record Charge. Value in:</td></tr><tr><td></td><td></td><td></td><td colspan="2">binary = 0b01001000</td></tr><tr><td></td><td></td><td></td><td colspan="2">integer = 72</td></tr><tr><td></td><td></td><td></td><td colspan="2">hex = 0x0048</td></tr><tr><td>R32</td><td>Ece ERange</td><td>integer</td><td colspan="2">see ERange constants authorized on section 5. Structures and Constants</td></tr><tr><td>raux1</td><td>Analog IN 1 ERange</td><td>integer</td><td colspan="2">see ERange constants authorized on section 5. Structures and Constants</td></tr><tr><td>raux2</td><td>Analog IN 2 ERange</td><td>integer</td><td colspan="2">see ERange constants authorized on section 5. Structures and Constants</td></tr></table>

# Note:

The external control option allows to control the potentiostat / galvanostat from an external signal source through the Analog IN 

2 input. For the potentio techniques the input voltage simply adds on the technique waveform. For the galvano techniques the input voltage is converted into a current. A simply rule of thumb is to consider 1V as the full scale of the selected current range. For more precise control one should divide the input voltage, (which should not exceed ± 1V) by the value of the shunt resistor. The input voltage has no effect if a technique is not running. 

# APPENDIX A. Find instruments

The EC-Lab® Development Package includes a library (blfind.dll) to find available instruments (Ethernet and USB). 

# Calling conventions

The library uses the stdcall calling conventions for all exported functions. 

# Multi-thread applications

All exported functions are protected by a synchronization object, they can be called in a multi-thread application. 

# Data types

See section 3.4. Data types 

# Functions reference

<table><tr><td>Function</td><td>BL_FINDECHEMDEV</td></tr><tr><td>Syntax</td><td>function BL_FindEChemDev(pLstdev: PChar; 
pzsize: point32; 
pNbrDev: point32): int32;</td></tr><tr><td rowspan="2">Parameters</td><td>pLstdev 
pointer to the buffer that will receive the serialization of 
instruments description (C-string format) 
(see section 5 Serializations format) 
psz 
pointer to a uint32 specifying the maximum number of 
characters of the buffer. It also returns the number of 
characters of the copied serialization.</td></tr><tr><td>pNbrDev 
pointer to a uint32 receiving the number of detected Ethernet 
and USB instruments.</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded 
&lt; 0 : see section 6 Error codes</td></tr><tr><td>Description</td><td>This function finds Ethernet and USB electrochemistry instruments 
and copies into the buffer a serialization of descriptions of detected 
instruments.</td></tr><tr><td>Delphi 
example</td><td>procedure FindEchemDevice; 
var</td></tr></table>

```vhdl
pSerializable: PChar;   
len, nbDev: uint32;   
Err: int32;   
begin   
len := 8192;   
pSerializable := StrAlloc(len);   
zeromemory(pSerializable, len);   
Err := BL_FindEChemDev( pSerializable, @len, @nbDev); ShowMessage('Instruments detected : ' + IntToStr(nbDev)); Str目的在于 (pSerializable);   
end;
```

<table><tr><td>Function</td><td>BL_FINDECHEMETHDEV</td></tr><tr><td>Syntax</td><td>function BL_FindEChemEthDev(pLstdev: PChar; 
psize: point32; 
pNbrDev: point32): int32;</td></tr><tr><td>Parameters</td><td>pLstdev 
pointer to the buffer that will receive the serialization of 
instruments description (C-string format) 
(see section 5 Serializations format) 
psize 
pointer to a uint32 specifying the maximum number of 
characters of the buffer. It also returns the number of 
characters of the copied serialization. 
pNbrDev 
pointer to a uint32 receiving the number of detected Ethernet 
instruments.</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded 
&lt; 0 : see section 6 Error codes</td></tr><tr><td>Description</td><td>This function finds Ethernet electrochemistry instruments and copies 
into the buffer a serialization of descriptions of detected instruments.</td></tr><tr><td>Delphi 
example</td><td>procedure FindEchemEthDevice; 
var 
pSerialization: PChar; 
len, nbDev: uint32; 
Err: int32; 
begin 
len := 4096; 
pSerialization := StrAlloc(len); 
zeromemory(pSerialization, len); 
Err := BL_FindEChemEthDev( pSerialization, @len, @nbDev); 
ShowMessage('Instruments detected: ' + IntToStr(nbDev)); 
Str目的在于找到所有可能的事件。</td></tr><tr><td></td><td>end;</td></tr><tr><td>Function</td><td>BL_FINDECHEMUSBDEV</td></tr><tr><td>Syntax</td><td>function BL_FindEChemUsbDev(pLstdev: PChar; 
  psize: uint32; 
  pNbrDev: uint32): int32;</td></tr><tr><td>Parameters</td><td>pLstdev 
  pointer to the buffer that will receive the serialization of 
  instruments description (C-string format) 
  (see section 5 Serializations format) 
  psize 
  pointer to a uint32 specifying the maximum number of 
  characters of the buffer. It also returns the number of 
  characters of the copied serialization. 
  pNbrDev 
  pointer to a uint32 receiving the number of detected 
  USB 
  instruments.</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded 
&lt; 0 : see section 6 Error codes</td></tr><tr><td>Description</td><td>This function finds USB electrochemistry instruments and copies into 
the buffer a serialization of descriptions of the detected instruments.</td></tr><tr><td>Delphi 
example</td><td>procedure FindEchemUsbDevice; 
var 
pSerialization: PChar; 
len, nbDev: uint32; 
Err: int32; 
begin 
len := 4096; 
pSerialization := StrAlloc(len); 
zeromemory(pSerialization, len); 
Err := BL_FindEChemUsbDev( pSerialization, @len, @nbDev); 
ShowMessage('Instruments detected: ' + IntToStr(nbDev)); 
Str目的在于IntToStr(nbDev)); 
end;</td></tr></table>

<table><tr><td>Function</td><td>BL_SETCONFIG</td></tr><tr><td>Syntax</td><td>function BL_SetConfig(pIP: PChar; pCfg: PChar): int32;</td></tr><tr><td>Parameters</td><td>pIP pointer to the buffer specifying the IP address of the instrument to configure. pCfg</td></tr><tr><td></td><td>pointer to the buffer specifying the new TCP/IP parameters of the instrument (see section 5 Serializations format)</td></tr><tr><td>Return value</td><td>= 0 : the function succeeded&lt; 0 : see section 6 Error codes</td></tr><tr><td>Description</td><td>This function sets new TCP/IP parameters of selected instrument. IP address, netmask and gateway may be modified.</td></tr><tr><td>Delphi example</td><td>procedure SetTCPIP;varIPaddress: array[0..15] of char;newCfg: array[0..57] of char;vErr : int32;beginIPaddress := '192.109.209.220' + #0;newCfg := 'IP%192.109.209.22\$' +‘NM%255.255.255.0\$’ +‘GW%192.109.209.170\$’ + #0;</td></tr><tr><td></td><td>vErr := BL_SetConfig(IPaddress, newCfg);end;</td></tr></table>

<table><tr><td>Procedure</td><td>BL_GETERRORMSG</td></tr><tr><td>Syntax</td><td>procedure BL_GetErrorMsg(errorcode: int32; pmsg: PChar; psize: uint32);</td></tr><tr><td>Parameters</td><td>errorcode
    error code selected
pmsg
    pointer to the buffer that will receive the text (C-string format)
psz
    pointer to a uint32 specifying the maximum number of characters of the buffer. It also returns the number of characters of the copied string.</td></tr><tr><td>Return value</td><td>None</td></tr><tr><td>Description</td><td>This function copies into the buffer the corresponding message of the selected error code.</td></tr><tr><td>Delphi</td><td>procedure DisplayErrorMessage;</td></tr></table>

# example

```txt
var   
msg: PChar;   
len: uint32;   
begin   
len := 255;   
msg := StrAlloc(len);   
zeromemory(msg, len);   
BL_GetErrorMsg(-20, msg, @len);   
ShowMessage(msg);   
Str目的在于 (msg);   
end;
```

# Serializations format

# 5.1. Instruments descriptions

This serialization consists of an array of characters containing the descriptions of detected instruments. If more than one instrument is detected, serialized descriptions are separated by the character $\%$ . 

An instrument description is defined by a set of 9 string descriptors. 

In the serialization, descriptors are separated by the character '$' and are always serialized in the same order : 

(1) Connection mode 

(2) IP address or USB plug index 

(3) Gateway (always empty with USB) 

(4) Netmask (always empty with USB) 

(5) MAC address (always empty with USB) 

(6) Identifier (always empty with USB) 

(7) Instrument type 

(8) Serial number 

(9) Name (always empty with USB) 

Example of instruments description serialization : 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/01e958edf2ac488520c0ad560c420c2924db898b07e3e2d73f70fd65a462ac94.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/eebc26d31f6bab1711eb1e45f3605c47ffb3c5db747db1d774eb30d97d212744.jpg)



Second instrument


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/bd50abffb66f765f3fb02de418c7229a75b8760330ab6d7e197fb1d56e096c43.jpg)


![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/e7538d2911e68bf81dfc78a6f605ee964f87cb9056e3178394f614da51ea24d2.jpg)


# 5.2. TCP/IP parameters

This serialization consists of an array of characters containing the definitions of TCP/IP parameters. If more than one parameter is defined, they must be separated by the character $\because$ . 

Each parameter must be defined by an identifier and a value, separated by the character $\%$ . 

Identifier must be one of the followings key words : 

IP : for IP address 

NM : for netmask 

GW : for gateway 

There is no order in the definition of TCP/IP parameters. 

Key words must be in upper case. 

Example of TCP/IP parameters serialization : 

![image](https://cdn-mineru.openxlab.org.cn/result/2026-03-13/31eff5d5-b59c-43cd-80a8-32f1b652534f/8f6bb9565e75440a34604c9754abe655d86bee6697520c9434ed033ecfa325ce.jpg)


# Error codes


General error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>BLFIND_ERR UNKNOWN</td><td>-1</td><td>unknown error</td></tr><tr><td>BLFIND_ERR_INVALID_PARAMETER</td><td>-2</td><td>invalid function parameters</td></tr></table>


Instrument error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>BLFIND_ERR_ACK_TIMEOUT</td><td>-10</td><td>instrument response timeout</td></tr><tr><td>BLFIND_ERR_EXP_RUNNING</td><td>-11</td><td>experiment is running on instrument</td></tr><tr><td>BLFIND_ERR_CMD_FAILED</td><td>-12</td><td>instrument do not execute command</td></tr></table>


Find error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>BLFIND_ERR_FIND_FAILED</td><td>-20</td><td>find failed</td></tr><tr><td>BLFIND_ERR_SOCKET_WRITE</td><td>-21</td><td>cannot write the request of the descriptions of Ethernet instruments</td></tr><tr><td>BLFIND_ERR_SOCKET_READ</td><td>-22</td><td>cannot read descriptions of Ethernet instrument</td></tr></table>


Modify error codes


<table><tr><td>Constant</td><td>Value</td><td>Description</td></tr><tr><td>BLFIND_ERR_CFG_MODIFY_FAILED</td><td>-30</td><td>set TCP/IP parameters failed</td></tr><tr><td>BLFIND_ERR_READ_PARAM_FAILED</td><td>-31</td><td>deserialization of TCP/IP parameters failed</td></tr><tr><td>BLFIND_ERR_empty_PARAM</td><td>-32</td><td>not any TCP/IP parameters in serialization</td></tr><tr><td>BLFIND_ERR_IP_format</td><td>-33</td><td>invalid format of IP address</td></tr><tr><td>BLFIND_ERR_NM_format</td><td>-34</td><td>invalid format of netmask address</td></tr><tr><td>BLFIND_ERR_GW_format</td><td>-35</td><td>invalid format of gateway address</td></tr><tr><td>BLFIND_ERR_IP_NOT_found</td><td>-38</td><td>instrument to modify not found</td></tr><tr><td>BLFIND_ERR_IP_ALREADYEXIST</td><td>-39</td><td>new IP address in TCP/IP parameters already exists</td></tr></table>

# APPENDIX B. Version Compatibility

By default, the user should use technique files, firmware binaries from the same EC-Lab® Development Package version. To verify if it is the case, the version compatibility is listed below. 

<table><tr><td colspan="5">Version Compatibility List for VMP-300 Series</td></tr><tr><td>EC-Lab®
Development
Package</td><td>DSP
Channel
Firmware</td><td>FPGA Channel
Firmware</td><td>ECLib.dll</td><td>BLFind.dll</td></tr><tr><td>V6.08</td><td>540 (V5.40)</td><td>43551 (0xAA1F)</td><td>V6.8.0.0</td><td>V1.18.0</td></tr><tr><td>V6.06</td><td>537 (V5.35)</td><td>43548 (0xAA1C)</td><td>V6.5.4.1</td><td>V1.18.0</td></tr><tr><td>V6.05</td><td>536 (V5.35)</td><td>43548 (0xAA1C)</td><td>V6.5.2.1</td><td>V1.16.0</td></tr><tr><td>V6.04</td><td>535 (V5.35)</td><td>43548 (0xAA1C)</td><td>V6.4.0.0</td><td>V1.16.0</td></tr><tr><td>V6.03</td><td>534 (V5.34)</td><td>43548 (0xAA1C)</td><td>V6.3.0.0</td><td>V1.15.0</td></tr><tr><td>V6.02</td><td>533 (V5.33)</td><td>43548 (0xAA1C)</td><td>V6.2.0.0</td><td>V1.14.0</td></tr><tr><td>V6.00</td><td>533 (V5.33)</td><td>43548 (0xAA1C)</td><td>V6.0.0.0</td><td>V1.7.0</td></tr><tr><td>V5.39</td><td>532 (V5.32)</td><td>43546 (0xAA1A)</td><td>V5.36.0.0</td><td>V1.7.0</td></tr><tr><td>V5.38</td><td>531 (V5.31)</td><td>43546 (0xAA1A)</td><td>V5.35.0.0</td><td>V1.7.0</td></tr><tr><td>V5.37</td><td>531 (V5.31)</td><td>43546 (0xAA1A)</td><td>V5.35.0.0</td><td>V1.7.0</td></tr><tr><td>...</td><td>...</td><td>...</td><td>...</td><td>...</td></tr><tr><td>V5.28</td><td>527 (V5.27)</td><td>43544 (0xAA18)</td><td>V5.28.0.0</td><td>V1.1.0</td></tr><tr><td>V5.27</td><td>527 (V5.27)</td><td>43544 (0xAA18)</td><td>V5.27.0.0</td><td>V1.1.0</td></tr></table>

<table><tr><td colspan="5">Version Compatibility List for VMP3 Series</td></tr><tr><td>EC-Lab®
Development
Package</td><td>DSP Channel
Firmware</td><td>FPGA Channel
Firmware</td><td>ECLib.dll</td><td>BLFind.dll</td></tr><tr><td>V6.08</td><td>5282 (V5.28.2)</td><td>42511 (0xA60F)</td><td>V6.8.0.0</td><td>V1.18.0</td></tr><tr><td>V6.06</td><td>5282 (V5.28.2)</td><td>42511 (0xA60F)</td><td>V6.5.4.1</td><td>V1.18.0</td></tr><tr><td>V6.05</td><td>5282 (V5.28.2)</td><td>42511 (0xA60F)</td><td>V6.5.2.1</td><td>V1.16.0</td></tr><tr><td>V6.04</td><td>5282 (V5.28.2)</td><td>42511 (0xA60F)</td><td>V6.4.0.0</td><td>V1.16.0</td></tr><tr><td>V6.03</td><td>5282 (V5.28.2)</td><td>42511 (0xA60F)</td><td>V6.3.0.0</td><td>V1.15.0</td></tr><tr><td>V6.02</td><td></td><td></td><td>V6.2.0.0</td><td>V1.14.0</td></tr><tr><td>V6.00</td><td></td><td></td><td>V6.0.0.0</td><td>V1.7.0</td></tr></table>


Version Compatibility List for VMP3 Series


<table><tr><td>EC-Lab®
Development
Package</td><td>DSP Channel
Firmware</td><td>FPGA Channel
Firmware</td><td>ECLib.dll</td><td>BLFind.dll</td></tr><tr><td>V5.39</td><td></td><td></td><td>V5.36.0.0</td><td>V1.7.0</td></tr><tr><td>V5.38</td><td></td><td></td><td>V5.35.0.0</td><td>V1.7.0</td></tr><tr><td>V5.37</td><td></td><td></td><td>V5.35.0.0</td><td>V1.7.0</td></tr><tr><td>...</td><td>...</td><td>...</td><td>...</td><td>...</td></tr><tr><td>V5.28</td><td></td><td></td><td>V5.28.0.0</td><td>V1.1.0</td></tr><tr><td>V5.27</td><td></td><td></td><td>V5.27.0.0</td><td>V1.1.0</td></tr></table>