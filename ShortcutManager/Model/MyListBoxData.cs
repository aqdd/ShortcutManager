using System.Windows.Media;

namespace ShortcutManager.Model;

public class MyListBoxData
{
    public ImageSource Src { get; set; }
    public string Name { get; set; }
    public string RealPath { get; set; }
    public string ShortcutPath { get; set; }
    public string Arguments { get; set; }
    public string[] Verbs { get; set; }
}
