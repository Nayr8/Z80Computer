namespace Z80Computer;

public enum IdeAddresses : byte
{
    DataPort = 0b000,
    ReadErrorCode = 0b001,
    NumberOfSectorsToTransfer = 0b010,
    SectorAddressLba0 = 0b011,
    SectorAddressLba1 = 0b100,
    SectorAddressLba2 = 0b101,
    SectorAddressLba3 = 0b110,
    ReadStatusWriteIssueCommand = 0b111,
}