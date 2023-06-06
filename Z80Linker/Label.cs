namespace Z80Linker;

public class Label
{
    public string Name { get; }
    public int Location { get; }
    
    public Label(string name, int location)
    {
        Name = name;
        Location = location;
    }

}