﻿namespace Arc;
public partial class Compiler
{
    public static Walker GetScope(Walker i, out Block scope)
    {
        scope = i.GetScope();

        return i;
    }
    public static bool TryTrimOne(string value, char s, out string? newValue)
    {
        newValue = null;
        if (value == null)
            return false;
        if (value.Length < 2)
            return false;
        if (value[0] != s)
            return false;
        if (value[^1] != s)
            return false;
        newValue = value[1..^1];
        return true;
    }
    public static Block RemoveEnclosingBrackets(Block scope)
    {
        scope.RemoveFirst();
        scope.RemoveLast();
        return scope;
    }
    public static bool TryGetVariable(Word locator, out IVariable? var)
    {
        return TryGetVariable(locator, out var, new Func<string, IVariable?>(global.Get), global.CanGet);
    }
    public static bool TryGetVariable(string locator, out IVariable? var, Func<string, IVariable?> Get, Func<string, bool> CanGet)
    {
        if (locator.StartsWith("trigger_value:", "event_target:", "modifier:") || locator.Contains(' ') || locator.EnclosedBy('`'))
        {
            var = null;
            return false;
        }

        if (locator.Contains(':'))
        {
            string[] KeyLocator = locator.Split(':');
            int f = 0;
            string currentKey;
            do
            {
                currentKey = KeyLocator[f];
                if (KeyLocator.Length > f + 1)
                {
                    try
                    {
                        IVariable? v = Get(currentKey) ?? throw ArcException.Create(currentKey, locator, Get, CanGet);
                        if (v is IArcObject n)
                        {
                            Get = n.Get;
                            CanGet = n.CanGet;
                        }
                    }
                    catch
                    {
                        throw ArcException.Create(currentKey, locator, Get, CanGet);
                    }
                }
                else
                {
                    if (CanGet(currentKey))
                    {
                        var = Get(currentKey);
                        return true;
                    }
                    else
                    {
                        var = null;
                        return false;
                    }
                }
                f++;
            } while (KeyLocator.Length > f);
            if (!CanGet(currentKey))
            {
                var = null;
                return false;
            }
            var = Get(currentKey);
            return true;
        }
        else
        {
            if (!CanGet(locator))
            {
                var = null;
                return false;
            }
            var = Get(locator);
            return true;
        }
    }
}