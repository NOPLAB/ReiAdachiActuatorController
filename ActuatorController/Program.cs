using ActuatorController.ControlTask;
using ActuatorController.Driver;
using ActuatorController.Settings;
using ActuatorController.Stream;
using CommandLine;

namespace ActuatorController;

internal static class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Service Start!");

        await Parser.Default.ParseArguments<CommandOptions>(args)
            .WithNotParsed(e => { Console.WriteLine("Options can't parse." + e); })
            .WithParsedAsync(ops =>
            {
                var settingsPath = ops.SettingsPath ?? "settings.json";

                var settings = new SettingsWrapper(settingsPath);
                var binder = new ActuatorsStatus(settings);

                IDriver driver;

                if (ops.DecoyMode)
                {
                    driver = new DecoyDriver();
                }
                else
                {
                    driver =
                        Rcb3Rcb4Driver.Connect3And4(
                            ops.Rcb3SerialPortName ??
                            throw new ArgumentException("Required port3 if program wasn't started DecoyMode"),
                            ops.Rcb4SerialPortName ??
                            throw new ArgumentException("Required port4 if program wasn't started DecoyMode"));
                }

                var taskManager = new TaskManager(settings, driver, binder);

                var oscHandler = new OscHandler(9090);
                IControlTask updater = new OscToActuatorController(oscHandler, binder);
                var token = new CancellationTokenSource();
                _ = taskManager.CreateControlTask(updater, token.Token);

                Console.ReadKey();
                
                token.Cancel();

                return Task.CompletedTask;
            });
    }
}