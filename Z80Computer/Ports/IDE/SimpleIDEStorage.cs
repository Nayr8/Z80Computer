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

public class SimpleIdeStorage
{
    private readonly StatusFlags _status = new StatusFlags();
    
    private const byte
        readBit = 0,
        writeBit = 1,
        resetBit = 2;

    public SimpleIdeStorage()
    {
        _status.Ready = true;
    }

    public byte GetStatus()
    {
        return _status.Status;
    }
    

    public void WriteHigherData(byte value)
    {
        
    }

    public void WriteLowerData(byte value)
    {
        
    }

    public byte ReadHigherData()
    {
        return 0;
    }

    public byte ReadLowerData()
    {
        return 0;
    }

    public void SetAddress(byte address)
    {
        
    }

    public void SetReadWrite(byte readWrite)
    {
        readWrite &= 0b11;
        
    }
}