namespace Z80Assembler.old;

public class StringBuffer
{
    private int _size;
    public int Size
    {
        get => _size;
    }

    private int _count;
    public int Count
    {
        get => _count;
    }

    private char[] _buffer;

    public StringBuffer(int size)
    {
        _size = size;
        _buffer = new char[size];
    }

    public bool Add(char value)
    {
        if (_count == _size)
        {
            return false;
        }
        _buffer[_count] = value;
        ++_count;
        return true;
    }

    public void Clear()
    {
        _count = 0;
    }

    public override string ToString()
    {
        return new string(_buffer.Take(_count).ToArray());
    }
}