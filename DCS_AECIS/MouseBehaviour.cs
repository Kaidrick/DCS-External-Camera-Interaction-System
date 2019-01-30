using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DCS_AECIS
{
    public static class MouseCommandBehavior
    {
        #region TheCommandToRun

        /// <summary>
        /// The comamnd which should be executed when the mouse is down
        /// </summary>
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand",
                typeof(ICommand),
                typeof(MouseCommandBehavior),
                new FrameworkPropertyMetadata(null, (obj, e) => OnMouseCommandChanged(obj, (ICommand)e.NewValue, "MouseDown")));

        /// <summary>
        /// Gets the MouseDownCommand property
        /// </summary>
        public static ICommand GetMouseDownCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(MouseDownCommandProperty);
        }

        /// <summary>
        /// Sets the MouseDownCommand property
        /// </summary>
        public static void SetMouseDownCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(MouseDownCommandProperty, value);
        }

        /// <summary>
        /// The comamnd which should be executed when the mouse is up
        /// </summary>
        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand",
                typeof(ICommand),
                typeof(MouseCommandBehavior),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback((obj, e) => OnMouseCommandChanged(obj, (ICommand)e.NewValue, "MouseUp"))));

        /// <summary>
        /// Gets the MouseUpCommand property
        /// </summary>
        public static ICommand GetMouseUpCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(MouseUpCommandProperty);
        }

        /// <summary>
        /// Sets the MouseUpCommand property
        /// </summary>
        public static void SetMouseUpCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(MouseUpCommandProperty, value);
        }

        #endregion

        /// <summary>
        /// Registeres the event and calls the command when it gets fired
        /// </summary>
        private static void OnMouseCommandChanged(DependencyObject d, ICommand command, string routedEventName)
        {
            if (String.IsNullOrEmpty(routedEventName) || command == null) return;

            var element = (FrameworkElement)d;

            switch (routedEventName)
            {
                case "MouseDown":
                    element.PreviewMouseDown += (obj, e) => command.Execute(null);
                    break;
                case "MouseUp":
                    element.PreviewMouseUp += (obj, e) => command.Execute(null);
                    break;
            }
        }
    }

    //class MouseBehaviour
    //{
    //    public static readonly DependencyProperty MouseUpCommandProperty =
    //        DependencyProperty.RegisterAttached("MouseUpCommand", typeof(ICommand),
    //        typeof(MouseBehaviour), new FrameworkPropertyMetadata(
    //        new PropertyChangedCallback(MouseUpCommandChanged)));

    //    public static readonly DependencyProperty MouseDownCommandProperty =
    //        DependencyProperty.RegisterAttached("MouseDownCommand", typeof(ICommand),
    //        typeof(MouseBehaviour), new FrameworkPropertyMetadata(
    //        new PropertyChangedCallback(MouseDownCommandChanged)));


    //    private static void MouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        FrameworkElement element = (FrameworkElement)d;

    //        element.MouseUp += new MouseButtonEventHandler(element_MouseUp);
    //    }

    //    private static void MouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        FrameworkElement element = (FrameworkElement)d;

    //        element.MouseUp += new MouseButtonEventHandler(element_MouseDown);
    //    }

    //    static void element_MouseUp(object sender, MouseButtonEventArgs e)
    //    {
    //        FrameworkElement element = (FrameworkElement)sender;

    //        ICommand command = GetMouseUpCommand(element);

    //        command.Execute(e);
    //    }

    //    private static void element_MouseDown(object sender, MouseButtonEventArgs e)
    //    {
    //        FrameworkElement element = (FrameworkElement)sender;

    //        ICommand command = GetMouseDownCommand(element);

    //        command.Execute(e);
    //    }

    //    public static void SetMouseUpCommand(UIElement element, ICommand value)
    //    {
    //        element.SetValue(MouseUpCommandProperty, value);
    //    }

    //    public static void SetMouseDownCommand(UIElement element, ICommand value)
    //    {
    //        element.SetValue(MouseDownCommandProperty, value);
    //    }

    //    public static ICommand GetMouseUpCommand(UIElement element)
    //    {
    //        return (ICommand)element.GetValue(MouseUpCommandProperty);
    //    }

    //    private static ICommand GetMouseDownCommand(FrameworkElement element)
    //    {
    //        return (ICommand)element.GetValue(MouseDownCommandProperty);
    //    }
    //}
}
