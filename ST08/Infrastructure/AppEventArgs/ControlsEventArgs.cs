using System;
using System.Windows;


namespace Stavadlo22.Infrastructure.AppEventArgs
{
    /*
    /// <summary>
    /// objekt obsahuje nazov useku na ktory sa kliklo a jeho stav PATH_MODE
    /// </summary>
    public class SelectedPath : IVariantElement
    {
        public string Name { get; set; }
        public PATH_MODE Mode { get; set; }

        public SelectedPath(string name, PATH_MODE mode)
        {
            Name = name;
            Mode = mode;
        }
    }
    */
    /*
    /// <summary>
    /// objekt obsahuje switch Name, na ktoru sa kliklo a jej stav SWITCH_STATE
    /// </summary>
    public class SelectedSwitch : IVariantElement
    {
        public string Name { get; set; }
        public SC.SWITCH_STATE Mode { get; set; }

        public SelectedSwitch(string name, SC.SWITCH_STATE mode)
        {
            Name = name;
            Mode = mode;
        }
    }
    */
    /// <summary>
    /// bazovy Interface, aby sa odvodene instancie dali pretypovat 
    /// </summary>
    //public interface IVariantElement
    //{

    //}

    class PathEventArgs : RoutedEventArgs
    {
        public System.Windows.Shapes.Path ClickedPath { get; set; }
    }

    /// <summary>
    /// EventArgs class pre UC Semafor1Control
    /// </summary>
    public class SemaphoreEventArgs
    {
        public String Name { get; set; }
        public Object sender { get; set; }
    }

    /// <summary>
    /// EventArgs class pre UCrozvadzac
    /// </summary>
    public class RozvadzacEventArgs
    {
        public Object sender { get; set; }
    }

    //EventArgs class pre UCgate2
    public class GateEventArgs
    {
        public Object sender { get; set; }
    }

    /// <summary>
    /// EventArgs pre UC_VStupOdchod
    /// </summary>
    public class SuhlasEventArgs
    {
        public Object sender { get; set; }
    }
}
