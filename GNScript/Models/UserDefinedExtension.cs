namespace GNScript.Models;
public record UserDefinedExtension(string Type, List<string> FunctionParameterNames, AstNode FunctionBody);
