namespace Experimental1.Data;
    class Foo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Bar Bar { get; set; } = new Bar();
    public Foo(int id, string name)
    {
        Id = id;
        Name = name;
    }
    public Foo()
    {
        Id = 0;
        Name = string.Empty;
    }

}
