﻿using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Rename variables from their temporary register based names to something more generic
/// </summary>
public class RenameVariablesPass : IPass
{
    public void RunOnFunction(DecompilationContext context, Function f)
    {
        var renamed = new HashSet<Identifier>();
        
        // Rename function arguments
        for (var i = 0; i < f.ParameterCount; i++)
        {
            var identifier = Identifier.GetRegister((uint)i);
            renamed.Add(identifier);
            if (f.ArgumentNames != null && f.ArgumentNames.Count > i && f.ArgumentNames[i].Name is { } n)
            {
                f.IdentifierNames[identifier] = n;
            }
            else
            {
                f.IdentifierNames[identifier] = $@"arg{i}";
            }
        }
        
        // Rename all the locals
        var localCounter = 0;
        foreach (var b in f.BlockList)
        {
            foreach (var i in b.Instructions)
            {
                if (i is Assignment a)
                {
                    var ll = 0;
                    foreach (var l in a.LeftList)
                    {
                        if (l is
                            {
                                HasIndex: false, Identifier: { IsRegister: true }
                            } && !renamed.Contains(l.Identifier))
                        {
                            renamed.Add(l.Identifier);
                            if (a.LocalAssignments != null && ll < a.LocalAssignments.Count &&
                                a.LocalAssignments[ll].Name is { } n)
                            {
                                f.IdentifierNames[l.Identifier] = n;
                            }
                            else
                            {
                                f.IdentifierNames[l.Identifier] = $@"f{f.FunctionId}_local{localCounter}";
                                localCounter++;
                            }
                        }
                        ll++;
                    }
                }
            }
        }
    }
}