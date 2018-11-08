using Serilog;

namespace Imagini
{
    public static class Logger
    {
        public static Serilog.Core.Logger Log { get; set; }

        static Logger()
        {
#if DEBUG
            Set(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console());
#else
            Set(new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console());
#endif
        }

        /// <summary>
        /// Sets global log settings for Imagini.
        /// </summary>
        public static void Set(LoggerConfiguration config) =>
            Log = config.CreateLogger();
    }
}