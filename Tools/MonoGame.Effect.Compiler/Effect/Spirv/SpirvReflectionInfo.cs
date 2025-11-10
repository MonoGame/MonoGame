// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    internal class SpirvReflectionInfo
    {
        public ReadOnlyCollection<SpirvVariable> Variables { get; init; }
        public string EntryPoint { get; init; }
        public ReadOnlyCollection<SpirvVariable> Input { get; init; }
        public ReadOnlyCollection<SpirvVariable> Output { get; init; }
        public ReadOnlyCollection<SpirvSampledImage> SampledImages { get; init; }

        internal class SpirvParseContext
        {
            internal Dictionary<string, string> Names = [];
            internal Dictionary<string, Dictionary<int, string>> MemberNames = [];
            internal Dictionary<string, SpirvTypeBase> Types = [];
            internal Dictionary<string, SpirvVariable> Variables = [];
            internal Dictionary<string, SpirvConstant> Constants = [];
            internal Dictionary<string, SpirvLoad> Loads = [];
            internal List<SpirvSampledImage> SampledImages = [];
            internal List<(string id, SpirvDecoration decoration)> Decorations = [];
            internal List<(string id, int index, SpirvDecoration decoration)> MemberDecorations = [];
            internal string[] EntryPoint = null;
        }

        internal static SpirvReflectionInfo Parse(string[] spirvFileLines)
        {
            SpirvParseContext context = new();

            // First grab everything out of the spirv that we care about
            for (int lineIdx = 0; lineIdx < spirvFileLines.Length; lineIdx++)
            {
                string[] parts = spirvFileLines[lineIdx].Trim().Split(' ');

                if (parts[0].StartsWith('%'))
                {
                    if (parts[2].StartsWith("OpType"))
                    {
                        SpirvTypeBase newType = SpirvTypeBase.ParseType(parts, context);
                        if (newType != null) context.Types.Add(newType.Id, newType);
                    }
                    else if (parts[2] == "OpVariable")
                    {
                        SpirvVariable newVar = SpirvVariable.ParseVariable(parts, context);
                        context.Variables.Add(newVar.Id, newVar);
                    }
                    else if (parts[2] == "OpConstant")
                    {
                        SpirvConstant newConst = SpirvConstant.ParseConstant(parts, context);
                        context.Constants.Add(newConst.Id, newConst);
                    }
                    else if (parts[2] == "OpLoad")
                    {
                        SpirvLoad load = SpirvLoad.ParseLoad(parts, context);
                        if (load != null) context.Loads.Add(load.Id, load);
                    }
                    else if (parts[2] == "OpSampledImage")
                    {
                        SpirvSampledImage sampledImage = SpirvSampledImage.ParseSampledImage(parts, context);
                        if (sampledImage != null) context.SampledImages.Add(sampledImage);
                    }
                }
                else if (parts[0] == "OpName")
                {
                    string name = parts[2].Trim(['\"']);
                    context.Names.Add(parts[1], name);
                }
                else if (parts[0] == "OpMemberName")
                {
                    if (!context.MemberNames.TryGetValue(parts[1], out Dictionary<int, string> members))
                    {
                        members = [];
                        context.MemberNames.Add(parts[1], members);
                    }

                    members.Add(int.Parse(parts[2]), parts[3].Trim('\"'));
                }
                else if (parts[0] == "OpDecorate" || parts[0] == "OpDecorateString")
                {
                    string target = parts[1];
                    SpirvDecoration decoration = SpirvDecoration.ParseDecorator(parts[2..]);
                    if (decoration != null) context.Decorations.Add((target, decoration));
                }
                else if (parts[0] == "OpMemberDecorate")
                {
                    string target = parts[1];
                    int index = int.Parse(parts[2]);
                    SpirvDecoration decoration = SpirvDecoration.ParseDecorator(parts[3..]);
                    context.MemberDecorations.Add((target, index, decoration));
                }
                else if (parts[0] == "OpEntryPoint")
                {
                    // Just grab this for now, we can't fill it in until later.
                    context.EntryPoint = parts;
                }
            }

            // Then assign anything that could have been a forward ref
            foreach ((string id, SpirvDecoration decoration) in context.Decorations)
            {
                if (context.Variables.TryGetValue(id, out SpirvVariable variable))
                {
                    variable.ApplyDecoration(decoration);
                }
            }

            foreach ((string id, int index, SpirvDecoration decoration) in context.MemberDecorations)
            {
                SpirvTypeStruct targetStruct = context.Types[id] as SpirvTypeStruct;

                targetStruct.Members[index].ApplyDecoration(decoration);
            }

            string[] entryPointInputOutput = context.EntryPoint[4..];
            List<SpirvVariable> inputs = [];
            List<SpirvVariable> outputs = [];

            foreach (string id in entryPointInputOutput)
            {
                SpirvVariable variable = context.Variables[id];

                if (variable.StorageClass == StorageClass.Input)
                {
                    inputs.Add(variable);
                }
                else if (variable.StorageClass == StorageClass.Output)
                {
                    outputs.Add(variable);
                }
            }

            return new SpirvReflectionInfo
            {
                EntryPoint = context.EntryPoint[3].Trim('\"'),
                Variables = context.Variables.Values.ToList().AsReadOnly(),
                Input = inputs.AsReadOnly(),
                Output = outputs.AsReadOnly(),
                SampledImages = context.SampledImages.AsReadOnly(),
            };
        }
    }
}
