
using System;
using System.Collections.Generic;

namespace DesignPatternsDemo
{

    public class ResourceManager
    {
        private static ResourceManager _instance;
        private static readonly object _lock = new();

        private Dictionary<string, object> _images = new();
        private Dictionary<string, object> _data = new();

        private ResourceManager() { }

        public static ResourceManager Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new ResourceManager();
                }
            }
        }

        public void LoadImage(string name, object image) => _images[name] = image;
        public object GetImage(string name) => _images.ContainsKey(name) ? _images[name] : null;

        public void StoreData(string key, object value) => _data[key] = value;
        public object GetData(string key) => _data.ContainsKey(key) ? _data[key] : null;
    }

    public interface ICurrencyRateProvider
    {
        double GetRate(string from, string to);
    }

    public class NbuApi
    {
        public double GetRateToUAH(string currency)
        {
            return currency switch
            {
                "USD" => 39.0,
                "EUR" => 42.0,
                _ => 1.0
            };
        }
    }

    public class NbuAdapter : ICurrencyRateProvider
    {
        private readonly NbuApi _api;
        public NbuAdapter(NbuApi api) => _api = api;

        public double GetRate(string from, string to)
        {
            if (from == "UAH") return 1.0 / _api.GetRateToUAH(to);
            if (to == "UAH") return _api.GetRateToUAH(from);

            double fromRate = _api.GetRateToUAH(from);
            double toRate = _api.GetRateToUAH(to);
            return fromRate / toRate;
        }
    }

    public interface IOrderStatusObserver
    {
        void OnStatusChanged(string orderId, string newStatus);
    }

    public class OrderStatusNotifier
    {
        private List<IOrderStatusObserver> _observers = new();

        public void AddObserver(IOrderStatusObserver observer) => _observers.Add(observer);
        public void RemoveObserver(IOrderStatusObserver observer) => _observers.Remove(observer);

        public void Notify(string orderId, string newStatus)
        {
            foreach (var observer in _observers)
            {
                observer.OnStatusChanged(orderId, newStatus);
            }
        }
    }

    public class UserNotifier : IOrderStatusObserver
    {
        private readonly string _userId;
        public UserNotifier(string userId) => _userId = userId;

        public void OnStatusChanged(string orderId, string newStatus)
        {
            Console.WriteLine($"User {_userId}: Order {orderId} status changed to '{newStatus}'");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Singleton Example ---");
            ResourceManager.Instance.LoadImage("logo", new byte[] { 1, 2, 3 });
            var image = ResourceManager.Instance.GetImage("logo");
            Console.WriteLine($"Image loaded: {(image != null ? "yes" : "no")}");

            Console.WriteLine("\n--- Adapter Example ---");
            ICurrencyRateProvider rateProvider = new NbuAdapter(new NbuApi());
            double rate = rateProvider.GetRate("USD", "EUR");
            Console.WriteLine($"USD to EUR rate: {rate}");

            Console.WriteLine("\n--- Observer Example ---");
            var notifier = new OrderStatusNotifier();
            var user1 = new UserNotifier("User123");
            var user2 = new UserNotifier("User456");

            notifier.AddObserver(user1);
            notifier.AddObserver(user2);

            notifier.Notify("ORD001", "Shipped");
            notifier.Notify("ORD001", "Delivered");
        }
    }
}
