using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BetterLC
{
    class Program
    {
        static void Main(string[] args)
        {
            log("Better LC 1.0");
            log("-------------");
            log("");


            var checkedArgs = args; //We actually prefer comma delimited values as they always work.
            #region Help and intitial arg count check
            if (args.Length < 6)
            {
                if (args.Any(a => a.Contains("/?")))
                {
                    log("Syntax");
                    log("------");
                    
                    log("arg 1 = Path to LC.exe in SDK Tools");
                    log("arg 2 = target assembly");
                    log("arg 3 = license.licx path");
                    log("arg 4 = outdir (normally\"obj\\Debug\\\"");
                    log("arg 5 = Build Architecture");
                    log("arg 6 = dependency resolve path(s)");
             
                    return;
                }
                else if (args.Length == 1 && args[0].Split(',').Length > 5)
                {
                    checkedArgs = args[0].Split(',').Select(s => s.Replace(",", "").Replace("\"", "")).ToArray();

                }
                else
                {
                    log($"Invalid number of arguments ({args.Length}):");
                    args.ToList().ForEach(a => log($"    {a}"));
                    Environment.Exit(1);
                    return;
                }
            }

            #endregion

            var toolsPath = checkedArgs[0];
            var targetAssembly = checkedArgs[1];
            var licxPath = checkedArgs[2];
            var outdir = checkedArgs[3];
            var architecture = checkedArgs[4];
            var resolvePaths = checkedArgs.ToList().GetRange(5, checkedArgs.Length -5);

            #region Argument Checking

            if (!File.Exists(toolsPath))
            {
                log("Tools path (argument 1) is invalid");
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrWhiteSpace(targetAssembly))
            {
                log("Target assembly (argument 2) is invalid");
                Environment.Exit(1);
                return;
            }

            if (!File.Exists(licxPath))
            {
                log("Licx file (argument 3) is invalid");
                Environment.Exit(1);
                return;
            }

            if (!Directory.Exists(outdir))
            {
                log("output directory (argument 4) is invalid");
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrWhiteSpace(architecture))
            {
                log("Architecture (argument 5) is invalid");
                Environment.Exit(1);
                return;
            }

            if (resolvePaths.Count < 1)
            {
                log("dependency resolve path(s) (argument 6) is invalid");
                Environment.Exit(1);
                return;
            }

            if (resolvePaths.Any(rp => !Directory.Exists(rp)))
            {
                log("dependency resolve path(s) (argument 6) is invalid");
                Environment.Exit(1);
                return;
            }



            #endregion


            var arguments = $"/target:{targetAssembly} /complist:{licxPath} /outdir:{outdir} ";


            #region auto-resolve dependencies

            log("Auto-Resolving dependencies...");

            var potentialDependencies = resolvePaths.SelectMany(rp => 
                Directory.GetFiles(rp, "*.dll", SearchOption.AllDirectories));

            var licxAssemblies = File.ReadAllLines(licxPath).Select(l => l.Split(',')[1].Replace(" ", "")).Distinct();

            var resolvedDependencies = potentialDependencies.Where(pd => 
                                            licxAssemblies.Any(la => pd.Contains(la))).ToList();
            
            if (licxAssemblies.Count() > resolvedDependencies.Count())
            {
                log("ERROR: Could not find all referenced dependencies.");
                log("       Please make sure all required dependencies exist in the specified resolve path(s)");

                var unresolved = resolvedDependencies.Where(pd =>
                                           !licxAssemblies.Any(la => pd.Contains(la)));

                log("       ");
                log("       Unresolved depenedencies are:");
                unresolved.ToList().ForEach(u => log($"        - {u}"));
                Environment.Exit(2);
                return;
            }

            #endregion


            resolvedDependencies.ForEach(rd => arguments += $"/i:\"{rd}\" ");


            log("Starting LC...");
            var proc = Process.Start(new ProcessStartInfo(toolsPath, arguments)
            {
                WorkingDirectory = Environment.CurrentDirectory,
            });

            proc.WaitForExit();

            if (proc.ExitCode > 0)
            {
                log($"LC Failed with Exit Code {proc.ExitCode}");
                Environment.Exit(3);
                return;
            }
            else
                log("Better LC completed successfully.");

        }





        private static void log(string s)
            => Console.WriteLine(s);
    }
}
