namespace ConsoleApp1.Data;

public struct DestinationStruct
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public DestinationStruct(int id, string name)
    {
        Id = id;
        Name = name;
    }
    

}