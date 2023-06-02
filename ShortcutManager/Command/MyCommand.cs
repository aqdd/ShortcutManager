using System;
using System.Windows.Input;

namespace ShortcutManager.Command;

public class MyCommand: ICommand
{
    private Action executeAction;

    public MyCommand(Action action)
    {
        executeAction = action;
    }
    
    public bool CanExecute(object? parameter)
    {
        // throw new NotImplementedException();
        return true;
    }

    public void Execute(object? parameter)
    {
        executeAction();
    }

    public event EventHandler? CanExecuteChanged;
}