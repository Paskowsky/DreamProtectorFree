using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Dream_Protector_Free
{
    public static class Codedom
    {
        public static bool Compile(string outPath, string language,string[] sources, string iconPath, string conditionalCompilation, string[] imports, string[] resourcePaths, string target,int netVersion)
        {

            if (!string.IsNullOrEmpty(outPath))
            {
                string netPath;
                string mscorlib;
                string system;

                switch (netVersion)
                {
                    case 20:
                        netPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"),
                  "Microsoft.NET\\Framework\\v2.0.50727\\");

                        mscorlib = Path.Combine(netPath, "mscorlib.dll");
                        system = Path.Combine(netPath, "System.dll");
                        break;
                    case 30:
                        netPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"),
                "Microsoft.NET\\Framework\\v3.0\\");

                        mscorlib = Path.Combine(netPath, "mscorlib.dll");
                        system = Path.Combine(netPath, "System.dll");
                        break;
                    case 35:
                        netPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"),
                "Microsoft.NET\\Framework\\v3.5\\");

                        mscorlib = Path.Combine(netPath, "mscorlib.dll");
                        system = Path.Combine(netPath, "System.dll");
                        break;
                    case 40:
                        netPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"),
                "Microsoft.NET\\Framework\\v4.0.30319\\");

                        mscorlib = Path.Combine(netPath, "mscorlib.dll");
                        system = Path.Combine(netPath, "System.dll");
                        break;
                    default:
                        throw new Exception();
                        
                }

              

                //var provider = new CSharpCodeProvider();

                var p = new CompilerParameters();
                var sb = new StringBuilder();

                p.OutputAssembly = outPath;

                p.ReferencedAssemblies.AddRange(new[] { mscorlib, system });
                if (imports != null && imports.Length > 0)
                {
                    foreach (string import in imports)
                    {
                        p.ReferencedAssemblies.Add(Path.Combine(netPath, import));
                    }
                    // p.ReferencedAssemblies.AddRange(imports);
                }
                p.GenerateExecutable = true;

                p.IncludeDebugInformation = false;

                string tempPath = Path.Combine(Environment.CurrentDirectory, "temp");

                Directory.CreateDirectory(tempPath);

                p.TempFiles = new TempFileCollection(tempPath,false);

                if (resourcePaths != null && resourcePaths.Length > 0)
                {
                    p.EmbeddedResources.AddRange(resourcePaths);
                }

                if (!string.IsNullOrEmpty(conditionalCompilation))
                {
                    sb.AppendFormat(" /define:{0} ", conditionalCompilation);
                }

                sb.AppendFormat(" /target:{0} ", target);


                sb.Append(" /filealign:512 /platform:x86 /optimize+ /nostdlib /unsafe");

                if ((!string.IsNullOrEmpty(iconPath)) && (File.Exists(iconPath)))
                {
                    sb.AppendFormat(" /win32icon:{0}", Convert.ToChar(34) + iconPath + Convert.ToChar(34));
                }
                
                p.CompilerOptions = sb.ToString();



                //var providerOptions = new Dictionary<string, string>();
                //providerOptions.Add("CompilerVersion", "v2.0");

                CompilerResults results = null;

                CodeDomProvider compilerProvider;

                if (language == "cs")
                {
                    compilerProvider = new CSharpCodeProvider();
                }
                else
                {
                    compilerProvider = new VBCodeProvider();
                }
                
                // = new CSharpCodeProvider();

                results = compilerProvider.CompileAssemblyFromSource(p, sources);

               // results.TempFiles.Delete();

                if (resourcePaths != null && resourcePaths.Length > 0)
                {
                    foreach (string res in resourcePaths)
                    {
                        try
                        {
                            File.Delete(res);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                // if (File.Exists("res.resources")) File.Delete("res.resources");
                if (Directory.Exists("temp")) Directory.Delete("temp", true);

               
                StringBuilder errors = new StringBuilder();

                if (results.Errors.HasErrors)
                {
                    
                        //  logger.LogText("Knight Logger Reloaded Edition encountered " + results.Errors.Count + " errors.");
                        foreach (CompilerError err in results.Errors)
                        {
                            if(!err.IsWarning)
                            errors.AppendLine(err.ToString());
                            
                            //if (UseLogger)
                            //{
                            //    Logger.Log(err.ErrorText + " At Line : " + err.Line);
                            //}
                            //log.Log(err.ErrorText + " At Line : " + err.Line);

                            
                        }
                    
#if DEBUG
                        File.WriteAllText("errors.log", errors.ToString());          
                    Process.Start("errors.log");

                    File.WriteAllText("source.cs",sources[0]);
#else

#endif           


                    return false;
                }
                else
                {
                    return File.Exists(outPath);
                }
                
                //    else
                //  {

                // }
            }
            //  else return false;//throw new ArgumentException("Please provide the output path to write to compiled executable.", "EthernalCompiler.Output = \"<path to output file here>\"");

            return false;
        }
    }
}
