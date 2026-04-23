using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ZabgcExamsDesktop.MVVM.View.Pages
{
    public static class DataGridColumnsBehavior
    {
        public enum ViewType
        {
            Departments,
            Groups,
            Audiences,
            Teachers,
            Disciplines,
            Managers,
            DepartmentOwners
        }
        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.RegisterAttached(
                "ViewType",
                typeof(ViewType),
                typeof(DataGridColumnsBehavior),
                new PropertyMetadata(OnViewTypeChanged));

        public static void SetViewType(DependencyObject element, ViewType value)
            => element.SetValue(ViewTypeProperty, value);

        public static ViewType GetViewType(DependencyObject element)
            => (ViewType)element.GetValue(ViewTypeProperty);

        private static void OnViewTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;

            grid.Columns.Clear();

            var viewType = (ViewType)e.NewValue;

            switch (viewType)
            {
                case ViewType.Departments:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Отделение",
                        Binding = new Binding("NameOfDepartment")
                    });
                    break;

                case ViewType.Groups:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Группа",
                        Binding = new Binding("NameOfGroup")
                    });
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Отделение",
                        Binding = new Binding("DepartmentName")
                    });
                    AddDeleteColumn(grid);
                    break;

                case ViewType.Audiences:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Аудитория",
                        Binding = new Binding("NumberAudience")
                    });
                    AddDeleteColumn(grid);
                    break;

                case ViewType.Teachers:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Преподаватель",
                        Binding = new Binding("FullName")
                    });
                    AddDeleteColumn(grid);
                    break;

                case ViewType.Disciplines:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Дисциплина",
                        Binding = new Binding("NameDiscipline")
                    });
                    AddDeleteColumn(grid);
                    break;

                case ViewType.Managers:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Должность",
                        Binding = new Binding("Post")
                    });
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "ФИО",
                        Binding = new Binding("FullName")
                    });
                    break;

                case ViewType.DepartmentOwners:
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Отделение",
                        Binding = new Binding("DepartmentName")
                    });
                    grid.Columns.Add(new DataGridTextColumn
                    {
                        Header = "Заведующий",
                        Binding = new Binding("OwnerName")
                    });
                    break;
            }
        }

        private static void AddDeleteColumn(DataGrid grid)
        {
            var template = new DataTemplate();

            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetResourceReference(FrameworkElement.StyleProperty, "DeleteButtonStyle");

            template.VisualTree = factory;

            grid.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Действия",
                CellTemplate = template
            });
        }
    }
}
