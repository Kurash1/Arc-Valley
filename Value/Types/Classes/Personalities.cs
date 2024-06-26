﻿using Arc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RulerPersonality : ArcObject
{
    public static readonly Dict<RulerPersonality> RulerPersonalities = new();
    public RulerPersonality(string id) { RulerPersonalities.Add(id, this); }
    public static new Walker Call(Walker i) => Call(i, Constructor);
    public static RulerPersonality Constructor(string id, Args args) => new(id)
    {
        { "id", new ArcString($"{id}_personality") },
        { "name", args.Get(ArcString.Constructor, "name") },
        { "desc", args.Get(ArcString.Constructor, "desc", new("")) },
        { "death", args.Get(ArcString.Constructor, "death", new("")) },
        { "ruler_allow", args.Get(ArcTrigger.Constructor, "ruler_allow", new()) },
        { "heir_allow", args.Get(ArcTrigger.Constructor, "heir_allow", new()) },
        { "consort_allow", args.Get(ArcTrigger.Constructor, "consort_allow", new()) },
        { "chance", args.Get(ArcTrigger.Constructor, "chance", new()) },
        { "allow", args.Get(ArcTrigger.Constructor, "allow", new()) },
        { "war_priority", args.Get(ArcTrigger.Constructor, "war_priority", new()) },
        { "ai_rules", args.Get(ArcCode.Constructor, "ai_rules", new()) },
        { "modifier", args.Get(ArcModifier.Constructor, "modifier", new()) },
        { "nation_designer_cost", args.Get(ArcInt.Constructor, "nation_designer_cost", new(1)) },
        { "can_be_ancestor", args.Get(ArcBool.Constructor, "can_be_ancestor", new(true)) },
    };
    public override string ToString() => Get("id").ToString();
    public override Walker Call(Walker i, ref Block result) { result.Add(ToString()); return i; }
    public Block TranspileThis()
    {
        Block s = new();
        string id = ToString();
        if (Get<ArcBool>("can_be_ancestor"))
        {
            Program.Localisation.Add($"ancestor_{id}", Get("name").ToString());
            Program.Localisation.Add($"ancestor_desc_{id}", Get("desc").ToString());
            Program.Localisation.Add($"ancestor_{id}_die_desc", Get("death").ToString());
        }
        Program.Localisation.Add($"{id}", Get("name").ToString());
        Program.Localisation.Add($"desc_{id}", Get("desc").ToString());
        Program.Localisation.Add($"{id}_die_desc", Get("death").ToString());
        s.Add(id, "=", "{");
        Get<ArcTrigger>("ruler_allow").Compile("ruler_allow", ref s);
        Get<ArcTrigger>("heir_allow").Compile("heir_allow", ref s);
        Get<ArcTrigger>("consort_allow").Compile("consort_allow", ref s);
        Get<ArcTrigger>("chance").Compile("chance", ref s);
        Get<ArcTrigger>("allow").Compile("allow", ref s);
        Get<ArcTrigger>("war_priority").Compile("war_priority", ref s);
        Get<ArcCode>("ai_rules").Compile(ref s);
        Get<ArcModifier>("modifier").Compile(ref s);
        s.Add("nation_designer_cost", "=", Get("nation_designer_cost"));
        s.Add("}");
        return s;
    }
    public static string Transpile()
    {
        Block main = new();
        Block ancestor = new();
        foreach (RulerPersonality Personality in RulerPersonalities.Values())
        {
            Block transpiled = Personality.TranspileThis();
            main.Add(transpiled);
            if (Personality.Get<ArcBool>("can_be_ancestor"))
            {
                if (transpiled.First == null) throw new Exception("?");
                transpiled.First.Value.Value = $"ancestor_{transpiled.First.Value.Value}";
                ancestor.Add(transpiled);
            }
        }
        Program.OverwriteFile($"{Program.TranspileTarget}/common/ruler_personalities/arc.txt", string.Join(' ', main));
        Program.OverwriteFile($"{Program.TranspileTarget}/common/ancestor_personalities/arc.txt", string.Join(' ', ancestor));
        return "Ruler Personalities";
    }
}