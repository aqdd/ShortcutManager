namespace ShortcutManager.Helper;

public class CopyHelper
{
    public static TChild AutoCopy<TParent, TChild>(TParent parent) where TChild : TParent, new()
    {
        TChild child = new TChild();
        var ParentType = typeof(TParent);
        var Properties = ParentType.GetProperties();
        foreach (var Propertie in Properties)
        {
            //循环遍历属性
            if (Propertie.CanRead && Propertie.CanWrite)
            {
                //进行属性拷贝
                Propertie.SetValue(child, Propertie.GetValue(parent, null), null);
            }
        }

        return child;
    }
}