using System.Text;

namespace GNScript.Models;
public class VariableCollection
{
    public int MaxScopeLevel { get; private set; }
    private Dictionary<ScopeLevel, Dictionary<string, object>> _variables = [];

    public void SetVariable(string name, object value, int scopeLevel = 0, bool isParameterVariable = false)
    {
        if (_variables.ContainsKey(new(scopeLevel)) == false)
        {
            _variables[new(scopeLevel)] = [];
            MaxScopeLevel = scopeLevel;
        }

        if (isParameterVariable)
        {
            _variables[new(scopeLevel)][name] = value;
            return;
        }

        var foundVariable = false;
        foreach (var (_, scope) in _variables.Reverse())
        {
            if (scope.ContainsKey(name))
            {
                scope[name] = value;
                foundVariable = true;
                break;
            }
        }

        if (foundVariable == false)
            _variables[new(scopeLevel)][name] = value;
    }

    public object GetVariable(string name, int scopeLevel = 0)
    {
        for (int i = scopeLevel; i >= 0; i--)
        {
            if (_variables.TryGetValue(new(i), out var scope))
            {
                if (scope.TryGetValue(name, out var value))
                {
                    return value;
                }
            }
        }

        throw new Exception($"Variable '{name}' not found");
    }

    public Dictionary<string, object> GetVariables(int scopeLevel)
    {
        if (_variables.TryGetValue(new(scopeLevel), out var scope))
        {
            return scope;
        }

        return new();
    }

    public void ClearScope(int scopeLevel)
    {
        if (scopeLevel == 0)
            throw new Exception("Cannot clear global scope variables");

        foreach (var (level, scope) in _variables.Reverse())
        {
            if (level.Level >= scopeLevel)
                scope.Clear();
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        var nonEmptyScopes = _variables
            .Where(scope => scope.Value != null && scope.Value.Count > 0)
            .OrderBy(scope => scope.Key.Level);

        foreach (var scope in nonEmptyScopes)
        {
            sb.AppendLine($"Scope level: {scope.Key.Level}");
            foreach (var variable in scope.Value)
            {
                var executionModel = ExecutionModel.FromObject(variable.Value);
                var variableValue = executionModel.ToPrintable();

                sb.AppendLine($"  {{{variable.Key}: {variableValue}}} [{executionModel.ModelType}]");
            }
        }

        return sb.ToString();
    }

    private record ScopeLevel(int Level);
}
