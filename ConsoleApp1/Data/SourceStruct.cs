namespace ConsoleApp1.Data;

public struct SourceStruct
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public SourceStruct(int id, string name)
    {
        Id = id;
        Name = name;
    }
    

}