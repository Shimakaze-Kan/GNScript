using GNScript.Helpers;

namespace GNScript.Models;
public class InterpreterRuntimeState
{
    public VariableCollection Variables { get; set; }
    public Dictionary<FunctionDictionaryKey, FunctionNode> Functions { get; set; }
    public Dictionary<string, RefBoxNode> RefBoxDefinitions { get; set; }
    public Stack<CallReturnValue> CallReturnValues { get; set; }
    public Dictionary<UserDefinedExtensionKey, UserDefinedExtension> UserDefinedExtensions { get; set; }
    public int ScopeLevel { get; set; }
    public bool IsForLoopParameterSection { get; set; }

    public void Combine(InterpreterRuntimeState state)
    {
        var maxStateLevel = state.Variables.MaxScopeLevel;
        for (int i = 0; i <= maxStateLevel; i++)
        {
            var scopeVariables = state.Variables.GetVariables(i);
            foreach (var variable in scopeVariables)
            {
                Variables.SetVariable(variable.Key, variable.Value, i);
            }
        }

        Functions.CombineDictionaries(state.Functions);
        RefBoxDefinitions.CombineDictionaries(state.RefBoxDefinitions);
        CallReturnValues = new(CallReturnValues.Concat(state.CallReturnValues.Reverse()));
        UserDefinedExtensions.CombineDictionaries(state.UserDefinedExtensions);
        ScopeLevel = state.ScopeLevel;
        IsForLoopParameterSection = state.IsForLoopParameterSection;
    }
}
