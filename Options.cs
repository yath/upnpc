using OpenSource.UPnP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace upnpc {
    class Options {
        /// <summary>
        /// Thrown when a usage error (e.g. unknown parameter) occurs and
        /// a help message should be shown to the user.
        /// </summary>
        public class UsageException : Exception {
            public UsageException(string message) : base(message) { }
        };

        /// <summary>
        ///  Whether to print verbose discovery messages.
        /// </summary>
        public readonly bool VerboseDiscovery = false;

        /// <summary>
        ///  Discovery timeout in milliseconds.
        /// </summary>
        public readonly int DisoveryTimeoutMs = 30000;

        /// <summary>
        ///  Timeout for rediscovery in milliseconds.
        /// </summary>
        public readonly int RediscoveryMs     = 1000;


        /// <summary>
        /// Unique Device Name of the device to talk to.
        /// </summary>
        public readonly string DeviceUDN          = null;    // 10b07600-0018-1000-9907-4844f7fa4f83

        /// <summary>
        /// URN of the device to talk to.
        /// </summary>
        public readonly string DeviceURN          = null;   // urn:samsung.com:device:MainTVServer2:1

        /// <summary>
        ///  Friendly name of the device to talk to.
        /// </summary>
        public readonly string DeviceFriendlyName = null; // [TV]UE40ES5700


        /// <summary>
        ///  ID of the service to talk to.
        /// </summary>
        public readonly string ServiceID  = null;   // urn:samsung.com:serviceId:MainTVAgent2

        /// <summary>
        ///  URN of the service to talk to.
        /// </summary>
        public readonly string ServiceURN = null;  // urn:samsung.com:service:MainTVAgent2:1


        /// <summary>
        /// Variables and their contents to be set before invoking the action.
        /// </summary>
        public readonly Dictionary<string, string> SetVars    = new Dictionary<string, string>();

        /// <summary>
        ///  Variables to be printed to Console after invoking the action.
        /// </summary>
        public readonly List<string>               GetVars    = new List<string>();

        /// <summary>
        /// A map of variable names and their expected content after invoking the action.
        /// </summary>
        public readonly Dictionary<string, string> ExpectVars = new Dictionary<string, string>();

        /// <summary>
        ///  The action to call.
        /// </summary>
        public readonly string Action = null;

        /// <summary>
        ///  Whether to only dump variables and exit.
        /// </summary>
        public readonly bool DumpVars = false;
        

        /// <summary>
        ///  Verifies this Option instance's validity.
        /// </summary>
        public void Verify() {
            if (ServiceID == null && ServiceURN == null)
                throw new UsageException("One of service ID (/si:) or URN (/su:) must be specified.");

             if (Action == null)
                throw new UsageException("Action (/a:) is mandatory.");
        }

        /// <summary>
        ///  Checks whether a UPnPDevice matches the filters of this Option.
        /// </summary>
        /// <param name="device">The UPnPDevice to check.</param>
        /// <returns>Whether all filters match.</returns>
        public bool Matches(UPnPDevice device) {
            if (DeviceFriendlyName != null && !DeviceFriendlyName.Equals(device.FriendlyName))
                return false;

            if (DeviceUDN != null && !DeviceUDN.Equals(device.UniqueDeviceName))
                return false;

            if (DeviceURN != null && !DeviceURN.Equals(device.DeviceURN))
                return false;

            return true;
        }
        /// <summary>
        ///  Checks whether a UPnPService matches the filters of this Option.
        /// </summary>
        /// <param name="service">The UPnPService to check.</param>
        /// <returns>Whether all filters match.</returns>
        public bool Matches(UPnPService service) {
            if (ServiceID != null && !ServiceID.Equals(service.ServiceID))
                return false;

            if (ServiceURN != null && !ServiceURN.Equals(service.ServiceURN))
                return false;

            return true;
        }

        /// <summary>
        ///  Returns an invocation help.
        /// </summary>
        /// <returns>The help string.</returns>
        public static string Help() {
            Options dft = new Options(); // default options

            return string.Format(
                "UPnPClient Arguments:\n" +
                "\t/t:{0}\tDiscovery timeout in ms\n" +
                "\t/r:{1}\tRediscovery every ms\n" +
                "\t/vd\tVerbose discovery\n" +
                "\n" +
                "\t/du:{2}\tDevice URN\n" +
                "\t/di:{3}\tDevice UDN (ID)\n" +
                "\t/df:{4}\tDevice friendly name\n" +
                "\t/su:{5}\tService URN\n" +
                "\t/si:{6}\tService ID\n" +
                "\n" +
                "\t/a:action\tCall action 'action'\n" +
                "\t/sv:var=value\tSet variable 'var' to 'value'\n" +
                "\t/gv:var\tPrint (Get) 'var's value before exiting\n" +
                "\t/dv\tDump all variables instead of calling action\n" +
                "\t/ev:var=value\tAfter call, expect 'var' to be 'value\n",
                dft.DisoveryTimeoutMs, // 0
                dft.RediscoveryMs, // 1
                dft.DeviceURN ?? "urn:...", // 2
                dft.DeviceUDN ?? "10b07600-00...", // 3
                dft.DeviceFriendlyName ?? "ABCD...", // 4
                dft.ServiceURN ?? "urn:...", // 5
                dft.ServiceID ?? "urn:...:1"); // 6
        }

        /// <summary>
        ///  Constructs an Option with command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public Options(string[] args) {
            foreach (string arg in args) {
                if (arg.StartsWith("/t:"))
                    DisoveryTimeoutMs = int.Parse(arg.Substring(3));
                else if (arg.StartsWith("/r:"))
                    RediscoveryMs = int.Parse(arg.Substring(3));
                else if (arg.Equals("/vd"))
                    VerboseDiscovery = true;
                else if (arg.StartsWith("/du:"))
                    DeviceURN = arg.Substring(4);
                else if (arg.StartsWith("/di:"))
                    DeviceUDN = arg.Substring(4);
                else if (arg.StartsWith("/df:"))
                    DeviceFriendlyName = arg.Substring(4);
                else if (arg.StartsWith("/su:"))
                    ServiceURN = arg.Substring(4);
                else if (arg.StartsWith("/si:"))
                    ServiceID = arg.Substring(4);
                else if (arg.StartsWith("/sv:")) {
                    string[] kv = arg.Substring(4).Split(new char[] { '=' }, 2);
                    if (kv.Count() != 2)
                        throw new UsageException("Usage: /sv:name=value");
                    string name = kv[0], value = kv[1];
                    if (SetVars.ContainsKey(name))
                        throw new UsageException("/sv: attempted to set \"" + name + "\" twice.");
                    SetVars[name] = value;
                } else if (arg.StartsWith("/gv:"))
                    GetVars.Add(arg.Substring(4));
                else if (arg.StartsWith("/a:"))
                    Action = arg.Substring(3);
                else if (arg.Equals("/dv"))
                    DumpVars = true;
                else if (arg.StartsWith("/ev:")) {
                    string[] kv = arg.Substring(4).Split(new char[] { '=' }, 2);
                    if (kv.Count() != 2)
                        throw new UsageException("Usage: /ev:name=value");
                    string name = kv[0], value = kv[1];
                    if (ExpectVars.ContainsKey(name))
                        throw new UsageException("/ev: attempted to expect \"" + name + "\" twice.");
                    ExpectVars[name] = value;
                } else
                    throw new UsageException("Illegal argument \"" + arg + "\". (Missing value?)");
            }
            Verify();
        }

        /// <summary>
        ///  Constructs Options with default values.
        /// </summary>
        public Options() { }
    }
}
