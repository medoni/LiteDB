using System;
using System.Reflection;

namespace LiteDB.Tests
{
    public static class Extensions
    {
        private static Lazy<FieldInfo> _getDiskService_database = new Lazy<FieldInfo>(() => typeof(LiteDatabase).GetField("_engine", BindingFlags.Instance | BindingFlags.NonPublic));
        public static IDiskService GetDiskService(this LiteDatabase database) {
            var value = (LazyLoad<LiteEngine>)_getDiskService_database.Value.GetValue(database);
            return value.Value.GetDiskService();
        }

        private static Lazy<FieldInfo> _getDiskService_Engine = new Lazy<FieldInfo>(() => typeof(LiteEngine).GetField("_disk", BindingFlags.Instance | BindingFlags.NonPublic));
        public static IDiskService GetDiskService(this LiteEngine engine) {
            return (IDiskService)_getDiskService_Engine.Value.GetValue(engine);
        }
    }
}
