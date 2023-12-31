﻿

namespace Arc;
public class Relation : ArcBlock
{
    public static readonly ArcList<Relation> Relations = new();
    public Relation(Block block)
    {
        Value = block;
    }
    public static Walker Call(Walker i)
    {
        if (!i.MoveNext()) throw new Exception();

        string id = Compiler.GetId(i.Current);
        if (!i.MoveNext()) throw new Exception();
        if (i.Current != "=") throw new Exception();
        if (!i.MoveNext()) throw new Exception();
        i = Compiler.GetScope(i, out Block scope);

        scope.Prepend(new("="));
        scope.Prepend(new(id));

        Relation Relation = new(
            scope
        );

        Relations.Values.Add(Relation);

        return i;
    }
    public static string Transpile()
    {
        Block b = new()
        {
            string.Join(' ', from rel in Relations.Values select rel.Compile())
        };

        if(Compiler.TryGetVariable("hre_defines:emperor", out IVariable? emperorVar))
        {
            if (emperorVar == null) throw new Exception("No Emperor Defined");
            if (emperorVar is not ArcString) throw new Exception("Emperor of wrong type");
            string emperorKey = ((ArcString)emperorVar).Value;
            if (!Country.Countries.CanGet(emperorKey)) throw new Exception($"Emperor {emperorKey} not found as a defined country");
            Country emperor = Country.Countries[emperorKey];
            b.Add("2500.1.1", "=", "{", "emperor", "=", emperor.Tag, "}");
        }
        else
        {
            throw new Exception("Emperor does not exist");
        }

        Program.OverwriteFile($"{Program.TranspileTarget}/history/diplomacy/arc.txt", string.Join(' ', b));
        return "Relations";
    }
    public override string Compile()
    {
        return Compiler.Compile(Value);
    }
    public override string ToString() => "[Arc Relation]";
    public Walker Call(Walker i, ref List<string> result) => throw new Exception();
}
