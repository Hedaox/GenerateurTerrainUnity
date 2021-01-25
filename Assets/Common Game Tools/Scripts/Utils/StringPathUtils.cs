using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringPathUtils {

    //In a string concatenated with 'separator' returns the last 'number' elements in the string
    //I.E. GetLastMembers("a.b.c.d.e", '.', 3) will return "c.d.e"
    public static string GetLastMembers(string str, char separator, int number)
    {
        string rev = ReverseString(str);
        int pos = NthOccurence(rev, '.', number);
        if (pos > 0)
            return ReverseString(rev.Substring(0, pos));
        return "";
    }

    public static int NthOccurence(string s, char t, int n)
    {
        int founds=0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == t)
                founds++;
            if (founds == n && i>0)
                return i;
        }
        return s.Length;
    }

    public static string ReverseString(string s)
    {
        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}
