/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bam.Net;
using Bam.Net.CommandLine;
using Bam.Net.Logging;
using Bam.Net.Incubation;
using Bam.Net.Configuration;
using System.IO;
using Bam.Net.Yaml;
using Bam.Net.Testing;
using System.Reflection;
using System.Threading.Tasks;
using Bam.Net.UserAccounts;
using Bam.Net.Data;
using Bam.Net.ServiceProxy;
using Bam.Net.Server;

namespace Bam.Net.Application
{
    [Serializable]
    class Program_bak : DeployableCommandLineTool
    {
        static void Main(string[] args)
        {
            TryWritePid(true);
            IsolateMethodCalls = false;
			Type type = typeof(Program_bak);
            AddValidArgument("apps", "Only serve the apps specified; a comma or semi-colon separated list");
            AddValidArgument("content", false, false, "The path to the content root directory");
            AddValidArgument("verbose", true, false, "Log 200 and 404 responses");
            AddValidArgument("ProcessMode", false, false, "Specify the process mode overriding what is found in the config file."); // this setting is automatically checked by ProcessMode.Current, adding the argument here as a reminder
			AddSwitches(type);
			DefaultMethod = type.GetMethod(nameof(Interactive));
            Initialize(args, (a) =>
            {
                Message.PrintLine("Error parsing arguments: {0}", ConsoleColor.Red, a.Message);
                Environment.Exit(1);
            });

            if (Arguments.Length > 0)
			{
				ExecuteSwitches(Arguments, type, null, null);
			}
			else
			{
				Interactive();
			}
        }
        
        static BamServer _server;
        static readonly object _serverLock = new object();
        public static BamServer Server
        {
            get
            {
                return _serverLock.DoubleCheckLock(ref _server, () => new BamServer(BamConf.Load(GetArgument("content", "Enter the path to the content root"))));
            }
        }

        [ConsoleAction("S", "Start Bamweb server")]
        public static void StartServer()
        {
            ConsoleLogger logger = InitLogger();
            Server.Subscribe(logger);
            ProcessArguments();
            Server.Start();
            LogConfig(logger); 
            Pause("BamWeb server started");
        }

        [ConsoleAction("K", "Stop (Kill) BamWeb server")]
        public static void StopServer()
        {
            Server.Stop();
			_server = null;
			Pause("BamWeb server stopped");
        }

        [ConsoleAction("R", "Restart BamWeb server")]
        public static void RestartServer()
        {
            Server.Stop();
            _server = null; // force reinitialization
            Server.Start();
            LogConfig(Log.Default as ConsoleLogger);
            Pause("BamWeb server re-started");
        }

        public static void LogResponses()
        {
            Server.ResponseFlushed += (s, res, req) =>
            {
                Task.Run(() =>
                {
                    StringBuilder messageFormat = new StringBuilder();
                    messageFormat.AppendLine("Responded: RequestId={0}, ClientIp={1}, Path={2}");
                    messageFormat.AppendLine("***");
                    messageFormat.AppendLine("{3}");
                    messageFormat.AppendLine("***");
                    Logger.Info(messageFormat.ToString(), req?.GetRequestId(), req?.GetClientIp() ?? "[null]", req?.Url?.ToString() ?? "[null]", req?.PropertiesToString());
                });
            };

            Server.NotResponded += (s, req) =>
            {
                Task.Run(() =>
                {
                    StringBuilder messageFormat = new StringBuilder();
                    messageFormat.AppendLine("DID NOT RESPOND: RequestId={0}, ClientIp={1}, Path={2}");
                    messageFormat.AppendLine("***");
                    messageFormat.AppendLine("{3}");
                    messageFormat.AppendLine("***");
                    Logger.Warning(messageFormat.ToString(), req?.GetRequestId(), req?.GetClientIp() ?? "[null]", req?.Url?.ToString() ?? "[null]", req?.PropertiesToString());
                });
            };
            
            Server.ServiceProxyResponder.Responded += (responder, context) =>
            {
                Task.Run(() =>
                {
                    IRequest req = context.Request;
                    StringBuilder messageFormat = new StringBuilder();
                    messageFormat.Append("ServiceProxy Responded: RequestId={0}, ClientIp={1}, Path={2}");
                    messageFormat.AppendLine("***");
                    messageFormat.AppendLine("{3}");
                    messageFormat.AppendLine("***");
                    Logger.Info(messageFormat.ToString(), req?.GetRequestId(), req?.GetClientIp() ?? "[null]", req?.Url.ToString() ?? "[null]", req?.PropertiesToString());
                });
            };
        }
        
        private static void LogConfig(ConsoleLogger logger)
        {
            BamConf conf = Server.GetCurrentConf();
            StringBuilder configurationMessage = new StringBuilder();
            configurationMessage.AppendLine();
            configurationMessage.AppendLine(new string('*', 40));
            foreach (AppConf appConfig in conf.AppsToServe)
            {
                configurationMessage.AppendLine(appConfig.Name);
                foreach (HostBinding hostPrefix in appConfig.Bindings)
                {
                    configurationMessage.AppendFormat("\t{0}\r\n", hostPrefix.ToString());
                }

                configurationMessage.AppendLine();
            }

            configurationMessage.AppendLine(new string('*', 40));
            logger.AddEntry(configurationMessage.ToString());
            Message.PrintLine("Server configured for Process Modes: {0}", ConsoleColor.DarkYellow, string.Join(',', conf.ProcessModes));
            PrintProcessMode();
        }

        private static void PrintProcessMode()
        {
            ProcessModes processMode = ProcessMode.Current.Mode;
            ConsoleColor color = ConsoleColor.Red;
            switch (processMode)
            {
                case ProcessModes.Dev:
                    color = ConsoleColor.Green;
                    break;
                case ProcessModes.Test:
                    color = ConsoleColor.Yellow;
                    break;
                case ProcessModes.Prod:
                    color = ConsoleColor.Red;
                    break;
            }

            Message.PrintLine("Current ProcessMode is {0}", color, processMode.ToString());
            Thread.Sleep(30);
        }

        private static void ProcessArguments()
        {
            if (Arguments.Contains("verbose"))
            {
                LogResponses();
            }

            if (Arguments.Contains("ProcessMode"))
            {
                string specifiedMode = Arguments["ProcessMode"];
                if (specifiedMode.TryToEnum(out ProcessModes mode))
                {
                    ProcessMode.Current.Mode = mode;
                }
            }
        }
    }
}
