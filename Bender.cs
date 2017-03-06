using System;
using System.Collections.Generic;

struct Pos
{
    public int x;
    public int y;
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    #region     Properties
    static string           output;
    static int              L, C;
    static List<List<int>>  histoGrid;  //  keep the invert/drunk/direction states for each cell
    static List<string>     grid;       //  Map
    static List<char>       blockers;
    static string           dir;        //  Bender's direction
    static Pos              p, teleport1, teleport2;
    static bool             Drunk, Invert;
    #endregion

    static void reinitHistoGrid()
    {
        histoGrid = new List<List<int>>();
        for(var l=0 ; l<L ; l++)
        {
            var line = new List<int>();
            for (var i=0 ; i<C ; i++)
                line.Add(0);
            histoGrid.Add(line);
        }
    }

    static bool registerHisto()
    {
        int state = 0;
        var filter = 0;
        if (dir == "WEST")
        {
            filter = 7;             //  000 000 000 111
            state += 1;             //  000 000 000 001
            if (Invert) state += 2; //  000 000 000 010
            if (Drunk) state += 4;  //  000 000 000 100
        }
        else if (dir == "EAST")
        {
            filter = 56;            //  000 000 111 000
            state += 8;             //  000 000 001 000
            if (Invert) state += 16;//  000 000 010 000
            if (Drunk) state += 32; //  000 000 100 000
        }
        else if (dir == "SOUTH")
        {
            filter = 448;            //  000 111 000 000
            state += 64;             //  000 001 000 000
            if (Invert) state += 128;//  000 010 000 000
            if (Drunk) state += 256; //  000 100 000 000
        }
        else if (dir == "NORTH")
        {
            filter = 3584;              //  111 000 000 000
            state += 512;               //  001 000 000 000
              if (Invert) state += 1024;//  010 000 000 000
            if (Drunk) state += 2048;   //  100 000 000 000
        }
        if ((histoGrid[p.y][p.x] & filter) == state)
            return false;

        histoGrid[p.y][p.x] |= state;
        return true;
    }
    
    static Pos getDelta(string newdir)
    {
        Pos delta;
        delta.x = 0;
        delta.y = 0;
        if     (newdir == "NORTH")  delta.y = -1;
        else if(newdir == "SOUTH")  delta.y = 1;
        else if(newdir == "EAST")   delta.x = 1;
        else if(newdir == "WEST")   delta.x = -1;
        return delta;
    }

    static bool specialCasesD(char modifier, Pos delta)
    {
        if (!Drunk)
            return false;

        switch(modifier)
        {
            case 'X':   string newLine = "";
                        newLine = grid[p.y+delta.y].Substring(0, p.x+delta.x);
                        newLine += " ";
                        newLine += grid[p.y+delta.y].Substring(p.x+delta.x+1);
                        grid[p.y+delta.y] = newLine;
                        reinitHistoGrid();
                        return true;
            case 'B':   Drunk = false;
                        return true;
        }
        return false;
    }

    static bool specialCasesI(char modifier, ref Pos delta, ref string newdir)
    {
        if (!Invert)
            return false;

        switch(modifier)
        {
            case 'X':
            case '#':   if      (!blockers.Contains(grid[p.y][p.x-1]))  newdir = "WEST";
                        else if (!blockers.Contains(grid[p.y-1][p.x]))  newdir = "NORTH";
                        else if (!blockers.Contains(grid[p.y][p.x+1]))  newdir = "EAST";
                        else                                            newdir = "SOUTH";
                        dir = newdir;
                        delta = getDelta(newdir);
                        return true;
            case 'I':   Invert = false;
                        return true;
        }
        return false;
    }

    static bool findModifierN()
    {
        string  newdir = dir;
        Pos delta = getDelta(dir);
        char modifier = grid[p.y+delta.y][p.x+delta.x];

        bool alreadyDone = specialCasesD(modifier, delta);
        if (alreadyDone)
            modifier = grid[p.y+delta.y][p.x+delta.x];
        alreadyDone |= specialCasesI(modifier, ref delta, ref newdir);
        if (!alreadyDone)
        {
            switch(modifier)
            {
                case '$': return false;
                case 'X': 
                case '#':
                        if      (!blockers.Contains(grid[p.y+1][p.x]))  dir = "SOUTH";
                        else if (!blockers.Contains(grid[p.y][p.x+1]))  dir = "EAST";
                        else if (!blockers.Contains(grid[p.y-1][p.x]))  dir = "NORTH";
                        else                                            dir = "WEST";
                        delta = getDelta(dir);
                        newdir = dir;
                        return true;
                case 'N': if (!Invert) newdir = "NORTH"; break;
                case 'S': if (!Invert) newdir = "SOUTH"; break;
                case 'E': if (!Invert) newdir = "EAST";  break;
                case 'W': if (!Invert) newdir = "WEST";  break;
                case 'B': if (!Drunk)  Drunk = true;     break;
                case 'I': if (!Invert) Invert = true;    break;
                case 'T':
                    if (teleport1.x == (p.x+delta.x)
                     && teleport1.y == (p.y+delta.y))
                    {
                        delta.x = teleport2.x-p.x;
                        delta.y = teleport2.y-p.y;
                    }
                    else
                    {
                        delta.x = teleport2.x-p.x;
                        delta.y = teleport1.y-p.y;
                    }
                    break;
            }
        }
        p.x += delta.x;
        p.y += delta.y;
        
        if (!registerHisto())
        {
            output = "LOOP";
            return false;
        }
        output += dir+"\n"; // ----------
        dir = newdir;
        return true;
    }

    static void Main(string[] args)
    {
        #region Init
        output = "";
        grid = new List<string>();
        blockers = new List<char>() {'#', 'X'};
        var inputs = Console.ReadLine().Split(' ');
        L = int.Parse(inputs[0]);
        C = int.Parse(inputs[1]);
        
        Console.Error.WriteLine("{0} {1}", L, C);

        p.x=0;
        p.y=0; 
        Drunk = false;
        Invert = false;
        dir = "SOUTH";
        for (int l = 0; l < L; l++)
        {
            string row = Console.ReadLine();
            grid.Add(row);

            //  Look for the '@'
            var aropos = row.IndexOf("@");
            if (aropos != -1)
            {
                p.x = aropos;
                p.y = l;
            }

            //  Look for the teleporters
            var Tidx = row.IndexOf("T");
            if (Tidx != -1)
            {
                if (teleport1.x==0)
                {
                    teleport1.x = Tidx;
                    teleport1.y = l;
                }
                else
                {
                    teleport2.x = Tidx;
                    teleport2.y = l;
                }
            }
        }
        reinitHistoGrid();
        #endregion

        #region Debug info
        Console.Error.WriteLine("Initial pos="+p.x+","+p.y);
        for (int l = 0; l < L; l++)
        {
            Console.Error.WriteLine(grid[l]);
        }
        #endregion

        bool exitCode = true;
        while(exitCode)
        {
            exitCode = findModifierN();
        }
        if (output != "LOOP")
            output += dir+"\n"; // ----------
        Console.WriteLine(output);
    }
}