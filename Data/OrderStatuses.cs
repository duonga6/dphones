namespace App.Data
{
    public static class OrderStatuses
    {
        public static string WaitAccept {get;} = "Chờ xác nhận";
        public static string Accepted {get;} = "Đã xác nhận";
        public static string Delivering {get;} = "Đang giao";
        public static string Delivered {get;} = "Đã giao";
        public static string Canceled {get;} = "Đã hủy";
        
    }

    public enum OrderStatusCode
    {
        WaitAccept,
        Accepted,
        Delivering,
        Delivered,
        Canceled
    }
}