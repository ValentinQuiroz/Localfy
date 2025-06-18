using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Localfy.Behaviors
{
    public class ListViewItemDoubleClickBehavior : Behavior<ListView>
    {
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListViewItemDoubleClickBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView)
            {
                // Verificamos si se hizo doble clic sobre un ítem
                var clickedItem = GetClickedItem(listView, e);
                if (clickedItem != null && Command?.CanExecute(clickedItem) == true)
                {
                    Command.Execute(clickedItem);
                }
            }
        }

        private object? GetClickedItem(ListView listView, MouseButtonEventArgs e)
        {
            var clickedPoint = e.GetPosition(listView);
            var element = listView.InputHitTest(clickedPoint) as DependencyObject;

            while (element != null && !(element is ListViewItem))
                element = VisualTreeHelper.GetParent(element);

            return (element as ListViewItem)?.DataContext;
        }
    }
}
