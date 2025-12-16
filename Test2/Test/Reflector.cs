// <copyright file="Reflector.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Test;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// reflector.
/// </summary>
public class Reflector
{
    /// <summary>
    /// to print structure of class.
    /// </summary>
    /// <param name="someClass">class to print.</param>
    public void PrintStructure(Type someClass)
    {
        ArgumentNullException.ThrowIfNull(someClass);

        var className = someClass.Name;
        var fileName = $"{className}.cs";

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("using System;");
        stringBuilder.AppendLine("using System.Collections.Generic;");
        stringBuilder.AppendLine();

        var classModifiers = this.GetTypeModifiers(someClass);

        stringBuilder.AppendLine($"{classModifiers}class {this.GetGenericTypeName(someClass)}");
        stringBuilder.AppendLine("{");

        var fields = someClass.GetFields();
        foreach (var field in fields)
        {
            stringBuilder.AppendLine($"    {this.GetFieldDeclaration(field)}");
        }

        if (fields.Length > 0)
        {
            stringBuilder.AppendLine();
        }

        var properties = someClass.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.Instance | BindingFlags.Static);
        foreach (var property in properties)
        {
            stringBuilder.AppendLine($"    {this.GetPropertyDeclaration(property)}");
        }

        if (properties.Length > 0)
        {
            stringBuilder.AppendLine();
        }

        var methods = someClass.GetMethods().Where(m => !m.IsSpecialName);

        foreach (var method in methods)
        {
            stringBuilder.AppendLine($"    {this.GetMethodDeclaration(method)}");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine($"        {this.GetMethodBody(method)}");
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine();
        }

        var nestedClasses = someClass.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var nestedType in nestedClasses)
        {
            if (!nestedType.IsClass)
            {
                continue;
            }

            stringBuilder.AppendLine($"    {this.GetTypeDeclaration(nestedType)}");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("}");

        File.WriteAllText(fileName, stringBuilder.ToString());
    }

    /// <summary>
    /// to print differences between classes.
    /// </summary>
    /// <param name="a">left class.</param>
    /// <param name="b">right class.</param>
    public void DiffClasses(Type a, Type b)
    {
        if (a == null || b == null)
        {
            throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));
        }

        Console.WriteLine("------- Сравнение классов -------");
        Console.WriteLine($"Класс A: {a.FullName}");
        Console.WriteLine($"Класс B: {b.FullName}");
        Console.WriteLine();

        this.CompareFields(a, b);
        Console.WriteLine();

        this.CompareProperties(a, b);
        Console.WriteLine();

        this.CompareMethods(a, b);
        Console.WriteLine();
    }

    /// <summary>
    /// to get class modifiers.
    /// </summary>
    /// <param name="type">class to get.</param>
    /// <returns>string with modifiers.</returns>
    private string GetTypeModifiers(Type type)
    {
        var result = string.Empty;

        if (type.IsPublic || type.IsNestedPublic)
        {
            result = "public ";
        }
        else if (type.IsNestedPrivate)
        {
            result = "private ";
        }
        else if (type.IsNestedFamily)
        {
            result = "protected ";
        }

        if (type.IsAbstract && type.IsSealed)
        {
            result += "static ";
        }
        else if (type.IsAbstract)
        {
            result += "abstract ";
        }
        else if (type.IsSealed)
        {
            result += "sealed ";
        }

        return result;
    }

    /// <summary>
    /// to get class name and generic type.
    /// </summary>
    /// <param name="type">class.</param>
    /// <returns>string with generic type.</returns>
    private string GetGenericTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name.Split('`')[0];
        var args = type.GetGenericArguments();
        var argNames = args.Select(this.GetGenericTypeName);

        return $"{name}<{string.Join(", ", argNames)}>";
    }

    /// <summary>
    /// to get field declaration.
    /// </summary>
    /// <param name="field">field to get declaration.</param>
    /// <returns>field modifies and name.</returns>
    private string GetFieldDeclaration(FieldInfo field)
    {
        var modifiers = new List<string>();

        if (!field.IsPublic)
        {
            if (field.IsPrivate)
            {
                modifiers.Add("private");
            }
        }
        else
        {
            modifiers.Add("public");
        }

        if (field.IsStatic)
        {
            modifiers.Add("static");
        }

        if (field.IsInitOnly)
        {
            modifiers.Add("readonly");
        }

        if (field.IsLiteral)
        {
            modifiers.Add("const");
        }

        var typeName = this.GetGenericTypeName(field.FieldType);
        return $"{string.Join(" ", modifiers)} {typeName} {field.Name};";
    }

    /// <summary>
    /// to get property declaration.
    /// </summary>
    /// <param name="property">property.</param>
    /// <returns>string with property declaration.</returns>
    private string GetPropertyDeclaration(PropertyInfo property)
    {
        var getter = property.GetGetMethod(true);
        var setter = property.GetSetMethod(true);

        var modifiers = new List<string>();

        if (getter != null)
        {
            if (getter.IsPublic)
            {
                modifiers.Add("public");
            }
            else if (getter.IsPrivate)
            {
                modifiers.Add("private");
            }
        }

        var typeName = this.GetGenericTypeName(property.PropertyType);

        var declaration = $"{string.Join(" ", modifiers)} {typeName} {property.Name}";
        declaration += " { ";

        if (getter != null)
        {
            if (getter.IsPrivate)
            {
                declaration += "private ";
            }

            declaration += "get; ";
        }

        if (setter != null)
        {
            if (setter.IsPrivate)
            {
                declaration += "private ";
            }

            declaration += "set; ";
        }

        declaration += "}";
        return declaration;
    }

    /// <summary>
    /// get method declaration.
    /// </summary>
    /// <param name="method">method to declaration.</param>
    /// <returns>string with declaration.</returns>
    private string GetMethodDeclaration(MethodInfo method)
    {
        var modifiers = method.IsPublic ? "public " :
            method.IsPrivate ? "private " :
            method.IsFamily ? "protected " : string.Empty;

        if (method.IsStatic)
        {
            modifiers += "static ";
        }

        var returnType = method.ReturnType == typeof(void) ? "void" : method.ReturnType.Name;

        var parameters = method.GetParameters()
            .Select(p => $"{p.ParameterType.Name} {p.Name}");

        return $"{modifiers}{returnType} {method.Name}({string.Join(", ", parameters)});";
    }

    /// <summary>
    /// to get method body.
    /// </summary>
    /// <param name="method">method.</param>
    /// <returns>body.</returns>
    private string GetMethodBody(MethodInfo method)
    {
        if (method.ReturnType == typeof(void))
        {
            return "return;";
        }
        else if (method.ReturnType.IsValueType)
        {
            return $"return default({this.GetGenericTypeName(method.ReturnType)});";
        }

        return "return null;";
    }

    /// <summary>
    /// to get class declaration.
    /// </summary>
    /// <param name="type">type to declare.</param>
    /// <returns>string with type declaration.</returns>
    private string GetTypeDeclaration(Type type)
    {
        return $"{this.GetTypeModifiers(type)}class {type.Name}";
    }

    /// <summary>
    /// to compare fields.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">b.</param>
    private void CompareFields(Type a, Type b)
    {
        var fieldsA = a.GetFields();
        var fieldsB = b.GetFields();

        Console.WriteLine("------ Поля ------");

        var allNames = fieldsA.Select(f => f.Name)
                             .Concat(fieldsB.Select(f => f.Name))
                             .Distinct()
                             .OrderBy(n => n);

        foreach (var name in allNames)
        {
            var fieldA = fieldsA.FirstOrDefault(f => f.Name == name);
            var fieldB = fieldsB.FirstOrDefault(f => f.Name == name);

            if (fieldA == null)
            {
                if (fieldB != null)
                {
                    Console.WriteLine($"Только в {b.Name}: {this.GetFieldInfo(fieldB)}");
                }
            }
            else if (fieldB == null)
            {
                Console.WriteLine($"Только в {a.Name}: {this.GetFieldInfo(fieldA)}");
            }
            else if (!this.AreFieldsSame(fieldA, fieldB))
            {
                Console.WriteLine($"Разное поле '{name}':");
                Console.WriteLine($"  {a.Name}: {this.GetFieldInfo(fieldA)}");
                Console.WriteLine($"  {b.Name}: {this.GetFieldInfo(fieldB)}");
            }
        }
    }

    /// <summary>
    /// get field info.
    /// </summary>
    /// <param name="field">field.</param>
    /// <returns>info.</returns>
    private string GetFieldInfo(FieldInfo field)
    {
        var access = field.IsPublic ? "public" : "private";
        return $"{access} {field.FieldType.Name} {field.Name}";
    }

    /// <summary>
    /// are fields same.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">b.</param>
    /// <returns>true if same.</returns>
    private bool AreFieldsSame(FieldInfo a, FieldInfo b)
    {
        return a.FieldType == b.FieldType && a.IsPublic == b.IsPublic;
    }

    /// <summary>
    /// to compare properties.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">b.</param>
    private void CompareProperties(Type a, Type b)
    {
        var propsA = a.GetProperties();
        var propsB = b.GetProperties();

        Console.WriteLine("------- Свойства --------");

        var allNames = propsA.Select(p => p.Name)
                           .Concat(propsB.Select(p => p.Name))
                           .Distinct()
                           .OrderBy(n => n);

        foreach (var name in allNames)
        {
            var propA = propsA.FirstOrDefault(p => p.Name == name);
            var propB = propsB.FirstOrDefault(p => p.Name == name);

            if (propA == null)
            {
                Console.WriteLine($"Только в {b.Name}: {propB?.PropertyType.Name} {name}");
            }
            else if (propB == null)
            {
                Console.WriteLine($"Только в {a.Name}: {propA.PropertyType.Name} {name}");
            }
            else if (propA.PropertyType != propB.PropertyType)
            {
                Console.WriteLine($"Разные типы свойства '{name}':");
                Console.WriteLine($"  {a.Name}: {propA.PropertyType.Name}");
                Console.WriteLine($"  {b.Name}: {propB.PropertyType.Name}");
            }
        }
    }

    /// <summary>
    /// to compare methods.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">b.</param>
    private void CompareMethods(Type a, Type b)
    {
        var methodsA = a.GetMethods().Where(m => !m.IsSpecialName).ToList();
        var methodsB = b.GetMethods().Where(m => !m.IsSpecialName).ToList();

        Console.WriteLine("-------- Методы ---------");

        var methodsDict = new Dictionary<string, (MethodInfo A, MethodInfo B)>();

        foreach (var method in methodsA)
        {
            var key = $"{method.Name}({method.GetParameters().Length})";
            methodsDict[key] = (method, null)!;
        }

        foreach (var method in methodsB)
        {
            var key = $"{method.Name}({method.GetParameters().Length})";

            if (methodsDict.ContainsKey(key))
            {
                var existing = methodsDict[key];
                methodsDict[key] = (existing.A, method);
            }
            else
            {
                Console.WriteLine($"Только в {b.Name}: {this.GetMethodInfo(method)}");
            }
        }

        foreach (var kvp in methodsDict)
        {
            if (!this.AreMethodsSame(kvp.Value.A, kvp.Value.B))
            {
                Console.WriteLine($"Разный метод '{kvp.Value.A.Name}':");
                Console.WriteLine($"  {a.Name}: {this.GetMethodInfo(kvp.Value.A)}");
                Console.WriteLine($"  {b.Name}: {this.GetMethodInfo(kvp.Value.B)}");
            }
        }
    }

    /// <summary>
    /// to get method info.
    /// </summary>
    /// <param name="method">.</param>
    /// <returns>string.</returns>
    private string GetMethodInfo(MethodInfo method)
    {
        var access = method.IsPublic ? "public" : "private";
        return $"{access} {method.ReturnType.Name} {method.Name}()";
    }

    /// <summary>
    /// are methods same.
    /// </summary>
    /// <param name="a">.</param>
    /// <param name="b">b.</param>
    /// <returns>true is same.</returns>
    private bool AreMethodsSame(MethodInfo a, MethodInfo b)
    {
        return a.ReturnType == b.ReturnType && a.IsPublic == b.IsPublic;
    }
}