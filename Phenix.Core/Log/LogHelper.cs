using System;
using System.Linq;
using Serilog;
using Serilog.Events;

namespace Phenix.Core.Log
{
    /// <summary>
    /// Serilog����
    /// ��Database.Default��PH7_EventLog_X��(������XΪ1~4)�洢��־
    /// </summary>
    public static class LogHelper
    {
        #region ����

        #region ������

        private static long? _fileSizeLimitBytes;

        /// <summary>
        /// ��־�ļ���С����
        /// Ĭ�ϣ�1G(>=1024)
        /// </summary>
        public static long FileSizeLimitBytes
        {
            get { return AppSettings.GetLocalProperty(ref _fileSizeLimitBytes, 1L * 1024 * 1024 * 1024); }
            set { AppSettings.SetLocalProperty(ref _fileSizeLimitBytes, value); }
        }

        private static int? _retainedFileCountLimit;

        /// <summary>
        /// ��־�ļ���������
        /// Ĭ�ϣ�90(>=1)
        /// </summary>
        public static int RetainedFileCountLimit
        {
            get { return AppSettings.GetLocalProperty(ref _retainedFileCountLimit, 90); }
            set { AppSettings.SetLocalProperty(ref _retainedFileCountLimit, value); }
        }

        private static bool? _buffered;

        /// <summary>
        /// ��־�ļ�����ˢ��
        /// Ĭ�ϣ�true
        /// </summary>
        public static bool Buffered
        {
            get { return AppSettings.GetLocalProperty(ref _buffered, true); }
            set { AppSettings.SetLocalProperty(ref _buffered, value); }
        }

        private static int? _flushToDiskIntervalSeconds;

        /// <summary>
        /// ��־�ļ�ˢ������(��)
        /// Ĭ�ϣ�3(>=1)
        /// </summary>
        public static int FlushToDiskIntervalSeconds
        {
            get { return AppSettings.GetProperty(ref _flushToDiskIntervalSeconds, 3); }
            set { AppSettings.SetProperty(ref _flushToDiskIntervalSeconds, value); }
        }

        private static string _outputTemplate;

        /// <summary>
        /// ��־ģ��
        /// Ĭ�ϣ�"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{Identity:lj}] {Message:lj}{NewLine}{Exception}"
        /// {Level:u3}��ʾ����־�ȼ��������ʾΪ3����д�ַ�����DBG��INF��ERR�ȡ�{Level:w3}��ʾ�����ַ���Сд
        /// {Message:lj}��ʾ����Ϣ���л���json�ַ�����string���ͳ��⣨j��ʾjson��l��ʾexcept for string literals��
        /// {Identity:lj}��չ�������Ϣ, ��ע��LogEventDynamicallyProperties
        /// </summary>
        public static string OutputTemplate
        {
            get { return AppSettings.GetLocalProperty(ref _outputTemplate, "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{Identity:lj}] {Message:lj}{NewLine}{Exception}"); }
            set { AppSettings.SetLocalProperty(ref _outputTemplate, value); }
        }

        #endregion

        #endregion

        #region ����

        /// <summary>
        /// ��ʼ������
        /// </summary>
        public static LoggerConfiguration InitializeConfiguration()
        {
            //��־����Fatal(��������) > Error(һ�����) > Warning(����) > Information(һ����Ϣ) > Debug(������Ϣ) > Verbose(��ϸģʽ����ȫ��)
            LoggerConfiguration result = AppRun.Debugging
                ? new LoggerConfiguration()
                    .MinimumLevel.Debug() //�������С��־����
                    .WriteTo.Console(LogEventLevel.Debug, OutputTemplate) //��־��ӡ������̨
                    .WriteTo.File($"{AppRun.BaseDirectory}Logs/log-.txt", //��־��ӡ���ļ���
                        restrictedToMinimumLevel: LogEventLevel.Debug, //�������С��־����
                        rollingInterval: RollingInterval.Day, //ÿ��һ���ļ�, �ļ�����ʽ: log-yyyyMMdd.txt
                        shared: false, //�����������̹�����־�ļ�
                        rollOnFileSizeLimit: true, //���ﵽ�ļ���С����ʱ�½�һ���ļ�, �ļ���׷��һ������
                        fileSizeLimitBytes: FileSizeLimitBytes >= 1024 ? FileSizeLimitBytes : null,
                        retainedFileCountLimit: RetainedFileCountLimit >= 1 ? RetainedFileCountLimit : null,
                        buffered: Buffered,
                        flushToDiskInterval: FlushToDiskIntervalSeconds >= 1 ? new TimeSpan(0, 0, FlushToDiskIntervalSeconds) : null,
                        outputTemplate: OutputTemplate)
                : new LoggerConfiguration()
                    .MinimumLevel.Information() //�������С��־����
                    .WriteTo.Console(LogEventLevel.Warning, OutputTemplate); //��־��ӡ������̨
            result
                .Enrich.With<LogEventDynamicProperties>()
                .MinimumLevel.Override("Default", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext() //������־������
                .WriteTo.Logger(configuration => configuration.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.File($"{AppRun.BaseDirectory}Logs/debugs/log-.txt",
                        rollingInterval: RollingInterval.Day, //ÿ��һ���ļ�, �ļ�����ʽ: log-yyyyMMdd.txt
                        shared: false, //�����������̹�����־�ļ�
                        rollOnFileSizeLimit: true, //���ﵽ�ļ���С����ʱ�½�һ���ļ�, �ļ���׷��һ������
                        fileSizeLimitBytes: FileSizeLimitBytes >= 1024 ? FileSizeLimitBytes : null,
                        retainedFileCountLimit: RetainedFileCountLimit >= 1 ? RetainedFileCountLimit : null,
                        buffered: Buffered,
                        flushToDiskInterval: FlushToDiskIntervalSeconds >= 1 ? new TimeSpan(0, 0, FlushToDiskIntervalSeconds) : null,
                        outputTemplate: OutputTemplate))
                .WriteTo.Logger(configuration => configuration.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File($"{AppRun.BaseDirectory}Logs/warnings/log-.txt",
                        rollingInterval: RollingInterval.Day, //ÿ��һ���ļ�, �ļ�����ʽ: log-yyyyMMdd.txt
                        shared: false, //�����������̹�����־�ļ�
                        rollOnFileSizeLimit: true, //���ﵽ�ļ���С����ʱ�½�һ���ļ�, �ļ���׷��һ������
                        fileSizeLimitBytes: FileSizeLimitBytes >= 1024 ? FileSizeLimitBytes : null,
                        retainedFileCountLimit: RetainedFileCountLimit >= 1 ? RetainedFileCountLimit : null,
                        buffered: Buffered,
                        flushToDiskInterval: FlushToDiskIntervalSeconds >= 1 ? new TimeSpan(0, 0, FlushToDiskIntervalSeconds) : null,
                        outputTemplate: OutputTemplate))
                .WriteTo.Logger(configuration => configuration.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File($"{AppRun.BaseDirectory}Logs/errors/log-.txt",
                        rollingInterval: RollingInterval.Day, //ÿ��һ���ļ�, �ļ�����ʽ: log-yyyyMMdd.txt
                        shared: false, //�����������̹�����־�ļ�
                        rollOnFileSizeLimit: true, //���ﵽ�ļ���С����ʱ�½�һ���ļ�, �ļ���׷��һ������
                        fileSizeLimitBytes: FileSizeLimitBytes >= 1024 ? FileSizeLimitBytes : null,
                        retainedFileCountLimit: RetainedFileCountLimit >= 1 ? RetainedFileCountLimit : null,
                        buffered: Buffered,
                        flushToDiskInterval: FlushToDiskIntervalSeconds >= 1 ? new TimeSpan(0, 0, FlushToDiskIntervalSeconds) : null,
                        outputTemplate: OutputTemplate))
                .WriteTo.Logger(configuration => configuration.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                    .WriteTo.File($"{AppRun.BaseDirectory}Logs/fatal-errors/log-.txt",
                        rollingInterval: RollingInterval.Day, //ÿ��һ���ļ�, �ļ�����ʽ: log-yyyyMMdd.txt
                        shared: false, //�����������̹�����־�ļ�
                        rollOnFileSizeLimit: true, //���ﵽ�ļ���С����ʱ�½�һ���ļ�, �ļ���׷��һ������
                        fileSizeLimitBytes: FileSizeLimitBytes >= 1024 ? FileSizeLimitBytes : null,
                        retainedFileCountLimit: RetainedFileCountLimit >= 1 ? RetainedFileCountLimit : null,
                        buffered: false,
                        flushToDiskInterval: null,
                        outputTemplate: OutputTemplate));
            return result;
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        public static void Verbose(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        public static void Verbose<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        public static void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        public static void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        public static void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Verbose(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        public static void Verbose(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        public static void Verbose<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        public static void Verbose<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        public static void Verbose<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        public static void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Verbose(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        public static void Debug(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Debug, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        public static void Debug<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Debug, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        public static void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        public static void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        public static void Debug(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Debug(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        public static void Debug(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Debug, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        public static void Debug<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        public static void Debug<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        public static void Debug<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        public static void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Debug(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Information, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Information, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Information(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Information, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Information, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        public static void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Information(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Warning, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Warning, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Warning(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Warning, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        public static void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Warning(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Error, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Error, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Error(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Error, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Error, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Error(exception, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        public static void Fatal(string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        public static void Fatal<T>(string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        public static void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        public static void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
        /// </summary>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        public static void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Fatal(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        public static void Fatal(Exception exception, string messageTemplate)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, exception, messageTemplate);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        public static void Fatal<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        public static void Fatal<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
        /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        public static void Fatal<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Serilog.Log.Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="messageTemplate">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        public static void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger.Fatal(exception, messageTemplate, propertyValues);
        }

        #endregion
    }
}