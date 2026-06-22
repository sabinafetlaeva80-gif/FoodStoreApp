using System.Windows;

namespace MMarketApp
{
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserRole { get; set; }
        public static string CurrentUserName { get; set; }
        public static int CurrentShiftId { get; set; }
    }
}