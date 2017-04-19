using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PluginDemo
{
    public interface IPlugin
    {
        int GetInt();

        string GetString();
        
        object GetNonMarshalByRefObject();

        Action GetAction();

        List<string> GetList();
    }
}
