using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabularEditor.TOMWrapper;
using System.Windows.Forms;


namespace MyPlugin2
{
    public class Class1 : ITabularEditorPlugin
    {
        Model model;
        TabularModelHandler Handler;
        public void Init(TabularModelHandler handler)
        {
            Handler = handler;

            model = handler.Model;
        }
        public void RegisterActions(Action<string, Action> registerCallback)
        {
            registerCallback("MyPlugin...", SayHello);
        }

        public void SayHello()
        {
            MessageBox.Show("Hello from  my plugin!\n\nCurrently the loaded model name");//:" + Handler.Model?.Name);

        }
    }
}
