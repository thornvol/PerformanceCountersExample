# PerformanceCountersExample
Sample solution for showing performance counters

Download this [zip](https://github.com/thornvol/PerformanceCountersExample/blob/master/PerformanceCountersExample.zip) that contains the solution and follow instructions below.

To run this example from PowerShell Command line, you will need to click "Build" from the menu and select "Build Solution". After the build completes, take the 2 DLLS (outlined below in **Figure 1**) from the bin folder (bin will be found in the project directory, like "c:\MySolutionDirectory\PerformanceCounterModule\bin\debug\") in the solution and create a folder named "PerformanceCounterModule" in this directory "C:\Windows\system32\WindowsPowerShell\v1.0\Modules\". By creating this folder in this place, you will be able to call the c# cmdlet from any powershell prompt. Important note: the folder name must match the cmdlet module name (in this case, folder name" PerformanceCounterModule" is the same name as the PerformanceCounterModule.dll).

**Figure 1.**
2 Dlls you will need to copy from the bin folder:
1. PerformanceCounterHelper.dll - contains the classes to calculate metrics/counters.
2. PerformanceCounterModule.dll - contains the c# powershell cmdlet that calls the above dll - this is what you will call from powershell command line.

Example usage from a PowerShell Command Line with results:

# Command
Get-PerformanceMetrics 

# Results
List of process name with CPU Percentage
