using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
    public static int ToLevel(this int number)
    {
        return number > 0 ? number : 1;
    }
}
