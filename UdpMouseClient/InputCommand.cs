using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InputCommand
{
    public InputAction action;
    public int x;
    public int y;

    public enum InputAction
    {
        Move,
        Click,
        DblClick
    }
}