using CommandLine;

namespace ActuatorController;

public class CommandOptions
{
    [Option('s', "settings", Required = false, HelpText = "Settings file path")]
    public string? SettingsPath { get; set; }
    
    [Option('p', "port3", Required = false, HelpText = "Serial port name")]
    public string? Rcb3SerialPortName { get; set; }
    
    [Option('q', "port4", Required = false, HelpText = "Serial port name")]
    public string? Rcb4SerialPortName { get; set; }
    
    [Option('d', "decoy", Required = false, HelpText = "Decoy mode")]
    public bool DecoyMode { get; set; }
}