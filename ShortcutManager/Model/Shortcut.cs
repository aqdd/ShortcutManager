using System;

namespace ShortcutManager.Model;

public class Shortcut
{
    private string _Name;
    private string _IconLocation;
    private string _Path;
    private string _Arguments;
    private string _WorkingDirectory;
    private string _Desc;

    public string Name
    {
        get => _Name;
        set
        {
            var s = value;
            var i = s.LastIndexOf(".lnk");
            if (i!=-1)
            {
                s= s.Remove(i, 4);
            }
            _Name = s ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public string Desc
    {
        get => _Desc;
        set => _Desc = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string IconLocation
    {
        get => _IconLocation;
        set => _IconLocation = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Path
    {
        get => _Path;
        set => _Path = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Arguments
    {
        get => _Arguments;
        set => _Arguments = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string WorkingDirectory
    {
        get => _WorkingDirectory;
        set => _WorkingDirectory = value ?? throw new ArgumentNullException(nameof(value));
    }
}