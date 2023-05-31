namespace Z80Computer;

public class StatusFlags
{
    public byte Status { get; set; }

    public bool Error
    {
        set => Status = (byte)((Status | 1) & (value ? 1 : 0));
    }

    public bool DataRequestReady
    {
        set => Status = (byte)((Status | 1 << 3) & (value ? 1 << 3 : 0));
    }

    public bool WriteFault
    {
        set => Status = (byte)((Status | 1 << 5) & (value ? 1 << 5 : 0));
    }

    public bool Ready
    {
        set => Status = (byte)((Status | 1 << 6) & (value ? 1 << 6 : 0));
    }

    public bool Busy
    {
        set => Status = (byte)((Status | 1 << 7) & (value ? 1 << 7 : 0));
    }
}