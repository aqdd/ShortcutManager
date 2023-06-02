using System.Windows;

namespace ShortcutManager.Core;

public interface IOperator
{
    public void FileDragOver(object sender, DragEventArgs e);
    public void FileDragLeave(object sender, DragEventArgs e);
    public void SaveShortcut(object sender, DragEventArgs e);
}