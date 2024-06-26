﻿using System.Collections;

namespace Arc;
public class ArcList<T> : IArcObject, IEnumerable, ArcEnumerable where T : IVariable
{
    public List<T?> Values { get; set; }
    public Dict<T>? dict { get; set; }
    public Func<string, Args, T>? constructor { get; set; }
    public Func<Block, T>? tConstructor { get; set; }
    public bool IsObject() => true;
    public ArcList()
    {
        Values = new();
    }
    public ArcList(Func<Block, T> Constructor)
    {
        Values = new();
        tConstructor = Constructor;
    }
    public ArcList(Dict<T> _dict)
    {
        Values = new();
        dict = _dict;
    }
    public ArcList(Block value, Func<Block, int, T> Constructor)
    {
        if (Parser.HasEnclosingBrackets(value)) Compiler.RemoveEnclosingBrackets(value);

        Values = new();

        if (value.Count == 0) return;

        Walker i = new(value);
        do
        {
            i = Compiler.GetScope(i, out Block f);
            Values.Add(Constructor(f, Values.Count));
        } while (i.MoveNext());
    }
    public ArcList(Block value, Func<string, Args, T> Constructor)
    {
        if (Parser.HasEnclosingBrackets(value)) Compiler.RemoveEnclosingBrackets(value);

        constructor = Constructor;
        Values = new();

        if (value.Count == 0) return;

        Walker i = new(value);
        do
        {
            string key = i.Current;
            i = Args.GetArgs(i, out Args args);

            Values.Add(Constructor(key, args));
        } while (i.MoveNext());
    }
    public ArcList(Block value, Func<Block, T> Constructor, bool va = true)
    {
        if (Parser.HasEnclosingBrackets(value)) Compiler.RemoveEnclosingBrackets(value);
        if (va && value.Count != 0 && !Parser.HasEnclosingBrackets(value))
        {
            value.Prepend("{");
            value.Add("}");
        }

        tConstructor = Constructor;
        Values = new();

        if (value.Count == 0) return;

        Walker i = new(value);
        do
        {
            i = Compiler.GetScope(i, out Block f);
            Values.Add(Constructor(f));
        } while (i.MoveNext());
    }
    public ArcList(Block value, Func<Args, T> Constructor, bool va = true)
    {
        if (Parser.HasEnclosingBrackets(value)) Compiler.RemoveEnclosingBrackets(value);
        if (va && value.Count != 0 && !Parser.HasEnclosingBrackets(value))
        {
            value.Prepend("{");
            value.Add("}");
        }

        tConstructor = (Block b) => {
            Args r = Args.GetArgs(b);
            return Constructor(r); 
        };
        Values = new();

        if (value.Count == 0) return;

        Walker i = new(value);
        do
        {
            i = Compiler.GetScope(i, out Block f);
            Values.Add(tConstructor(f));
        } while (i.MoveNext());
    }
    public ArcList(Block value, Dict<T> Dictionary)
    {
        if(Parser.HasEnclosingBrackets(value)) Compiler.RemoveEnclosingBrackets(value);

        dict = Dictionary;
        Values = new();

        if (value.Count == 0) return;

        Walker i = new(value);
        do
        {
            if (!Dictionary.CanGet(i.Current)) throw ArcException.Create($"{i.Current} does not exist within dictionary while creating a list", value, Dictionary);
            Values.Add((T?)Dictionary.Get(i.Current));
        } while (i.MoveNext());
    }
    public Walker Call(Walker i, ref Block result)
    {

        if (!i.MoveNext())
        {
            foreach (T? t in Values)
            {
                if (t == null) continue;
                t.Call(i, ref result);
            }
            return i;
        }

        if (i.Current != "+=")
        {
            i.ForceMoveBack();
            foreach (T? t in Values)
            {
                if (t == null) continue;
                t.Call(i, ref result);
            }
            return i;
        }
        i.ForceMoveNext();
        if(i.Current == "new")
        {
            if (constructor == null) throw ArcException.Create(i);
            i.ForceMoveNext();

            string id = Compiler.GetId(i.Current);

            i = Args.GetArgs(i, out Args args);

            Values.Add(constructor(id, args));
        }
        else if (i.Current == "{")
        {
            if (tConstructor == null) throw ArcException.Create(i);
            i.ForceMoveNext();

            i = Compiler.GetScope(i, out Block scope);

            if (Compiler.TryGetVariable(scope.ToWord(), out IVariable? var))
            {
                if (var == null) throw ArcException.Create(i, result, scope);
                else if (var is ArgList ar && ar.Get() is T va) Values.Add(va);
                else if (var is T v) Values.Add(v);
                else throw ArcException.Create(i, result, scope, var);
            }
            else
            {
                Values.Add(tConstructor(scope));
            }
            i.ForceMoveNext();
            i.Asssert("}");
        }
        else
        {
            if (dict == null) throw ArcException.Create(i);
            string s = Compiler.GetId(i.Current);
            Values.Add((T?)dict.Get(s));
        }
        return i;
    }
    public override string ToString()
    {
        return string.Join(' ', from value in Values select value.ToString());
    }
    public static Func<Block, ArcList<T>> GetConstructor(Dict<T> dict)
    {
        return (Block s) => new ArcList<T>(s, dict);
    }
    public static Func<Block, ArcList<T>> GetConstructor(Func<Block, T> func, bool va = true)
    {
        return (Block s) => new ArcList<T>(s, func, va);
    }
    public static Func<Block, ArcList<T>> GetConstructor(Func<Args, T> func)
    {
        return (Block s) => new ArcList<T>(s, func);
    }
    public static Func<Block, ArcList<T>> GetConstructor(Func<string, Args, T> func)
    {
        return (Block s) => new ArcList<T>(s, func);
    }
    public virtual IVariable? Get(string indexer)
    {
        if (indexer == "count") return new ArcInt(Values.Count);
        if(int.TryParse(indexer, out int res))
        {
            res -= 1;
            return Values[res];
        }
        throw ArcException.Create(indexer);
    }

    public virtual bool CanGet(string indexer)
    {
        if (indexer == "count") return true;
        if (int.TryParse(indexer, out int res))
        {
            res -= 1;
            if(res < 0) return false;
            if(res >= Values.Count) return false;
            return true;
        }
        return false;
    }
    public void Add(T value)
    {
        Values.Add(value);
    }
    public IEnumerator GetEnumerator() => Values.GetEnumerator();

    public IEnumerator<IVariable> GetArcEnumerator()
    {
        return (from v in Values select v as IVariable).GetEnumerator();
    }
}