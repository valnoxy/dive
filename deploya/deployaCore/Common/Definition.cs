﻿namespace deployaCore.Common
{
    public class Entities
    {
        public enum Firmware
        {
            BIOS,
            EFI,
        }

        public enum Bootloader
        {
            BOOTMGR,
            NTLDR,
        }

        public enum PartitionStyle
        {
            Single,
            SeparateBoot,
            Full
        }
    }
}
