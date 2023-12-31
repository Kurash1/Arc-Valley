﻿namespace Arc;
public class ArcBool : IValue
{
    public bool Value { get; set; }

    public ArcBool(bool value)
    {
        Value = value;
    }
    public ArcBool(string value)
    {
        value = value.Replace("yes", "true");
        value = value.Replace("no", "false");
        Value = bool.Parse(value);
    }
    public static ArcBool Constructor(Block b) => new(b);
    public ArcBool(Block value)
    {
        if (value.Count > 1)
            throw new Exception("Too many elements given to ArcString");
        if (value.Count < 0)
            throw new Exception("Too few elements given to ArcString");
        if (value.First == null)
            throw new Exception();
        Value = new ArcBool(value.First.Value).Value;
    }
    public void Set(Block value)
    {
        if (value.Count > 1)
            throw new Exception("Too many elements given to ArcString");
        if (value.Count < 0)
            throw new Exception("Too few elements given to ArcString");
        if (value.First == null)
            throw new Exception();
        Value = new ArcBool(value.First.Value).Value;
    }
    public bool IsBool() => true;
    public override string ToString()
    {
        return Value?"yes":"no";
    }
    public static implicit operator bool(ArcBool b) => b.Value;
    public Walker Call(Walker i, ref Block result)
    {
        if (i.MoveNext())
        {
            switch (i.Current)
            {
                case ":=":
                    {
                        if (!i.MoveNext())
                            throw new Exception();

                        string k = i.Current;
                        k = k.Replace("yes", "true");
                        k = k.Replace("no", "false");
                        Value = bool.Parse(k);
                    }
                    break;

                default:
                    {
                        i.MoveBack();
                        result.Add(ToString());
                    }
                    break;
            }
        }
        else result.Add(ToString());
        return i;
    }
}