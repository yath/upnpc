using OpenSource.UPnP;
using System;
using System.Diagnostics;
using System.Threading;

namespace upnpc {
    class Program {
        /// <summary>
        ///  Thrown when a condition in running the program occurs that should
        ///  cause it to terminate unsuccessfully.
        /// </summary>
        class ProgramExecutionException : Exception {
            public ProgramExecutionException(string message) : base(message) { }
        }

        /// <summary>
        /// The service we're going to talk to.
        /// </summary>
        volatile UPnPService _Service = null;

        /// <summary>
        /// Options for this Program.
        /// </summary>
        readonly Options _Options;

        /// <summary>
        ///  Prints the verbose status of the discovery to the Console iff
        ///  _Options.VerboseDiscovery is true.
        /// </summary>
        private void PrintDiscoveryStatus(string format, params object[] args) {
            if (_Options.VerboseDiscovery)
                Console.WriteLine(format, args);
        }

        /// <summary>
        /// Handles a newly discovered or added device.
        /// </summary>
        /// <param name="smartControlPoint">UPnPSmartControlPoint that discovered this device.</param>
        /// <param name="addedDevice">The device added.</param>
        protected void HandleDeviceAdded(UPnPSmartControlPoint smartControlPoint, UPnPDevice addedDevice) {
            var logPrefix = string.Format("Discovered Device \"{0}\", URN={1}, UDN={2}: ",
                addedDevice.FriendlyName, addedDevice.DeviceURN, addedDevice.UniqueDeviceName);

            if (!_Options.Matches(addedDevice)) {
                PrintDiscoveryStatus(logPrefix + "Device does not match");
                return;
            } else {
                PrintDiscoveryStatus(logPrefix + "Device matches");
            }

            foreach (UPnPService svc in addedDevice.Services)
                HandleServiceAdded(smartControlPoint, svc);
        }

        /// <summary>
        /// Handles a newly discovered or added service.
        /// </summary>
        /// <param name="smartControlPoint">UPnPSmartControlPoint that discovered this service.</param>
        /// <param name="addedService">The service added.</param>
        protected void HandleServiceAdded(UPnPSmartControlPoint smartControlPoint, UPnPService addedService) {
            var logPrefix = string.Format("Discovered service ID={0}, URN={1}: ",
                addedService.ServiceID, addedService.ServiceURN);

            if (!_Options.Matches(addedService)) {
                PrintDiscoveryStatus(logPrefix + "Does not match filter");
                return;
            } else {
                PrintDiscoveryStatus(logPrefix + "Matches filter");
            }

            var old = Interlocked.CompareExchange<UPnPService>(ref _Service, addedService, null);
            if (old != null && old != addedService)
                Console.Error.WriteLine("Ignoring duplicate service {0}, already got {1}",
                    addedService, old);
        }

        /// <summary>
        /// Dumps all arguments of an UPnPAction to Console.
        /// </summary>
        /// <param name="action">The UPnPAction whose parameters are to be dumped.</param>
        private static void DumpVars(UPnPAction action) {
            foreach (UPnPArgument arg in action.ArgumentList) {
                Console.WriteLine("var name={0} (direction={1} type={2} value=\"{3}\" isReturnValue={4}",
                    arg.Name, arg.Direction, arg.RelatedStateVar.GetNetType(), arg.DataValue, arg.IsReturnValue);
            }
        }

        /// <summary>
        /// Initializes this Program (i.e. parse Options).
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private Program(string[] args) {
            _Options = new Options(args);
        }

        /// <summary>
        /// Runs this Program.
        /// </summary>
        private void Run() {
            UPnPSmartControlPoint smartControlPoint = new UPnPSmartControlPoint(HandleDeviceAdded);
            smartControlPoint.OnAddedService += HandleServiceAdded;

            Stopwatch discoveryTimeoutStopwatch = new Stopwatch();
            Stopwatch rescanTimeoutStopwatch = new Stopwatch();

            discoveryTimeoutStopwatch.Start();
            rescanTimeoutStopwatch.Start();
            while (_Service == null && discoveryTimeoutStopwatch.ElapsedMilliseconds < _Options.DisoveryTimeoutMs) {
                if (rescanTimeoutStopwatch.ElapsedMilliseconds >= _Options.RediscoveryMs) {
                    PrintDiscoveryStatus("Rescanning");
                    smartControlPoint.Rescan();
                    rescanTimeoutStopwatch.Restart();
                }

                Thread.Sleep(100);
            }

            if (_Service == null) {
                throw new ProgramExecutionException("Service not found");
            }

            UPnPAction action = _Service.GetAction(_Options.Action);
            if (action == null)
                throw new ProgramExecutionException("Action \"" + _Options.Action + "\" not found.");

            if (_Options.DumpVars) {
                DumpVars(action);
                return;
            }

            foreach (var setVar in _Options.SetVars) {
                string name = setVar.Key;
                string value = setVar.Value;

                UPnPArgument arg = action.GetArg(name);
                if (arg == null)
                    throw new ProgramExecutionException("Variable \"" + name + "\" not found.");
                arg.DataValue = Convert.ChangeType(value, arg.RelatedStateVar.GetNetType());
            }

            _Service.InvokeSync(action.Name, action.ArgumentList);

            foreach (var name in _Options.GetVars) {
                UPnPArgument arg = action.GetArg(name);
                if (arg == null)
                    throw new ProgramExecutionException("Variable \"" + name + "\" not found.");
                Console.WriteLine(arg.DataValue);
            }

            foreach (var expectVar in _Options.ExpectVars) {
                string name = expectVar.Key, value = expectVar.Value;
                UPnPArgument arg = action.GetArg(name);
                if (arg == null)
                    throw new ProgramExecutionException("Variable \"" + name + "\" not found.");
                if (!value.Equals(arg.DataValue))
                    throw new ProgramExecutionException(String.Format(
                        "Variable \"{0}\" expected to be \"{1}\", but is actually \"{2}\".",
                        name, value, arg.DataValue));
            }

        }

        /// <summary>
        /// Constructs and runs a new Program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>0 on success, 1 on error.</returns>
        static int Main(string[] args) {
            int rc = 1;
            try {
                new Program(args).Run();
                rc = 0;
            } catch (Options.UsageException ue) {
                Console.Error.WriteLine("Error: {0}\nUsage: {1}",
                    ue.Message, Options.Help());
            } catch (ProgramExecutionException pee) {
                Console.Error.WriteLine("Error during execution: {0}", pee.Message);
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
            }

            return rc;
        }
    }
}
