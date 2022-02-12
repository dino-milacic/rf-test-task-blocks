using System.Collections.Generic;
using System.Linq;

namespace RFTestTaskBlocks
{
    public static class Services
    {
        private static readonly HashSet<IGameService> _registeredServices;

        static Services()
        {
            _registeredServices = new HashSet<IGameService>();
        }

        public static void Register<T>(T service) where T : class, IGameService
        {
            _registeredServices.Add(service);
        }

        public static void Unregister<T>(T service) where T : class, IGameService
        {
            _registeredServices.Remove(service);
        }

        public static T Get<T>() where T : class, IGameService
        {
            IGameService service = _registeredServices.SingleOrDefault(s => s is T);
            return service as T;
        }
    }

    public interface IGameService
    {
    }
}