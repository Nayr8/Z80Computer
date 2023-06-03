namespace Z80Computer;

public class SimpleIdeStorage
{
    private readonly StatusFlags _status = new StatusFlags();
    
    private const byte
        readBit = 0,
        writeBit = 1,
        resetBit = 2;

    private IdeAddresses _address = 0;

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
        _address = (IdeAddresses)(address & 0b111);
    }

    public void SetReadWrite(byte readWrite)
    {
        readWrite &= 0b11;
        
    }
}