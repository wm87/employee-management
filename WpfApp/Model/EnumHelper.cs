namespace WpfApp.Model
{
    public static class EnumHelper
    {
        public static Array GenderValues => Enum.GetValues(typeof(Gender));
        public static Array DepartmentValues => Enum.GetValues(typeof(Department));
    }
}
