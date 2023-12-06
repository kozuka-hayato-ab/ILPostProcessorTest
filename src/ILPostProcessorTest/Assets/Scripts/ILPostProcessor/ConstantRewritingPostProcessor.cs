using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Settings;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Constant.Rewriting.CodeGen
{
    public class ConstantRewritingPostProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            // Settings Assemblyのみを対象とする
            return compiledAssembly.Name == "Settings";
        }
        
        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
            {
                return null;
            }

            using var assembly = ILPostProcessUtility.AssemblyDefinitionFor(compiledAssembly);
            var diagnosticMessageList = new List<DiagnosticMessage>();
            foreach (var type in assembly.MainModule.Types)
            {
                foreach (var property in type.Properties)
                {
                    if (!property.HasCustomAttributes)
                    {
                        continue;
                    }

                    var attribute = property.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == typeof(RewritingAttribute).FullName);

                    if (attribute == null)
                    {
                        continue;
                    }

                    var constructors = type.GetConstructors();
                    foreach (var method in constructors)
                    {
                        var postProcessor = method.Body.GetILProcessor();
                        var instruction = postProcessor.Body.Instructions.FirstOrDefault(
                            inst => inst.Operand != null && inst.Operand.ToString().Contains($"<{property.Name}>k__BackingField")
                        );
                        
                        if (instruction == null)
                        {
                            continue;
                        }
                        
                        var prevInstruction = instruction.Previous;
                        if (prevInstruction.OpCode != OpCodes.Ldstr)
                        {
                            continue;
                        }
                        // 書き換え
                        prevInstruction.Operand = "Hello ILPostProcessor";
                    }
                    return ILPostProcessUtility.GetResult(assembly, diagnosticMessageList);
                }
            }
            return new ILPostProcessResult(null, diagnosticMessageList);
        }
    }
}