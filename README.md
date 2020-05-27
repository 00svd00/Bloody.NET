# Bloody.NET
A dotnet library to control Bloody B930 keyboard, forked from Vulcan.NET

RGB Packet Structure:
- All packets sent to the keyboard which involve anything to do with rgb are 64 bytes long.
- First Packet of rgb data transfer starts with 07030601 and next bytes are empty.
- Next Packets contain actual rgb data and start with 070306xx where xx is 07->0C hex range, basically giving packets order where 07 packet is first and 0C packet is last.
- 07, 08 packets send Red data; 09,0A packets send Green data; 0B,0C send Blue data.
- The 07,09,0B packets contain colors for first half of keyboard, and 08,0A,0C cointain data for second half of keyboard. 
- First half are keys in Key.cs with number less than 57 and vice versa. As each packet contains data for 58 keys.
- First 4 bytes in each packets are used for header and next 2 packets are always empty, so the first key to appear at this offset in first packet is labled as 0 (ESC=0)

This Library has some comments to explain what each function does
