using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
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
                // Settings.ProjectDefine型を探す
                if (type.FullName != "Settings.ProjectDefine")
                {
                    continue;
                }

                // Settings.ProjectDefineのコンストラクタを取得する
                var methodDefinition = type.Methods.FirstOrDefault((method) => method.Name == ".ctor");
                
                if (methodDefinition == null)
                {
                    diagnosticMessageList.Add(new DiagnosticMessage()
                    {
                        MessageData = ".ctorが見つかりませんでした。",
                        DiagnosticType = DiagnosticType.Warning
                    });
                    continue;
                }

                var postProcessor = methodDefinition.Body.GetILProcessor();

                for (int i = 0; i < postProcessor.Body.Instructions.Count; i++)
                {
                    var instruction = postProcessor.Body.Instructions[i];
                    // 命令のオペコードがStfldでない場合はスキップ
                    if (instruction.OpCode != OpCodes.Stfld)
                    {
                        continue;
                    }
                    // 命令のオペランドにValueが含まれていない場合はスキップ
                    if (!instruction.Operand.ToString().Contains($"<{nameof(ProjectDefine.Value)}>k__BackingField"))
                    {
                        continue;
                    }
                    
                    //　含まれている場合、一つ前の命令を取得し、オペコードがLdstrでない場合はスキップ
                    var prevInstruction = postProcessor.Body.Instructions[i - 1];
                    if (prevInstruction.OpCode != OpCodes.Ldstr)
                    {
                        continue;
                    }
                    // 書き換え
                    prevInstruction.Operand = "Hello ILPostProcessor";
                }

                return ILPostProcessUtility.GetResult(assembly, diagnosticMessageList);
            }
            return new ILPostProcessResult(null, diagnosticMessageList);
        }
    }
}