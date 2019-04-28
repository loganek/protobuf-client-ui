using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;

namespace protobufclientui
{
    public class ProtobufCompiler
    {
        readonly string _outputFile;
        readonly string _generatorPath;
        readonly string _generatorArguments;

        public string ProcessOutput { get; private set; }

        public ProtobufCompiler(string filePath, string generatorPath)
        {
            string outputLocation = Path.GetTempPath();
            _generatorArguments = string.Format("--csharp_out={0} -I{1} {2}", outputLocation, Path.GetDirectoryName(filePath), filePath);
            _generatorPath = generatorPath;
            _outputFile = Path.Combine(outputLocation, Path.GetFileNameWithoutExtension(filePath) + ".cs");
        }

        public Assembly GetAssembly()
        {
            var results = Compile();

            if (results.Errors.HasErrors)
            {
                string errors = "";
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    errors += i + ": " + results.Errors[i] + "\n";
                }
                throw new Exception("Compilation failed:\n" + errors);
            }

            return results.CompiledAssembly;
        }

        CompilerResults Compile()
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();
            string protobufAssembly = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Google.Protobuf.dll");
            parameters.ReferencedAssemblies.Add(protobufAssembly);

            return provider.CompileAssemblyFromFile(parameters, new[] { GenerateCSFile() });
        }

        string FindGeneratedFile()
        {
            string directory = Path.GetDirectoryName(_outputFile);
            foreach (var file in Directory.GetFiles(directory))
            {
                if (Path.GetFileName(file).Equals(Path.GetFileName(_outputFile), StringComparison.CurrentCultureIgnoreCase))
                {
                    return file;
                }
            }

            throw new FileNotFoundException("Unable to find generated file");
        }

        internal string GenerateCSFile()
        {
            ProcessOutput = String.Empty;
            var process = new System.Diagnostics.Process();

            process.StartInfo = new System.Diagnostics.ProcessStartInfo(_generatorPath, _generatorArguments);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                ProcessOutput += process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("protoc generator failed");
            }

            return FindGeneratedFile();
        }
    }
}


