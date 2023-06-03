namespace ShortcutManager.Model;

public class Data
{
    public uint Id { get; set; }
    public bool IsMyComputer { get; set; }
    public string Name { get; set; }
    public string RealPath { get; set; }
    public string ShortcutPath { get; set; }
    public string Arguments { get; set; }
    public string[] Verbs { get; set; }

    public string[] Categories { get; set; }
    public uint UpdateTimestamp { get; set; }
    public int Sort { get; set; }
}